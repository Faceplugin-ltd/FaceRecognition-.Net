using FaceSDK;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System.Reflection;

namespace FaceRecognition_.Net
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
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
            FaceEngineClass faceSDK = new FaceEngineClass();

            MachineCodeLabel.Text = faceSDK.GetMachineCode();
            SemanticScreenReader.Announce(MachineCodeLabel.Text);

            ImageCaption.Source = ImageSource.FromFile("dotnet_bot.png");

            // Read local image, convert it to byte array, and get image size
            string imagePath = "E:/1.jpg";
            byte[] imageBytes;
            int imageWidth, imageHeight;

            using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    imageBytes = memoryStream.ToArray();
                }
            }

            // Get image size
            using (var image = System.Drawing.Image.FromFile(imagePath))
            {
                imageWidth = image.Width;
                imageHeight = image.Height;
            }

        }


    }

}
