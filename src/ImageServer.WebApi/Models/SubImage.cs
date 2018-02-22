namespace ImageServer.WebApi.Models
{
    public class SubImage
    {
        public string Uri { get; set; }
        public string Location { get; set; }
        public uint Overlap { get; set; }
    }
}