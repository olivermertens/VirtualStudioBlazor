using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core
{
    public static class TransmissionMethods
    {
        private static HashSet<string> _collection = new HashSet<string>
        {
            WebRtc, Ndi
        };
        public static IEnumerable<string> Collection => _collection;

        public static string WebRtc => "WebRtc";
        public static string Ndi => "Ndi";
    }
}
