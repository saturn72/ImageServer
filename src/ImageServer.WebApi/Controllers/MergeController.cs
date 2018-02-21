using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ImageServer.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImageServer.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class MergeController : Controller
    {

        // GET api/values
        [HttpPost]
        public IActionResult MergeImages([FromBody]MergeImagesRequest request)
        {
            using (var primaryImage = new Bitmap(Image.FromFile(request.Primary)))
            {
                var topRightImage = GetSubImageByPosition(request.SubImages, SubImageLocation.TopRight);
                var middleRightImage = GetSubImageByPosition(request.SubImages, SubImageLocation.Right);
                var bottomRightImage = GetSubImageByPosition(request.SubImages, SubImageLocation.BottomRight);

                var paddingRight = Math.Max(Math.Max(topRightImage?.Width ?? 0, middleRightImage?.Width ?? 0), bottomRightImage?.Width ?? 0);

                var topLeftImage = GetSubImageByPosition(request.SubImages, SubImageLocation.TopLeft);
                var middleLeftImage = GetSubImageByPosition(request.SubImages, SubImageLocation.Left);
                var bottomLeftImage = GetSubImageByPosition(request.SubImages, SubImageLocation.BottomLeft);

                var paddingLeft = Math.Max(Math.Max(topLeftImage?.Width ?? 0, middleLeftImage?.Width ?? 0), bottomLeftImage?.Width ?? 0);

                var newHeight = primaryImage.Height;
                var newWidth = primaryImage.Width + paddingRight + paddingLeft;
                var newImage = new Bitmap(newWidth, newHeight);

                PlacePrimaryImage(newImage, primaryImage, paddingLeft);
                PlaceLeftImages(newImage, paddingLeft, topLeftImage, middleLeftImage, bottomLeftImage);


                //put right Top Image
                if (topRightImage != null)
                {
                    for (int y = 0; y < topRightImage.Height; y++)
                    {
                        for (int x = 0; x < topRightImage.Width; x++)
                        {
                            Color col = topRightImage.GetPixel(x, y);
                            newImage.SetPixel(newWidth - topRightImage.Width + x, y, col);
                        }
                    }
                }


                
                newImage.Save("output/newImage.jpg");

                if (topRightImage != null)
                    topRightImage.Dispose();
            }
            return Ok();
        }

        private void PlaceLeftImages(Bitmap newImage, int paddingLeft, Bitmap topLeftImage, Bitmap middleLeftImage, Bitmap bottomLeftImage)
        {
            throw new NotImplementedException();
        }

        private void PlacePrimaryImage(Bitmap newImage, Bitmap primaryImage, int paddingLeft)
        {
            for (int y = 0; y < newImage.Height; y++)
            {
                for (int x = 0; x < primaryImage.Width; x++)
                {
                    Color col = primaryImage.GetPixel(x, y);
                    newImage.SetPixel(paddingLeft+ x, y, col);
                }
            }
        }

        private Bitmap GetSubImageByPosition(IEnumerable<SubImage> subImages, SubImageLocation imgLocation)
        {
            var img = subImages.FirstOrDefault(x => (SubImageLocation)x.MergeLocation == imgLocation);
            return img != null ? new Bitmap(Image.FromFile(img.Uri)) : null;
        }

        private (int addToTop, int addToBottom, int addToRight, int addToLeft) GetHeightValueToAdd(IEnumerable<System.Drawing.Image> subImages)
        {
            return (0, 0, subImages.ElementAt(0).Width, 0);

            //var imageToAddToTop = subImages.Where(si =>
            //{
            //    var ml = (SubImageLocation)si.Key.MergeLocation;
            //    return ml == SubImageLocation.TopLeft
            //        || ml == SubImageLocation.TopMiddle
            //        || ml == SubImageLocation.TopRight;
            //});
            //var addToTop = GetMax(imageToAddToTop);

            //var imageToAddToBottom = subImages.Where(si =>
            // {
            //     var ml = (SubImageLocation)si.Key.MergeLocation;
            //     return ml == SubImageLocation.BottomLeft
            //         || ml == SubImageLocation.BottomMiddle
            //         || ml == SubImageLocation.BottomRight;
            // });
            //var addToBottom = GetMax(imageToAddToBottom);

            //var imageToAddToRight = subImages.Where(si =>
            // {
            //     var ml = (SubImageLocation)si.Key.MergeLocation;
            //     return ml == SubImageLocation.Right;
            // });
            //var addToRight = GetMax(imageToAddToRight);

            //var imageToAddToLeft = subImages.Where(si =>
            //{
            //    var ml = (SubImageLocation)si.Key.MergeLocation;
            //    return ml == SubImageLocation.Left;
            //});
            //var addToLeft = GetMax(imageToAddToLeft);

            //return (addToTop, addToBottom, addToRight, addToLeft);
        }

        //private int GetMax(IEnumerable<KeyValuePair<SubImage, Image<Rgba32>>> imagesData)
        //{
        //    return imagesData.Any() ?
        //        (int)imagesData
        //        .Select(s => s.Key.Overlap * s.Value.Height / 100)
        //        .Max()
        //        : 0;
        //}

        //private void Save(string path, Image<Rgba32> image, string origPath)
        //{
        //    image.Save(path + Path.GetExtension(origPath));
        //}
    }
}