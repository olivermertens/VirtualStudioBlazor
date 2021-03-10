using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualStudio.Core;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Core.Arrangement;
using VirtualStudio.Core.Operations;
using VirtualStudio.Shared;
using VirtualStudio.Shared.Abstractions;

namespace VirtualStudio.Core
{
    /// <summary>
    /// Manages multiple instances of VirtualStudio.
    /// </summary>
    public class VirtualStudioRepository
    {
        private struct VirtualStudioEntry
        {
            public VirtualStudio VirtualStudio { get; set; }
            public VirtualStudioEventProcessor EventProcessor { get; set; }
        }

        public int Count => virtualStudios.Count;

        Dictionary<string, VirtualStudioEntry> virtualStudios = new Dictionary<string, VirtualStudioEntry>();
        ILoggerFactory loggerFactory;
        IVirtualStudioUpdateListener virtualStudioUpdateListener;

        public VirtualStudioRepository(IVirtualStudioUpdateListener updateListener = null, ILoggerFactory loggerFactory = null)
        {
            this.virtualStudioUpdateListener = updateListener;
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
                return virtualStudios[name].VirtualStudio;
            }
            else
            {
                var virtualStudio = new VirtualStudioWithArrangement(logger: loggerFactory.CreateLogger<VirtualStudio>());
                InitializeVirtualStudio(virtualStudio);

                var eventProcessor = virtualStudioUpdateListener is null ? null : new VirtualStudioEventProcessor(virtualStudio, name, virtualStudioUpdateListener);

                virtualStudios.Add(name, new VirtualStudioEntry { VirtualStudio = virtualStudio, EventProcessor = eventProcessor });
                return virtualStudio;
            }
        }

        public bool TryGetVirtualStudio(string v, out object virtualStudio)
        {
            throw new NotImplementedException();
        }

        private void InitializeVirtualStudio(VirtualStudioWithArrangement virtualStudio)
        {
            var placeholder1 = new PlaceholderStudioComponent();
            placeholder1.SetName("Placeholder with input");
            placeholder1.AddInput("Input 1", DataKind.Audio, "WebRtc");
            var placeholder2 = new PlaceholderStudioComponent();
            placeholder2.SetName("Placeholder with output");
            placeholder2.AddOutput("Output 1", DataKind.Audio, "WebRtc");
            virtualStudio.ComponentRepository.AddPlaceholder(placeholder1);
            virtualStudio.ComponentRepository.AddPlaceholder(placeholder2);
            var componentNode1 = virtualStudio.AddComponent(placeholder1, new Position2D(100, 100));
            var componentNode2 = virtualStudio.AddComponent(placeholder2, new Position2D(100, 250));
            virtualStudio.CreateConnection(componentNode2.Component.Outputs[0], componentNode1.Component.Inputs[0]);
        }
    }
}
