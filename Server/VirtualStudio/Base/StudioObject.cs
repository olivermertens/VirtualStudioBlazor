using System;

namespace VirtualStudio.Server
{
    public abstract class StudioObject
    {
        static class IdCounter
        {
            static int counter = 0;
            public static int GetId()
            {
                ++counter;
                return counter;
            }
        }

        public int Id { get; } = IdCounter.GetId();
    }
}