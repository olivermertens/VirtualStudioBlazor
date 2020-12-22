using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualStudio.Core;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core
{
    public class VirtualStudioManager
    {
        public int Count => virtualStudios.Count;

        Dictionary<string, VirtualStudio> virtualStudios;
        ILoggerFactory loggerFactory;

        public VirtualStudioManager(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        }

        public IEnumerable<string> GetVirtualStudioIds()
        {
            return virtualStudios.Keys;
        }

        public VirtualStudio GetVirtualStudio(string name)
        {
            if (virtualStudios.ContainsKey(name))
            {
                return virtualStudios[name];
            }
            else
            {
                VirtualStudio virtualStudio = new VirtualStudio(null, loggerFactory.CreateLogger<VirtualStudio>());
                virtualStudios.Add(name, virtualStudio);
                return virtualStudio;
            }
        }

        public VirtualStudio AddComponent(string virtualStudioId, StudioComponentBase studioComponent)
        {
            var virtualStudio = GetVirtualStudio(virtualStudioId);
            virtualStudio?.AddComponent(studioComponent);
            return virtualStudio;
        }
    }
}
