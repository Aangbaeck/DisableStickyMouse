using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading.Tasks;
using CSharpLib;
namespace DisableStickyCorners
{
    class Program
    {
        //Hide console window
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;


        //Last position
        static int x, y;
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);
        const int ENUM_CURRENT_SETTINGS = -1;

        public class Screen
        {
            public int Top { get; set; }
            public int Left { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }


        static void Main(string[] args)
        {

            var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\DisableStickyCorners.exe";
            CSharpLib.Shortcut shortcut = new CSharpLib.Shortcut();
            var startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            shortcut.CreateShortcutToFile(path, startupPath + "\\DisableStickyCorners.lnk");

            //If app already running, kill process.
            if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly()?.Location)).Length > 1) System.Diagnostics.Process.GetCurrentProcess().Kill();

            // Hide console window
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            int stickyCornersHeight = 6; //Sticky corners are 5 pixels high.
            List<Screen> CornerPositions = new List<Screen>();
            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
            {
                DEVMODE dm = new DEVMODE();
                dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                EnumDisplaySettings(screen.DeviceName, ENUM_CURRENT_SETTINGS, ref dm);

                Console.WriteLine($"Device: {screen.DeviceName}");
                Console.WriteLine($"Top: {screen.Bounds.Top}, L:{screen.Bounds.Left}, R:{screen.Bounds.Right}");
                Console.WriteLine();

                CornerPositions.Add(new Screen() { Top = screen.Bounds.Top, Right = screen.Bounds.Right, Left = screen.Bounds.Left, Bottom = screen.Bounds.Bottom});
            }



            Task.Run(async () =>
            {
                while (!Console.KeyAvailable)
                {
                    if (GetCursorPos(out var point) && (point.X != x || point.Y != y))
                    {
                        x = point.X;
                        y = point.Y;
                        //Console.WriteLine("({0},{1})", point.X, point.Y);
                        //Find what screen we are on and see if we should move the mouse cursor ++ or --
                        foreach (var cornerPosition in CornerPositions)
                        {
                            if (point.Y >= cornerPosition.Top - stickyCornersHeight || point.Y <= cornerPosition.Bottom + stickyCornersHeight)
                                if (point.X == cornerPosition.Left && point.X != 0)
                                {
                                    Console.WriteLine($"(Corner! : {point.X},{point.Y})");
                                    x--;
                                    SetCursorPos(x, y);
                                    break;
                                }
                                else if (point.X == cornerPosition.Right - 1)
                                {
                                    Console.WriteLine($"(Corner! : {point.X},{point.Y})");
                                    x++;
                                    SetCursorPos(x, y);
                                    break;
                                }
                        }
                    }
                    await Task.Delay(20);
                }
            });
            Console.ReadKey();
        }

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }


    }

    public struct POINT
    {
        public int X;
        public int Y;
    }


}
