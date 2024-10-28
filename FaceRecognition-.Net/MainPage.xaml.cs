using FaceSDK;
using Microsoft.Extensions.Logging.Abstractions;
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
        string image_path;
        string image_path1;
        string image_path2;
        FaceEngineClass faceSDK = new FaceEngineClass();

        public MainPage()
        {
            InitializeComponent();

            var dictPath = $"{AppDomain.CurrentDomain.BaseDirectory}assets";
            int ret = faceSDK.Init(dictPath);
            if (ret != (int)SDK_STATUS.SDK_SUCCESS) {
                DisplayAlert("check", "Failed to init SDK", "ok");
            }

            // *** call async function from sync function
            // Task task = Task.Run(async () => await LoadMauiAsset());
            // var fullPath = System.IO.Path.Combine(FileSystem.AppDataDirectory,"MyFolder","myfile.txt");

        }

        async Task LoadMauiAsset()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("AboutAssets.txt");

            using var reader = new StreamReader(stream);

            var contents = reader.ReadToEnd();
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

                DetectFace(image_path);
            }
            catch (OperationCanceledException)
            {
                throw new OperationCanceledException("It was cancelled");
            }

        }

        private void DetectFace(string image_path)
        {
            System.Drawing.Image image = null;
            try
            {
                image = LoadImageWithExif(image_path);
            }
            catch (Exception)
            {
                DisplayAlert("test", "Unknown Format!", "ok");
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
                        image_path = result.FullPath;

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

        public async Task<FileResult> PickAndShow1(PickOptions options)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(options);
                if (result != null)
                {
                    if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                        result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                    {
                        FaceImage1.Source = ImageSource.FromFile(result.FullPath);

                        // Read local image, convert it to byte array, and get image size
                        image_path1 = result.FullPath;

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

        public async Task<FileResult> PickAndShow2(PickOptions options)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(options);
                if (result != null)
                {
                    if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                        result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                    {
                        FaceImage2.Source = ImageSource.FromFile(result.FullPath);

                        // Read local image, convert it to byte array, and get image size
                        image_path2 = result.FullPath;

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

        private async void OnSelectFace1Btn(object sender, EventArgs e)
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
                await PickAndShow1(options);
            }
            catch (OperationCanceledException)
            {
                throw new OperationCanceledException("It was cancelled");
            }

        }

        private async void OnSelectFace2Btn(object sender, EventArgs e)
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
                await PickAndShow2(options);
            }
            catch (OperationCanceledException)
            {
                throw new OperationCanceledException("It was cancelled");
            }

        }

        private async void OnCompareBtn(object sender, EventArgs e)
        {
            
            // Load first image
            System.Drawing.Image image1 = null;
            try
            {
                image1 = LoadImageWithExif(image_path1);
            }
            catch (Exception)
            {
                DisplayAlert("test", "Unknown Format!", "ok");
            }

            Bitmap imgBmp1 = ConvertTo24bpp(image1);
            BitmapData bitmapData1 = imgBmp1.LockBits(new Rectangle(0, 0, imgBmp1.Width, imgBmp1.Height), ImageLockMode.ReadWrite, imgBmp1.PixelFormat);

            int bytesPerPixel1 = Bitmap.GetPixelFormatSize(imgBmp1.PixelFormat) / 8;
            int byteCount1 = bitmapData1.Stride * imgBmp1.Height;
            byte[] pixels1 = new byte[byteCount1];
            IntPtr ptrFirstPixel1 = bitmapData1.Scan0;
            Marshal.Copy(ptrFirstPixel1, pixels1, 0, pixels1.Length);

            imgBmp1.UnlockBits(bitmapData1);
            
            // Load second image
            System.Drawing.Image image2 = null;
            try
            {
                image2 = LoadImageWithExif(image_path2);
            }
            catch (Exception)
            {
                DisplayAlert("test", "Unknown Format!", "ok");
            }

            Bitmap imgBmp2 = ConvertTo24bpp(image2);
            BitmapData bitmapData2 = imgBmp2.LockBits(new Rectangle(0, 0, imgBmp2.Width, imgBmp2.Height), ImageLockMode.ReadWrite, imgBmp2.PixelFormat);

            int bytesPerPixel2 = Bitmap.GetPixelFormatSize(imgBmp2.PixelFormat) / 8;
            int byteCount2 = bitmapData2.Stride * imgBmp2.Height;
            byte[] pixels2 = new byte[byteCount2];
            IntPtr ptrFirstPixel2 = bitmapData2.Scan0;
            Marshal.Copy(ptrFirstPixel2, pixels2, 0, pixels2.Length);

            imgBmp2.UnlockBits(bitmapData2);
            

            if (image_path1 != null && image_path2 != null)
            {
                float[] similarity = new float[1];
                int ret = faceSDK.Compare(pixels1, imgBmp1.Width, imgBmp1.Height, bitmapData1.Stride, pixels2, imgBmp2.Width, imgBmp2.Height, bitmapData2.Stride, similarity);

                string resultString = "similarity: " + similarity[0].ToString();

                ResultEditor1.Text = resultString;
            }
        }
    }
}
