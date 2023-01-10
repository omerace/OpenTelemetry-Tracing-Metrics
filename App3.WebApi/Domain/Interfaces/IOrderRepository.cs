using App3.WebApi.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App3.WebApi.Domain.Interfaces
{
    public interface IOrderRepository: IRepository<Order>
    {
        Task<List<Order>> GetOrdersByBookId(int bookId);
    }
}
