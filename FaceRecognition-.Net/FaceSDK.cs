using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FaceSDK
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ResultBox
    {
        public int x1, y1, x2, y2;

        public float yaw, roll, pitch;
        public float face_quality, face_luminance, eye_dist;

        public float left_eye_closed, right_eye_closed, face_occlusion, mouth_opened;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 68 * 2)]
        public float[] landmark_68; // Array of 136 floats

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2048)]
        public byte[] templates; // Array of 2048 bytes

        public ResultBox(int n)
        {
            x1 = x2 = y1 = y2 = 0;
            yaw = roll = pitch = 0;
            face_quality = face_luminance = eye_dist = 0;
            left_eye_closed = right_eye_closed = face_occlusion = mouth_opened = 0;
            templates = new byte[2056];
            landmark_68 = new float[68 * 2];
        }
    };

    public class FaceEngineClass
    {
        public FaceEngineClass()
        {

        }

        [DllImport("FacepluginSDK.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Faceplugin_get_hardware_id();

        public String GetHardwareId()
        {
            try
            {
                IntPtr machineCode = Faceplugin_get_hardware_id();
                if (machineCode == null)
                    throw new Exception("Failed to retrieve machine code.");

                string strMachineCode = Marshal.PtrToStringAnsi(machineCode);
                return strMachineCode;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        [DllImport("FacepluginSDK.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Faceplugin_init(IntPtr modelPath);

        public int Init(string modelPath)
        {
            return Faceplugin_init(Marshal.StringToHGlobalAnsi(modelPath));
        }

        [DllImport("FacepluginSDK.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Faceplugin_detect(
            IntPtr rgbData, // Pointer to the RGB data
            int width,      // Width of the image
            int height,     // Height of the image
            int stride,     // Stride of the image
            [In, Out] ResultBox[] resultBoxes, // Array of ResultBox
            int maxCount // Number of face boxes
        );

        public int DetectFace(byte[] rgbData, int width, int height, int stride, [In, Out] ResultBox[] faceBoxes, int faceBoxCount)
        {
            IntPtr imgPtr = Marshal.AllocHGlobal(rgbData.Length);
            Marshal.Copy(rgbData, 0, imgPtr, rgbData.Length);

            try
            {
                int ret = Faceplugin_detect(imgPtr, width, height, stride, faceBoxes, faceBoxCount);
                return ret;
            }
            finally
            {
                Marshal.FreeHGlobal(imgPtr);
            }
        }

    }
}
