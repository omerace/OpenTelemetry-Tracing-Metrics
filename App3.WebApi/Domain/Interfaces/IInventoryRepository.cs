using App3.WebApi.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App3.WebApi.Domain.Interfaces
{
    public interface IInventoryRepository : IRepository<Inventory>
    {
        Task<IEnumerable<Inventory>> SearchInventoryForBook(string bookName);
    }
}