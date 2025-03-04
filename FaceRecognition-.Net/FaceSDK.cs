using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FaceSDK
{
    enum SDK_STATUS
    {
        SDK_SUCCESS = 0,
        SDK_LICENSE_KEY_ERROR = -1,
        SDK_LICENSE_APPID_ERROR = -2,
        SDK_LICENSE_EXPIRED = -3,
        SDK_NO_ACTIVATED = -4,
        SDK_INIT_ERROR = -5,
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ResultBox
    {
        public int x1, y1, x2, y2;

        public float liveness;
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
            liveness = 0;
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
        public static extern int Faceplugin_activate(IntPtr license);

        public int Activate(String license)
        {
            IntPtr ptr = Marshal.StringToHGlobalAnsi(license);

            try
            {
                return Faceplugin_activate(ptr);
            }
            finally
            {
                // Free the unmanaged memory when done
                Marshal.FreeHGlobal(ptr);
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

        [DllImport("FacepluginSDK.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Faceplugin_compare(
            IntPtr rgbData1, // Pointer to the RGB data
            int width1,      // Width of the image
            int height1,     // Height of the image
            int stride1,     // Stride of the image
            IntPtr rgbData2, // Pointer to the RGB data
            int width2,      // Width of the image
            int height2,     // Height of the image
            int stride2,     // Stride of the image
            [In, Out] float[] similarity // Array of ResultBox
        );

        public int Compare(byte[] rgbData1, int width1, int height1, int stride1, byte[] rgbData2, int width2, int height2, int stride2, [In, Out] float[] similarity)
        {
            IntPtr imgPtr1 = Marshal.AllocHGlobal(rgbData1.Length);
            Marshal.Copy(rgbData1, 0, imgPtr1, rgbData1.Length);

            IntPtr imgPtr2 = Marshal.AllocHGlobal(rgbData2.Length);
            Marshal.Copy(rgbData2, 0, imgPtr2, rgbData2.Length);

            try
            {
                int ret = Faceplugin_compare(imgPtr1, width1, height1, stride1, imgPtr2, width2, height2, stride2, similarity);
                return ret;
            }
            finally
            {
                Marshal.FreeHGlobal(imgPtr1);
                Marshal.FreeHGlobal(imgPtr2);
            }
        }

        [DllImport("FacepluginSDK.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Faceplugin_extract(
            IntPtr rgbData, // Pointer to the RGB data
            int width,      // Width of the image
            int height,     // Height of the image
            int stride,     // Stride of the image
            ResultBox faceBox, // Pointer to the RGB data
            [Out] float[] feature // Array of ResultBox
        );

        public int Extract(byte[] rgbData, int width, int height, int stride, ResultBox faceBox, float[] feature)
        {
            IntPtr imgPtr1 = Marshal.AllocHGlobal(rgbData.Length);
            Marshal.Copy(rgbData, 0, imgPtr1, rgbData.Length);

            try
            {
                int ret = Faceplugin_extract(imgPtr1, width, height, stride, faceBox, feature);
                return ret;
            }
            finally
            {
                Marshal.FreeHGlobal(imgPtr1);
            }
        }

        [DllImport("FacepluginSDK.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern float Faceplugin_similarity(
            [In] float[] feature1,
            [In] float[] feature2
        );

        public float Similarity(float[] feature1, float[] feature2)
        {

            try
            {
                float sim = Faceplugin_similarity(feature1, feature2);
                return sim;
            }
            finally
            {
            }
        }

    }
}
