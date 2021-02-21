using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core
{
    public class StudioClientMessage
    {
        public virtual string Category { get; set; }
        public virtual string Identifier { get; set; }
        public string Args { get; set; }

        public StudioClientMessage()
        {

        }
    }
}
