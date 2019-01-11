using System;

using AppIPG.Models;

namespace AppIPG.ViewModels
{
    public class ItemDetailViewModel : BaseViewModel
    {
        public Item Item { get; set; }
        public ItemDetailViewModel(Item item = null)
        {
            Title = item?.Title;
            Item = item;
        }
    }
}
