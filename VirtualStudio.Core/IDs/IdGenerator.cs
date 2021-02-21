using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Core
{
    public class IdGenerator
    {
        int idCounter = 1;

        public IdGenerator(int startValue = 1)
        {
            idCounter = startValue;
        }

        public int GetNewId() => idCounter++;
    }
}
