using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Core.Arrangement
{
    public abstract class Node
    {
        public event EventHandler<Position2D> PositionChanged;

        private Position2D _position;
        public Position2D Position 
        {
            get => _position;
            set
            {
                _position = value;
                InvokePositionChanged();
            }
        }

        protected void InvokePositionChanged()
        {
            PositionChanged?.Invoke(this, Position);
        }
    }
}
