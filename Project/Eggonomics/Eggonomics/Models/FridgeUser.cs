using System;
using System.Collections.Generic;
using System.Text;

namespace Eggonomics.Models
{
    public class FridgeUser
    {
        public int Id { get; set; }
        public int EggsInserted { get; set; }
        public int EggsWithdraw { get; set; }

        public Fridge Fridge { get; set; }
        public User User { get; set; }

        public FridgeUser(int id, int eggsInserted, int eggsWithdraw, Fridge fridge, User user)
        {
            Id = id;
            EggsInserted = eggsInserted;
            EggsWithdraw = eggsWithdraw;
            Fridge = fridge;
            User = user;
        }
    }
}
