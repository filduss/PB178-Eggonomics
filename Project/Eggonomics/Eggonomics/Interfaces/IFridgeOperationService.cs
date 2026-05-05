using System;
using System.Collections.Generic;
using System.Text;

namespace Eggonomics.Interfaces
{
    public interface IFridgeOperationService
    {
        public Task InsertEggsAsync();
    }
}
