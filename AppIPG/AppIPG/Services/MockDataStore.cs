using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AppIPG.Helpers;
using AppIPG.Models;
using Newtonsoft.Json;

namespace AppIPG.Services
{
    public class MockDataStore : IDataStore<Item>
    {
        List<Item> items;
        private readonly ApiServices _apiServices = new ApiServices();
        public MockDataStore()
        {
            items = new List<Item>();
        }

        public async Task<bool> AddItemAsync(Item item)
        {
            var accessToken = AccountDetailsStore.Instance.Token;
            var Workshops = await _apiServices.WorkshopPostAsync(accessToken, item); //<- fazer o get no endpoint

            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Item item)
        {
            var oldItem = items.Where((Item arg) => arg.Id == item.Id).FirstOrDefault();
            items.Remove(oldItem);
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var oldItem = items.Where((Item arg) => arg.Id == id).FirstOrDefault();
            items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<Item> GetItemAsync(string id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }


        
        public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
        {
            var accessToken = AccountDetailsStore.Instance.Token;   
            var Workshops = await _apiServices.WorkshopAsync(accessToken); //<- fazer o get no endpoint
            // utilizar uma libraria para parse do JSON --- Download no nuGet
            var Items = JsonConvert.DeserializeObject<List<Item>>(Workshops);
            foreach (var workshop in Items)
            {
                items.Add(workshop);
            }

            return await Task.FromResult(items);
        }
    }
}