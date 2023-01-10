using System;
using App3.WebApi.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App3.WebApi.Domain.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAll();
        Task<Order> GetById(int id);
        Task<Order> Add(Order order); 
        Task<Order> Remove(Order order);
    }
}
