using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using TesteHubCount.ValueObjects;

public interface IOrderService
{
    Task<List<OrderViewModelSaida>> ProcessOrderFile(IFormFile file);
}