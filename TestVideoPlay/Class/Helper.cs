using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TestVideoPlay.Class
{
    class Helper
    {
        public static void DisableScreenSaver()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
        }
        public static void EnableScreenSaver()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }

        [DllImport("kernel32.dll")]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
        [FlagsAttribute]
        enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        const int SW_RESTORE = 9;

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);
        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);
        [DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);
    }
}

