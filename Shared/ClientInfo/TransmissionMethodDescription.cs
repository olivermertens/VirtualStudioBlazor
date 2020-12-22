namespace VirtualStudio.Shared
{
    public enum TransmissionMethodType
    {
        WebRtc, Ndi, Custom
    }

    public struct TransmissionMethodDescription
    {
        public string Name { get; set; }
        public TransmissionMethodType Type { get; set; }
        public DataKind SupportedDataKinds { get; set; }

        public TransmissionMethodDescription(string name, TransmissionMethodType type, DataKind supportedDataKinds)
        {
            Name = name;
            Type = TransmissionMethodType.WebRtc;
            SupportedDataKinds = supportedDataKinds;
        }

        public static TransmissionMethodDescription WebRtc => new TransmissionMethodDescription("WebRTC", TransmissionMethodType.WebRtc, DataKind.Video | DataKind.Audio | DataKind.Data);
    }
}