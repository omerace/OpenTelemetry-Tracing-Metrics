using System.Threading.Tasks;

namespace App3.WebApi.Domain.Interfaces
{
    public interface ISqlRepository
    {
        Task Persist(string message);
    }
}