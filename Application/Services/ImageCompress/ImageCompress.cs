//using Microsoft.AspNetCore.Http;
//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Formats.Jpeg;
//using SixLabors.ImageSharp.Processing;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Application.Services.ImageCompress
//{
//    public class ImageCompress : IImageCompress
//    {
//        public async Task Compress(IFormFile imageFile, string outputPath, int quality = 75)
//        {
//            using var imageStream = imageFile.OpenReadStream();
//            using var image = Image.Load(imageStream);
//            int newWidth = image.Width;
//            int newHeight = image.Height;
//            if (image.Width > 1000)
//            {
//                 newWidth = Math.Max((int)(image.Width * 0.5), 1000);
//                 newHeight = Math.Max((int)(image.Height * 0.5), 1000);
//            }
//            // Resize the image
//            image.Mutate(x => x.Resize(newWidth, newHeight));
//            var encoder = new JpegEncoder
//            {
//                Quality = quality, // Adjust this value for desired compression quality
//            };

//            await Task.Run(() => image.Save(outputPath, encoder));
//        }
//    }
//}
