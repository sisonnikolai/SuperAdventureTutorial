using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Item
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string NamePlural { get; set; }
        public int Price { get; set; }

        public Item(int iD, string name, string namePlural, int price)
        {
            ID = iD;
            Name = name;
            NamePlural = namePlural;
            Price = price;
        }
    }
}
