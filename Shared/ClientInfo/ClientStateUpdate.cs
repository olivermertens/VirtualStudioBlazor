namespace VirtualStudio.Shared
{
    public class LinkStateUpdate
    {
        public int LinkId { get; set; }
        public LinkState State { get; set; }

        public LinkStateUpdate (int linkId, LinkState state)
        {
            LinkId = linkId;
            State = state;
        }
    }
}