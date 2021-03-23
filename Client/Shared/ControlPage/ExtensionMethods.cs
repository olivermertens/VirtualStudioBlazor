using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VirtualStudio.Client.Shared
{
    public static class ExtensionMethods
    {
        public static Vector2 GetClientPos(this Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
        {
            return new Vector2(e.ClientX, e.ClientY);
        }
    }
}
