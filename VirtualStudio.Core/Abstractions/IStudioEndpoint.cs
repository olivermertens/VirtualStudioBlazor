﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Core.Abstractions
{
    public interface IStudioEndpoint
    {
        DataKind DataKind { get; }
    }
}
