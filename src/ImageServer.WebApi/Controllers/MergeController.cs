using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageServer.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;

namespace ImageServer.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class MergeController : Controller
    {
        // GET api/values
        [HttpPost]
        public async Task<IActionResult> MergeImages([FromBody]MergeImagesRequest request)
        {
            if (!request.Primary.HasValue())
                return BadRequest(new
                {
                    message = "missing data. Please specify both images",
                    data = request
                });

            var secondaries = request.SubImages.Select(s => Image.Load(s.Uri));

            using (var primary = Image.Load(request.Primary))
            {

                //ignore multiple elements
                var secondary = secondaries.ElementAt(0);

                var overlapPercentage = (int)(100 - request.SubImages.ElementAt(0).Overlap) / 100;
                //Create empty filr for output
                int width = primary.Width + secondary.Width * overlapPercentage;
                var heightAddition = GetHeightAddition(request.SubImages);
                int height = primary.Height + secondary.Height * overlapPercentage;

                using (var output = new Image<Rgba32>(width, height))
                {

                    // do your drawing in here...
                }

                //get overlap
                //get location

                // image is now in a file format agnositic structure in memory as a series of Rgba32 pixels
                //image.Mutate(ctx => ctx.Resize(image.Width / 2, image.Height / 2)); // resize the image in place and return it for chaining

                var outDir = "output";
                if (!Directory.Exists(outDir))
                    Directory.CreateDirectory(outDir);
                Save(outDir + "\\primary", primary, request.Primary);
                for (int i = 0; i < secondaries.Count(); i++)
                    Save(outDir + "\\secondary_" + i, secondaries.ElementAt(i), 
                        request.SubImages.ElementAt(i).Uri);
            }   

            for (int i = 0; i < secondaries.Count(); i++)
                secondaries.ElementAt(i).Dispose();

            throw new NotImplementedException();
        }

        private int[] GetHeightAddition(IEnumerable<SubImage> subImages)
        {
            var result = new [] { 0, 0 };
            var siLocations = subImages.Select(si => (SubImageLocation)si.MergeLocation);

            //Choose max overlap from bottom and top
            if (siLocations.Any(slBotton =>
            slBotton == SubImageLocation.BottomLeft ||
            slBotton == SubImageLocation.BottomMiddle ||
            slBotton == SubImageLocation.BottomRight))
                result[0] = 


            throw new NotImplementedException();
        }

        private void Save(string path, Image<Rgba32> image, string origPath)
        {
            image.Save(path + Path.GetExtension(origPath));
        }
    }
}