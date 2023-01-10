using App3.WebApi.Domain.Interfaces;
using App3.WebApi.Domain.Models;
using App3.WebApi.Infrastructure.Context;
using App3.WebApi.Infrastructure.Metrics;
using System.Threading.Tasks;

namespace App3.WebApi.Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly OtelMetrics _meters;

        public CategoryRepository(BookStoreDbContext context, 
            OtelMetrics meters) : base(context)
        {
            _meters = meters;
        }

        public override async Task Add(Category entity)
        {
            await base.Add(entity);
            _meters.AddCategory();
            _meters.IncreaseTotalCategories();
        }

        public override async Task Update(Category entity)
        {
            await base.Update(entity);
            _meters.UpdateCategory();
        }

        public override async Task Remove(Category entity)
        {
            await base.Remove(entity);
            _meters.DeleteCategory();
            _meters.DecreaseTotalCategories();
        }


    }
}