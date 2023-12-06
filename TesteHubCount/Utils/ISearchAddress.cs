using TesteHubCount.Models;

namespace TesteHubCount.Utils
{
    public interface ISearchAddress
    {
        Task<Address> SearchAddressZipCode(string zipCode);
    }
}
