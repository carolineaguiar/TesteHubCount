using Newtonsoft.Json;
using TesteHubCount.Models;

namespace TesteHubCount.Utils
{
    public class  SearchAddress : ISearchAddress
    {
        private readonly HttpClient _httpClient;
        private const string ViaCepBaseUrl = "https://viacep.com.br/ws/";

        public SearchAddress()
        {
            _httpClient = new HttpClient();
        }

        public async Task<Address> SearchAddressZipCode(string zipCode)
        {
            try
            {
                var url = $"{ViaCepBaseUrl}{zipCode}/json";
                var response = await _httpClient.GetStringAsync(url);

                if (!string.IsNullOrEmpty(response))
                {
                    var addressInfo = JsonConvert.DeserializeObject<Address>(response);
                    return addressInfo;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao consultar o endereço: {ex.Message}");
            }

            return null;
        }
    }
}

