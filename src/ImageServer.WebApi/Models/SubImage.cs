namespace ImageServer.WebApi.Models
{
    public class SubImage
    {
        public string Uri { get; set; }
        public uint MergeLocation { get; set; }
        public uint Overlap { get; set; }
    }
}