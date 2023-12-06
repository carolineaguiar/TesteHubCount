using OfficeOpenXml;
using System.Drawing;
using TesteHubCount.Utils;
using TesteHubCount.ValueObject;
using TesteHubCount.ValueObjects;

namespace TesteHubCount.Services
{
    public class OrderService : IOrderService
    {
        private readonly ISearchAddress _searchAddress;
        public OrderService(ISearchAddress searchAddress)
        {
            _searchAddress = searchAddress;
        }

        #region Public Methods
        public async Task<List<OrderViewModelSaida>> ProcessOrderFile(IFormFile file)
        {
            var importData = await ImportFile(file);
            var ordersData = new List<OrderViewModelSaida>();

            foreach (var item in importData)
            {
                var order = await ProcessOrderData(item);
                ordersData.Add(order);
            }

            return ordersData;
        }

        #endregion

        #region Private Methods
        private async Task<OrderViewModelSaida> ProcessOrderData(Order order)
        {

            var addressInfo = await _searchAddress.SearchAddressZipCode(order.CEP);
            var region = ObterRegiaoPorUF(addressInfo.Uf);
            if (addressInfo.Uf == "SP" && addressInfo.Localidade == "São Paulo")
                region = Regiao.SaoPauloCapital;
            
            var deliveryDate = AddBusinessDays(order.Data, ObterPrazoFretePorRegiao(region));
            if (region.ToString() == "Sudeste")
                deliveryDate = order.Data.AddDays(1);

            if (Enum.TryParse<EnumPriceProduct>(order.Produto, out var produtoEnum))
            {

                var priceProduct = (int)produtoEnum;
                var frete = CalcularFrete(priceProduct, region);

                var orderData = new OrderViewModelSaida()
                {
                    ClientName = order.RazaoSocial,
                    Product = order.Produto,
                    CountryRegion = region.ToString(),
                    DeliveryDate = deliveryDate,
                    FinalPrice = (priceProduct + frete).ToString()
                };

                return orderData;
            }
            else
            {
                throw new ArgumentException("Produto não reconhecido", nameof(order.Produto));
            }
        }


        private async Task<List<Order>> ImportFile(IFormFile file)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];

                    var orders = new List<Order>();

                    for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                    {
                        if (string.IsNullOrEmpty(worksheet.Cells[row, 1].Text))
                        {
                            continue;
                        }
                        var order = new Order
                        {
                            Documento = worksheet.Cells[row, 1].Text,
                            RazaoSocial = worksheet.Cells[row, 2].Text,
                            CEP = worksheet.Cells[row, 3].Text,
                            Produto = worksheet.Cells[row, 4].Text,
                            NumeroDoPedido = Convert.ToInt32(worksheet.Cells[row, 5].Text),
                            Data = DateTime.Parse(worksheet.Cells[row, 6].Text),
                        };

                        orders.Add(order);
                    }

                    return orders;
                }
            }
        }

        public static DateTime AddBusinessDays(DateTime startDate, int businessDays)
        {

            int currentDayOfWeek = (int)startDate.DayOfWeek;
            int remainingDays = businessDays;

            while (remainingDays > 0)
            {
                startDate = startDate.AddDays(1);

                if (startDate.DayOfWeek != DayOfWeek.Saturday && startDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    remainingDays--;
                }
            }

            return startDate;
        }

        private Regiao ObterRegiaoPorUF(string uf)
        {
            switch (uf)
            {
                case "AC":
                case "AP":
                case "AM":
                case "RO":
                case "RR":
                case "TO":
                    return Regiao.Norte;
                case "AL":
                case "BA":
                case "CE":
                case "MA":
                case "PA":
                case "PB":
                case "PE":
                case "PI":
                case "RN":
                case "SE":
                    return Regiao.Nordeste;
                case "DF":
                case "GO":
                case "MT":
                case "MS":
                    return Regiao.CentroOeste;
                case "ES":
                case "MG":
                case "RJ":
                case "SP":
                    return Regiao.Sudeste;
                case "PR":
                case "RS":
                case "SC":
                    return Regiao.Sul;
                default:
                    throw new ArgumentException("UF não reconhecido", nameof(uf));
            }
        }

        private int ObterPrazoFretePorRegiao(Regiao regiao)
        {
            switch (regiao)
            {
                case Regiao.Norte:
                case Regiao.Nordeste:
                    return 10;
                case Regiao.CentroOeste:
                case Regiao.Sul:
                    return 5;
                case Regiao.Sudeste:
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(regiao), regiao, "Região não reconhecida");
            }
        }

        private decimal CalcularFrete(decimal valorProduto, Regiao regiao)
        {
            switch (regiao)
            {
                case Regiao.Norte:
                case Regiao.Nordeste:
                    return valorProduto * 0.3m;
                case Regiao.CentroOeste:
                case Regiao.Sul:
                    return valorProduto * 0.2m;
                case Regiao.Sudeste:
                    return valorProduto * 0.1m;
                case Regiao.SaoPauloCapital:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(regiao), regiao, "Região não reconhecida");
            }
        }

        #endregion
    }
}

