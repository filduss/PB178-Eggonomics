using System;
using System.Collections.Generic;
using System.Text;
using Eggonomics.Enums;

namespace Eggonomics.Models
{
    public class EggTray
    {
        public int Size { get; set; }
        public string Name { get; set; }
        public Suppliers Origin { get; set; }
        public DateOnly ExpirationDate { get; set; }
        public decimal Price { get; set; }
        public decimal PricePerEgg { get; set; }
        public Fridge Fridge { get; set; }

        public EggTray(int size, string name, Suppliers origin, DateOnly expirationDate, decimal price, decimal pricePerEgg, Fridge fridge)        {
            Size = size;
            Name = name;
            Origin = origin;
            ExpirationDate = expirationDate;
            Price = price;
            PricePerEgg = pricePerEgg;
            Fridge = fridge;
        }

    }
}
