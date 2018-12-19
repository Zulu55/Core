namespace Core4.Data
{
    using Entities;
    using Models;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRepository
    {
        Task AddProductAsync(Product product, string userName);

        Product GetProduct(int id);

        IEnumerable<Product> GetProducts();

        bool ProductExists(int id);

        void RemoveProduct(Product product);

        Task<bool> SaveAllAsync();

        void UpdateProduct(Product product);

        Task<bool> ConfirmOrderAsync(string userName);

        Task<IEnumerable<Order>> GetOrdersAsync(string userName);

        Task<IEnumerable<OrderDetailTemp>> GetDetailTempsAsync(string userName);

        Task DeleteDetailTempAsync(int id);

        Task<OrderDetailTemp> GetDetailTempAsync(int id);

        IEnumerable<SelectListItem> GetComboProducts();

        Task AddItemToOrderAsync(AddItemViewModel model, string userName);

        Task ModifyOrderDetailTempQuantityAsync(int id, double quantity);
    }
}