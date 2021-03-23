using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VirtualStudio.Client.Shared
{
    public interface INoodleData
    {
        Vector2 StartPos { get; }
        Vector2 EndPos { get; }
        bool Connected { get; }
        void Refresh();
        string Color { get; }

        event EventHandler NoodleChanged;
    }
}
