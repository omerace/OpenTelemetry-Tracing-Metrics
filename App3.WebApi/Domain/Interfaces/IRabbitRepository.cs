using App3.WebApi.Events;

namespace App3.WebApi.Domain.Interfaces
{
    public interface IRabbitRepository
    {
        void Publish(IEvent evt);
    }
}
