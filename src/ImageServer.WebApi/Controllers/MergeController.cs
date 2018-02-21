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

            var secondaries = request.SubImages.ToDictionary(k=>k, v => Image.Load(v.Uri));

            using (var primary = Image.Load(request.Primary))
            {
                var secondary = secondaries.ElementAt(0);

                var pixelsToAdd = GetHeightValueToAdd(secondaries);
                var height = primary.Height + pixelsToAdd.addToTop + pixelsToAdd.addToBottom;
                var width = primary.Width + pixelsToAdd.addToLeft + pixelsToAdd.addToRight;

                using (var output = new Image<Rgba32>((int)width, (int)height))
                {
                    // do your drawing in here...
                }
            }

            for (int i = 0; i < secondaries.Count(); i++)
                secondaries.ElementAt(i).Value.Dispose();

            throw new NotImplementedException();
        }

        private (uint addToTop, uint addToBottom, uint addToRight, uint addToLeft) GetHeightValueToAdd(IDictionary<SubImage, Image<Rgba32>> subImages)
        {
            var imageToAddToTop = subImages.Where(si =>
            {
                var ml = (SubImageLocation)si.Key.MergeLocation;
                return ml == SubImageLocation.TopLeft
                    || ml == SubImageLocation.TopMiddle
                    || ml == SubImageLocation.TopRight;
            });
            var addToTop = GetMax(imageToAddToTop);

            var imageToAddToBottom= subImages.Where(si =>
            {
                var ml = (SubImageLocation)si.Key.MergeLocation;
                return ml == SubImageLocation.BottomLeft
                    || ml == SubImageLocation.BottomMiddle
                    || ml == SubImageLocation.BottomRight;
            });
            var addToBottom = GetMax(imageToAddToBottom);

            var imageToAddToRight= subImages.Where(si =>
            {
                var ml = (SubImageLocation)si.Key.MergeLocation;
                return ml == SubImageLocation.Right;
            });
            var addToRight= GetMax(imageToAddToRight);

            var imageToAddToLeft = subImages.Where(si =>
            {
                var ml = (SubImageLocation)si.Key.MergeLocation;
                return ml == SubImageLocation.Left;
            });
            var addToLeft= GetMax(imageToAddToLeft);

            return (addToTop, addToBottom, addToRight, addToLeft);
        }

        private uint GetMax(IEnumerable<KeyValuePair<SubImage, Image<Rgba32>>> imagesData)
        {
            return imagesData.Any() ?
                (uint)imagesData
                .Select(s => s.Key.Overlap * s.Value.Height / 100)
                .Max()
                : 0;
        }

        private void Save(string path, Image<Rgba32> image, string origPath)
        {
            image.Save(path + Path.GetExtension(origPath));
        }
    }
}