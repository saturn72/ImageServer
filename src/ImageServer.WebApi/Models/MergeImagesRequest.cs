using System.Collections.Generic;

namespace ImageServer.WebApi.Models
{
    public class MergeImagesRequest
    {
        public string Primary { get; set; }
        public IEnumerable<SubImage> SubImages { get; set; }
    }
}
