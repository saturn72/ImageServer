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
                var subImages = GetSubImagesByPosition(request.SubImages);

                var rightImages = subImages.Where(si =>
                    si.Key == SubImageLocation.TopRight
                    || si.Key == SubImageLocation.MiddleRight
                    || si.Key == SubImageLocation.BottomRight);
                var paddingRight = rightImages.Any()? rightImages.Max(x => x.Value.Width): 0;

                var leftImages = subImages.Where(si =>
                si.Key == SubImageLocation.TopLeft
                || si.Key == SubImageLocation.MiddleLeft
                || si.Key == SubImageLocation.BottomLeft);
                var paddingLeft = leftImages.Any()? leftImages.Max(x => x.Value.Width) : 0;

                var newHeight = primaryImage.Height;
                var newWidth = primaryImage.Width + paddingRight + paddingLeft;
                var newImage = new Bitmap(newWidth, newHeight);

                PlacePrimaryImage(newImage, primaryImage, paddingLeft);
                PlaceSubImages(newImage, subImages);

                newImage.Save("output/newImage.jpg");

                if (subImages != null)
                {
                    foreach (var bm in subImages.Values)
                        bm.Dispose();
                }
            }
            return Ok();
        }

        private void PlaceSubImages(Bitmap outputImage, IDictionary<SubImageLocation, Bitmap> subImages)
        {
            var outputStartYIndex = 0;
            var outputStartXIndex = 0;
            int maxImgHeight = 0;

            foreach (var img in subImages)
            {
                var image = img.Value;
                var location = img.Key;

                switch (location)
                {
                    case SubImageLocation.TopRight:
                        outputStartYIndex = 0;
                        outputStartXIndex = Math.Max( outputImage.Width-img.Value.Width, 0);
                        maxImgHeight = Math.Min(image.Height, outputImage.Height);
                        break;
                    case SubImageLocation.TopMiddle:
                        outputStartYIndex = 0;
                        outputStartXIndex = Math.Max((outputImage.Width - img.Value.Width) / 2, 0);
                        maxImgHeight = Math.Min(image.Height, outputImage.Height - outputStartYIndex);
                        break;
                    case SubImageLocation.TopLeft:
                        outputStartYIndex = 0;
                        outputStartXIndex = 0;
                        maxImgHeight = Math.Min(image.Height, outputImage.Height);
                        break;
                    case SubImageLocation.MiddleRight:
                        outputStartYIndex = Math.Max(0, (outputImage.Height - image.Height) / 2);
                        outputStartXIndex = Math.Max(outputImage.Width - img.Value.Width, 0);
                        maxImgHeight = Math.Min(image.Height, outputImage.Height - outputStartYIndex);
                        break;
                    case SubImageLocation.Center:
                        outputStartYIndex = Math.Max(0, (outputImage.Height - image.Height) / 2);
                        outputStartXIndex = Math.Max((outputImage.Width - img.Value.Width)/2, 0);
                        maxImgHeight = Math.Min(image.Height, outputImage.Height - outputStartYIndex);
                        break;
                    case SubImageLocation.MiddleLeft:
                        outputStartYIndex = Math.Max(0, (outputImage.Height - image.Height) / 2);
                        outputStartXIndex = 0;
                        maxImgHeight = Math.Min(image.Height, outputImage.Height - outputStartYIndex);
                        break;
                    case SubImageLocation.BottomRight:
                        outputStartYIndex = Math.Max(0, outputImage.Height - image.Height);
                        outputStartXIndex = Math.Max(outputImage.Width - img.Value.Width, 0);
                        maxImgHeight = image.Height;
                        break;
                    case SubImageLocation.BottomMiddle:
                        outputStartYIndex = Math.Max(0, outputImage.Height - image.Height);
                        outputStartXIndex = Math.Max((outputImage.Width - img.Value.Width)/2, 0);
                        maxImgHeight = Math.Min(image.Height, outputImage.Height - outputStartYIndex);
                        break;
                    case SubImageLocation.BottomLeft:
                        outputStartYIndex = Math.Max(0, outputImage.Height - image.Height);
                        outputStartXIndex = 0;
                        maxImgHeight = image.Height;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(location));
                }

                //place top left

                for (int newImageY = outputStartYIndex, imgY = 0; imgY < maxImgHeight; newImageY++, imgY++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Color col = image.GetPixel(x, imgY);
                        outputImage.SetPixel(outputStartXIndex + x, newImageY, col);
                    }
                }
            }
        }

        private void PlacePrimaryImage(Bitmap newImage, Bitmap primaryImage, int paddingLeft)
        {
            for (int y = 0; y < newImage.Height; y++)
            {
                for (int x = 0; x < primaryImage.Width; x++)
                {
                    Color col = primaryImage.GetPixel(x, y);
                    newImage.SetPixel(paddingLeft + x, y, col);
                }
            }
        }

        private IDictionary<SubImageLocation, Bitmap> GetSubImagesByPosition(IEnumerable<SubImage> subImages)
        {
            var result = new Dictionary<SubImageLocation, Bitmap>();

            var list = subImages.Where(si => Enum.TryParse(si.Location, true, out SubImageLocation sil));
            foreach (var itm in list)
                result[Enum.Parse<SubImageLocation>(itm.Location, true)] = new Bitmap(Image.FromFile(itm.Uri));
            return result;
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