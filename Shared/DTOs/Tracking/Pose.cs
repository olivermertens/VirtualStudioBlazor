using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace VirtualStudio.Shared.DTOs.Tracking
{
    public struct TrackingMessage<T> where T : struct
    {
        public long Timestamp { get; set; }
        public T Data {get; set;}
    }

    public struct Pose
    {
        public Vector3 Position { get; set; }
        public Vector4 Orientation { get; set; }
        public Matrix4x4 Projection { get; set; }

        public byte[] ToBinary()
        {
            return ToBinary(this);
        }

        public static byte[] ToBinary(Pose pose)
        {
            int size = Marshal.SizeOf(pose);
            byte[] bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(pose, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        public static Pose FromBinary(byte[] bytes)
        {
            var pose = new Pose();
            int size = Marshal.SizeOf(pose);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, ptr, size);
            pose = (Pose)Marshal.PtrToStructure(ptr, typeof(Pose));
            Marshal.FreeHGlobal(ptr);
            return pose;
        }
    }
}
