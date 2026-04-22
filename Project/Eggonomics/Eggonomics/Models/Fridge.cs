using System;
using System.Collections.Generic;
using System.Text;

namespace Eggonomics.Models
{
    public class Fridge
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public User Owner { get; set; }
        

        public Fridge(int id, string name, User owner)
        {
            Id = id;
            Name = name;
            Owner = owner;
        }
    }
}
