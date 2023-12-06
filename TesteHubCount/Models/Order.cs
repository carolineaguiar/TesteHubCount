namespace TesteHubCount.ValueObjects
{
    public class Order
    {
        public string Documento { get; set; }
        public string RazaoSocial { get; set; }
        public string CEP { get; set; }
        public string Produto { get; set; }
        public int NumeroDoPedido { get; set; }
        public DateTime Data { get; set; }
    }
}
