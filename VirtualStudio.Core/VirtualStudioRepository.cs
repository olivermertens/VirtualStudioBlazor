﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualStudio.Core;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Core.Arrangement;
using VirtualStudio.Core.Operations;

namespace VirtualStudio.Core
{
    /// <summary>
    /// Manages multiple instances of VirtualStudio.
    /// </summary>
    public class VirtualStudioRepository
    {
        public int Count => virtualStudios.Count;

        Dictionary<string, VirtualStudio> virtualStudios = new Dictionary<string, VirtualStudio>();
        ILoggerFactory loggerFactory;

        public VirtualStudioRepository(ILoggerFactory loggerFactory = null)
        {
            this.loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        }

        public IEnumerable<string> GetVirtualStudioIds()
        {
            return virtualStudios.Keys;
        }

        public bool TryGetVirtualStudio(string name, out VirtualStudio virtualStudio)
        {
            virtualStudio = GetVirtualStudio(name);
            return virtualStudio != null;
        }

        public VirtualStudio GetVirtualStudio(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            if (virtualStudios.ContainsKey(name))
            {
                return virtualStudios[name];
            }
            else
            {
                var virtualStudio = new VirtualStudioWithArrangement(logger: loggerFactory.CreateLogger<VirtualStudio>());
                virtualStudios.Add(name, virtualStudio);
                return virtualStudio;
            }
        }

        public bool TryGetVirtualStudio(string v, out object virtualStudio)
        {
            throw new NotImplementedException();
        }
    }
}
