using FaceSDK;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FaceRecognition_.Net
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        FaceEngineClass faceSDK = new FaceEngineClass();

        public MainPage()
        {
            InitializeComponent();

            int ret = faceSDK.Init("E:/assets");

        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);

        }

        private void OnGetMachineCode(object sender, EventArgs e)
        {

            MachineCodeLabel.Text = faceSDK.GetHardwareId();
            SemanticScreenReader.Announce(MachineCodeLabel.Text);

            //ImageCaption.Source = ImageSource.FromFile("dotnet_bot.png");

            // Read local image, convert it to byte array, and get image size
            string imagePath = "E:/1.jpg";

            System.Drawing.Image image = null;
            try
            {
                image = LoadImageWithExif(imagePath);
            }
            catch (Exception)
            {
                DisplayAlert("test", "Unknown Format!", "ok");
                return;
            }

            //byte[] imgbuf = ImageToByteArray(image);

            Bitmap imgBmp = ConvertTo24bpp(image);
            BitmapData bitmapData = imgBmp.LockBits(new Rectangle(0, 0, imgBmp.Width, imgBmp.Height), ImageLockMode.ReadWrite, imgBmp.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(imgBmp.PixelFormat) / 8;
            int byteCount = bitmapData.Stride * imgBmp.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);

            imgBmp.UnlockBits(bitmapData);

            ResultBox[] resultBoxes = new ResultBox[10];
            int faceCount = faceSDK.DetectFace(pixels, imgBmp.Width, imgBmp.Height, bitmapData.Stride, resultBoxes, 10);

        }

        public byte[] BitmapToJpegByteArray(Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Save the Bitmap as JPEG to the MemoryStream
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                // Return the byte array
                return memoryStream.ToArray();
            }
        }

        public byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }

        public static Bitmap ConvertTo24bpp(System.Drawing.Image img)
        {
            var bmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var gr = Graphics.FromImage(bmp))
                gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            return bmp;
        }

        private static System.Drawing.Image LoadImageWithExif(String filePath)
        {
            try
            {
                System.Drawing.Image image = System.Drawing.Image.FromFile(filePath);

                // Check if the image has EXIF orientation data
                if (image.PropertyIdList.Contains(0x0112))
                {
                    int orientation = image.GetPropertyItem(0x0112).Value[0];

                    switch (orientation)
                    {
                        case 1:
                            // Normal
                            break;
                        case 3:
                            // Rotate 180
                            image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        case 6:
                            // Rotate 90
                            image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case 8:
                            // Rotate 270
                            image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                        default:
                            // Do nothing
                            break;
                    }
                }

                return image;
            }
            catch (Exception e)
            {
                throw new Exception("Image null!");
            }
        }


    }

}
