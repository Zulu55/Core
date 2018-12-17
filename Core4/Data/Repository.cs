namespace Core4.Data
{
    using Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Repository : IRepository
    {
        private readonly DataContext context;
        private readonly UserManager<User> userManager;

        public Repository(DataContext context, UserManager<User> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public IEnumerable<Product> GetProducts()
        {
            return this.context.Products
                .Include(p => p.User)
                .OrderBy(p => p.Name);
        }

        public Product GetProduct(int id)
        {
            return this.context.Products.Find(id);
        }

        public async Task AddProductAsync(Product product, string userName)
        {
            var user = await this.userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return;
            }

            product.User = user;
            this.context.Products.Add(product);
        }

        public void UpdateProduct(Product product)
        {
            this.context.Update(product);
        }

        public void RemoveProduct(Product product)
        {
            this.context.Products.Remove(product);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await this.context.SaveChangesAsync() > 0;
        }

        public bool ProductExists(int id)
        {
            return this.context.Products.Any(p => p.Id == id);
        }
    }

}
