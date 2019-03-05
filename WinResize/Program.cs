using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinResize
{
    class Program
    {
        static int SW_MAXIMIZE = 3;

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int X);

        [DllImport("user32.dll")]
        public static extern bool SetFocus(IntPtr hWnd);



        [DllImport("user32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        // Define the SetWindowPos API function
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        // Define the SetWindowPosFlags enumeration.
        [Flags()]
        private enum SetWindowPosFlags : uint
        {
            SynchronousWindowPosition = 0x4000,
            DeferErase = 0x2000,
            DrawFrame = 0x0020,
            FrameChanged = 0x0020,
            HideWindow = 0x0080,
            DoNotActivate = 0x0010,
            DoNotCopyBits = 0x0100,
            IgnoreMove = 0x0002,
            DoNotChangeOwnerZOrder = 0x0200,
            DoNotRedraw = 0x0008,
            DoNotReposition = 0x0200,
            DoNotSendChangingEvent = 0x0400,
            IgnoreResize = 0x0001,
            IgnoreZOrder = 0x0004,
            ShowWindow = 0x0040,
        }

        static void Main(string[] args)
        {
            Console.Title = "WinResize";
            //Read parameters from command line
            if (args.Count() == 0)
            {
                Console.WriteLine("Usage: sleep_time(ms) process_name,args [process_name,args ... (max 9 => 3x3)]");
                return;
            }

            int sleepTime = int.Parse(args[0]);
            int windowNumber = args.Count() - 1;
            List<Process> procs = new List<Process>();
            //Launch process
            for (int n = 1; n < 1 + windowNumber; ++n)
                {
                Process p = new Process();
                p.StartInfo.FileName = args[n].Split(',')[0];
                p.StartInfo.Arguments = args[n].Split(',')[1];
                p.Start();
                procs.Add(p);
                Console.WriteLine("Launching " + args[n] + " " + (n - 1) + "/" + windowNumber);
                System.Threading.Thread.Sleep(sleepTime);
            }

            Console.WriteLine("Fixing windows");
            //Process parameters
            int windowWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int windowHeight = Screen.PrimaryScreen.WorkingArea.Height;
            int xtiles = 1;
            int ytiles = 1;
            int dx0 = 0;
            int dx1 = 0;
            int dy0 = 0;
            int dy1 = 0;
            int[] configs= {
            //  x, dx0, dx1     y, dy0, dy1
                1,   0,   0,    1,   0,   0,
                2,  -8,  15,    1,   0,   5,
                2,  -9,  16,    2,   0,   8,
                3,  -5,  15,    2,   0,   8,
                3,  -15, 30,    3,   0,  40 // crap
            };
            for (int c = 0; c < 6 * 5; c += 6)
            {
                xtiles = configs[c];
                ytiles = configs[c + 3];
                //Console.WriteLine((float)(windowNumber / xtiles / ytiles) + " from " + windowNumber + " " + xtiles + " " + ytiles);
                if ((float) windowNumber / xtiles / ytiles <= 1)
                {
                    dx0 = configs[c + 1];
                    dx1 = configs[c + 2];
                    dy0 = configs[c + 4];
                    dy1 = configs[c + 5];
                    break;
                }; 
            };
            Console.WriteLine("Tiling " + xtiles + " x " + ytiles);
            int i = 0;
            int j = 0;
            foreach (Process proc in procs)
            {
                int x = windowWidth * i / xtiles;
                int y = windowHeight * j / ytiles;
                int width = windowWidth / xtiles;
                int height = windowHeight / ytiles;
                Console.WriteLine("tile" + i + " " + j + " at " + x + " " + y + " with size " + width + " " + height);
                while (string.IsNullOrEmpty(proc.MainWindowTitle) && !proc.HasExited) // wait for window to appear
                {
                    System.Threading.Thread.Sleep(100);
                    proc.Refresh();
                }
                IntPtr handle = proc.MainWindowHandle;
                // ShowWindow(handle, SW_MAXIMIZE);
                // Neither SetWindowPos nor MoveWindow work exactly, thus dx/dy "corretions"
                MoveWindow(handle, x + dx0, y + dy0, width + dx1, height + dy1, true);
                //SetWindowPos(handle, IntPtr.Zero, x + dx0, y + dy0, width + dx1, height + dy1, 0);
                SetFocus(handle);
                if (i == xtiles - 1)
                {
                    j++;
                    i = 0;
                }
                else
                {
                    i++;
                }
            }

            Console.WriteLine("Done");
            //Console.ReadKey();
        }
    }
}
