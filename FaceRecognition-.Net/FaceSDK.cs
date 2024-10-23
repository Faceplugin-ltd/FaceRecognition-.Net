using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FaceSDK
{
    public class FaceEngineClass
    {
        public FaceEngineClass()
        {

        }

        [DllImport("FacepluginSDK.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getMachineCode();

        public String GetMachineCode()
        {
            try
            {
                IntPtr machineCode = getMachineCode();
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

    }
}
