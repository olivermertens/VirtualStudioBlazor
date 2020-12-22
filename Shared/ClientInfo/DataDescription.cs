namespace VirtualStudio.Shared
{
    public struct DataDescription
    {
        public string Identifier { get; set; }

        public DataDescription(string identifier)
        {
            Identifier = identifier;
        }

        public static DataDescription Video { get; } = new DataDescription("Video");
        public static DataDescription Audio { get; } = new DataDescription("Audio");

    }
}