﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VirtualStudio.Core.Abstractions
{
    public interface IStudioClient
    {     
        IStudioComponent GetComponent();
    }
}
