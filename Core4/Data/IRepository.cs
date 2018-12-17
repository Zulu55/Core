using System.Collections.Generic;
using System.Threading.Tasks;
using Core4.Data.Entities;

namespace Core4.Data
{
    public interface IRepository
    {
        Task AddProductAsync(Product product, string userName);
        Product GetProduct(int id);
        IEnumerable<Product> GetProducts();
        bool ProductExists(int id);
        void RemoveProduct(Product product);
        Task<bool> SaveAllAsync();
        void UpdateProduct(Product product);
    }
}