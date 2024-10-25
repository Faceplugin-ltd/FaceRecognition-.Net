using FaceSDK;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

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

        private async void OnRecognizeFaceBtn(object sender, EventArgs e)
        {

            var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.my.comic.extension" } }, // UTType values
                    { DevicePlatform.Android, new[] { "application/comics" } }, // MIME type
                    { DevicePlatform.WinUI, new[] { ".jpg", ".png" } }, // file extension
                    { DevicePlatform.Tizen, new[] { "*/*" } },
                    { DevicePlatform.macOS, new[] { "jpg", "png" } }, // UTType values
                });

            PickOptions options = new()
            {
                PickerTitle = "Please select a image file",
                FileTypes = customFileType,
            };

            try
            {
                await PickAndShow(options);
            }
            catch (OperationCanceledException)
            {
                throw new OperationCanceledException("It was cancelled");
            }

        }

        private void OnActivate(object sender, EventArgs e)
        {
            int ret = faceSDK.Activate(LicenseLabel.Text);
            DisplayAlert("check", ret.ToString(), "ok");
        }

        private void OnGetMachineCode(object sender, EventArgs e)
        {

            MachineCodeLabel.Text = faceSDK.GetHardwareId();
            SemanticScreenReader.Announce(MachineCodeLabel.Text);

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

        public async Task<FileResult> PickAndShow(PickOptions options)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(options);
                if (result != null)
                {
                    if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                        result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                    {
                        FaceImage.Source = ImageSource.FromFile(result.FullPath);

                        // Read local image, convert it to byte array, and get image size
                        string imagePath = result.FullPath;

                        System.Drawing.Image image = null;
                        try
                        {
                            image = LoadImageWithExif(imagePath);
                        }
                        catch (Exception)
                        {
                            DisplayAlert("test", "Unknown Format!", "ok");
                            return null;
                        }

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

                        string resultString = "{\n";

                        for (int i = 0; i < faceCount; i++)
                        {
                            resultString += "x1: " + resultBoxes[i].x1.ToString() + ",\n";
                            resultString += "y1: " + resultBoxes[i].y1.ToString() + ",\n";
                            resultString += "x2: " + resultBoxes[i].x2.ToString() + ",\n";
                            resultString += "y2: " + resultBoxes[i].y2.ToString() + ",\n";
                            resultString += "livenessConfidence: " + resultBoxes[i].liveness.ToString() + ",\n";
                        }

                        resultString += "}\n";

                        ResultEditor.Text = resultString;

                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
            }

            return null;
        }
    }
}
