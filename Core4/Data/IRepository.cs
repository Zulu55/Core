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

        Task<City> GetCityAsync(int id);

        Task<IEnumerable<Order>> GetOrdersAsync(string userName);

        Task<IEnumerable<Country>> GetCountriesAsync();

        Task<Order> GetOrdersAsync(int id);

        Task<int> DeleteCityAsync(City city);

        Task DeleteOrderAsync(int id);

        Task<IEnumerable<OrderDetailTemp>> GetDetailTempsAsync(string userName);

        Task<Country> GetCountryAsync(int id);

        Task<int> UpdateCity(City city);

        Task DeleteDetailTempAsync(int id);

        Task<OrderDetailTemp> GetDetailTempAsync(int id);

        Task AddCity(CityViewModel model);

        IEnumerable<SelectListItem> GetComboProducts();

        Task AddItemToOrderAsync(AddItemViewModel model, string userName);

        Task ModifyOrderDetailTempQuantityAsync(int id, double quantity);

        Task DeliverOrder(DeliverViewModel model);

        Task AddCountryAsync(Country country);

        Task UpdateCountryAsync(Country country);

        Task RemoveCountryAsync(Country country);

        IEnumerable<SelectListItem> GetComboCountries();

        IEnumerable<SelectListItem> GetComboCities(int conuntryId);

        Task<Country> GetCountryAsync(City city);
    }
}