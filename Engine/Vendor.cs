using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Engine
{
    public class Vendor : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public BindingList<InventoryItem> Inventory { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public Vendor(string name)
        {
            Name = name;
            Inventory = new BindingList<InventoryItem>();
        }
        public void AddItemToInventory(Item itemToAdd, int quantity = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToAdd.ID);

            if(item == null)
            {
                //If item is not in inventory, add by 1
                Inventory.Add(new InventoryItem(itemToAdd, quantity));
            }
            else
            {
                //If item is present, increase by 1
                item.Quantity += quantity;
            }
            OnPropertyChanged("Inventory");
        }

        public void RemoveItemFromInventory(Item itemToRemove, int quantity = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToRemove.ID);

            if(item == null)
            {
                //Item not found. Error message might be necessary in this situation
            }
            else
            {
                //Item is present. Decrease by 1
                item.Quantity -= quantity;

                //Negative quantity is not allowed. Error message necessary 
                if(item.Quantity < 0)
                {
                    item.Quantity = 0;
                }

                if(item.Quantity == 0)
                {
                    Inventory.Remove(item);
                }
                OnPropertyChanged("Inventory");
            }
        }

        public static Vendor GetVendor()
        {
            Vendor vendor = new Vendor("Bobby");

            var itemID1 = RandomNumberGenerator.NumberBetween(1, World.Items.Count);
            var itemID2 = RandomNumberGenerator.NumberBetween(1, World.Items.Count);

            vendor.AddItemToInventory(World.ItemByID(itemID1), 5);
            vendor.AddItemToInventory(World.ItemByID(itemID2), 5);

            return vendor;
        }

        private void OnPropertyChanged(string name)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
