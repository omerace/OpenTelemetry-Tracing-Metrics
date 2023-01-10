using System.Collections.Generic;

namespace App3.WebApi.Domain.Models
{
    public class Category : Entity
    {
        public string Name { get; set; }
        
        public IEnumerable<Book> Books { get; set; }
    }
}