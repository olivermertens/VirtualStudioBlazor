using System;
using System.Collections.Generic;

namespace VirtualStudio.Shared
{
    public class StudioClientBase
    {
        public string Identifier { get; set; }
        public NetworkInfo NetworkInfo { get; set; }
        public IODescription IODescription { get; set; }
        public List<TransmissionMethodDescription> TransmissionMethods { get; set; }

        public StudioClientBase()
        {
            TransmissionMethods = new List<TransmissionMethodDescription>();
            IODescription = new IODescription();
        }
    }
}