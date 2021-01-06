using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Core.Arrangement
{
    public struct Position2D
    {
        public float X { get; }
        public float Y { get; }

        public Position2D(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Position2D Zero => new Position2D();
    }
}
