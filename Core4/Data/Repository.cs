namespace Core4.Data
{
    using Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Repository : IRepository
    {
        private readonly DataContext context;
        private readonly UserManager<User> userManager;
        private readonly ILogger<DataContext> logger;

        public Repository(
            DataContext context, 
            UserManager<User> userManager, 
            ILogger<DataContext> logger)
        {
            this.context = context;
            this.userManager = userManager;
            this.logger = logger;
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

        public async Task<IEnumerable<Order>> GetOrdersAsync(string userName)
        {
            var user = await this.userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return null;
            }

            if (await this.userManager.IsInRoleAsync(user, "Admin"))
            {
                var orders = this.context.Orders
                    .Include(o => o.User)
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                    .OrderBy(o => o.OrderDate);
                return orders;
            }
            else
            {
                var orders = this.context.Orders
                    .Include(o => o.User)
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                    .Where(o => o.User == user)
                    .OrderBy(o => o.OrderDate);
                return orders;

            }
        }

        public async Task<IEnumerable<OrderDetailTemp>> GetDetailTempsAsync(string userName)
        {
            var user = await this.userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return null;
            }

            var orderDetailTemps = this.context.OrderDetailTemps
                .Include(o => o.Product)
                .Where(o => o.User == user)
                .OrderBy(o => o.Product.Name);
            return orderDetailTemps;
        }

        public IEnumerable<SelectListItem> GetComboProducts()
        {
            var list = this.context.Products.Select(p => new SelectListItem
            {
                Text = p.Name,
                Value = p.Id.ToString()
            }).OrderBy(l => l.Text).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "(Select a product...)",
                Value = "0"
            });

            return list;
        }

        public async Task AddItemToOrderAsync(AddItemViewModel model, string userName)
        {
            var user = await this.userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return;
            }

            var product = await this.context.Products.FindAsync(model.ProductId);
            if (product == null)
            {
                return;
            }

            var orderDetailTemp = await this.context.OrderDetailTemps
                .Where(odt => odt.User == user && odt.Product == product)
                .FirstOrDefaultAsync();
            if (orderDetailTemp == null)
            {
                orderDetailTemp = new OrderDetailTemp
                {
                    Price = product.Price,
                    Product = product,
                    Quantity = model.Quantity,
                    User = user,
                };

                this.context.OrderDetailTemps.Add(orderDetailTemp);
            }
            else
            {
                orderDetailTemp.Quantity += model.Quantity;
                this.context.OrderDetailTemps.Update(orderDetailTemp);
            }

            await this.context.SaveChangesAsync();
        }

        public async Task ModifyOrderDetailTempQuantityAsync(int id, double quantity)
        {
            var orderDetailTemp = await this.context.OrderDetailTemps.FindAsync(id);
            if (orderDetailTemp == null)
            {
                return;
            }

            orderDetailTemp.Quantity += quantity;
            if (orderDetailTemp.Quantity > 0)
            {
                this.context.OrderDetailTemps.Update(orderDetailTemp);
                await this.context.SaveChangesAsync();
            }
        }

        public async Task<OrderDetailTemp> GetDetailTempAsync(int id)
        {
            var orderDetailTemp = await this.context.OrderDetailTemps.FindAsync(id);
            return orderDetailTemp;
        }

        public async Task DeleteDetailTempAsync(int id)
        {
            var orderDetailTemp = await this.context.OrderDetailTemps.FindAsync(id);
            if (orderDetailTemp == null)
            {
                return;
            }

            this.context.OrderDetailTemps.Remove(orderDetailTemp);
            await this.context.SaveChangesAsync();
        }

        public async Task<bool> ConfirmOrderAsync(string userName)
        {
            var user = await this.userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return false;
            }

            var orderTmps = await this.context.OrderDetailTemps
                .Include(o => o.Product)
                .Where(o => o.User == user)
                .ToListAsync();

            if (orderTmps == null || orderTmps.Count == 0)
            {
                return false;
            }

            var details = orderTmps.Select(o => new OrderDetail
            {
                Price = o.Price,
                Product = o.Product,
                Quantity = o.Quantity
            }).ToList();

            var order = new Order
            {
                OrderDate = DateTime.UtcNow,
                User = user,
                Items = details,
            };

            this.context.Orders.Add(order);
            this.context.OrderDetailTemps.RemoveRange(orderTmps);
            await this.context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteOrderAsync(int id)
        {
            var order = await this.context.Orders
                .Where(o => o.Id == id)
                .Include(o => o.Items)
                .FirstOrDefaultAsync();
            if (order == null)
            {
                return;
            }

            if (order.DeliveryDate != DateTime.MinValue)
            {
                return;
            }

            this.context.Orders.Remove(order);
            await this.context.SaveChangesAsync();
        }

        public async Task<Order> GetOrdersAsync(int id)
        {
            return await this.context.Orders.FindAsync(id);
        }

        public async Task DeliverOrder(DeliverViewModel model)
        {
            var order = await this.context.Orders.FindAsync(model.Id);
            if (order == null)
            {
                return;
            }

            order.DeliveryDate = model.DeliveryDate;
            this.context.Orders.Update(order);
            await this.context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Country>> GetCountriesAsync()
        {
            return await this.context.Countries
                .Include(c => c.Cities)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Country> GetCountryAsync(int id)
        {
            return await this.context.Countries
                .Include(c => c.Cities)
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task AddCountryAsync(Country country)
        {
            this.context.Countries.Add(country);
            await this.context.SaveChangesAsync();
        }

        public async Task UpdateCountryAsync(Country country)
        {
            this.context.Countries.Update(country);
            await this.context.SaveChangesAsync();
        }

        public async Task RemoveCountryAsync(Country country)
        {
            this.context.Countries.Remove(country);
            await this.context.SaveChangesAsync();
        }

        public async Task<City> GetCityAsync(int id)
        {
            return await this.context.Cities.FindAsync(id);
        }

        public async Task AddCity(CityViewModel model)
        {
            var country = await this.GetCountryAsync(model.CountryId);
            if (country == null)
            {
                return;
            }

            country.Cities.Add(new City { Name = model.Name });
            this.context.Countries.Update(country);
            await this.context.SaveChangesAsync();
        }

        public async Task<int> UpdateCity(City city)
        {
            var country = await this.context.Countries.Where(c => c.Cities.Any(ci => ci.Id == city.Id)).FirstOrDefaultAsync();
            if (country == null)
            {
                return 0;
            }

            this.context.Cities.Update(city);
            await this.context.SaveChangesAsync();
            return country.Id;
        }

        public async Task<int> DeleteCityAsync(City city)
        {
            var country = await this.context.Countries.Where(c => c.Cities.Any(ci => ci.Id == city.Id)).FirstOrDefaultAsync();
            if (country == null)
            {
                return 0;
            }

            this.context.Cities.Remove(city);
            await this.context.SaveChangesAsync();
            return country.Id;
        }

        public IEnumerable<SelectListItem> GetComboCountries()
        {
            var list = this.context.Countries.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            }).OrderBy(l => l.Text).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "(Select a country...)",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboCities(int conuntryId)
        {
            var country = this.context.Countries.Find(conuntryId);
            var list = new List<SelectListItem>();
            if (country != null)
            {
                list = country.Cities.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }).OrderBy(l => l.Text).ToList();
            }

            list.Insert(0, new SelectListItem
            {
                Text = "(Select a city...)",
                Value = "0"
            });

            return list;
        }

        public async Task<Country> GetCountryAsync(City city)
        {
            return await this.context.Countries.Where(c => c.Cities.Any(ci => ci.Id == city.Id)).FirstOrDefaultAsync();
        }
    }
}