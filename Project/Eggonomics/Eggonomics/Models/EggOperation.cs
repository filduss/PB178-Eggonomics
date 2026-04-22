using System;
using System.Collections.Generic;
using System.Text;
using Eggonomics.Enums;

namespace Eggonomics.Models
{
    public class EggOperation
    {
        public OperationType Type { get; set; }
        public Fridge Fridge { get; set; }
        public FridgeUser FridgeUser { get; set; }

    }
}
