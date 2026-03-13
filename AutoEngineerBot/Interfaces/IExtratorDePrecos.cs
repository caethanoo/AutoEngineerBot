using System.Threading.Tasks;
using AutoEngineerBot.Models;

namespace AutoEngineerBot.Interfaces
{
    public interface IExtratorDePrecos
    {
        Task<Produto> ExtrairAsync(string url);
    }
}