﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassicUO.Interfaces
{
    internal interface IPoolable
    {
        void OnPickup();
        void OnReturn();
    }
}
