using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;


//Most of the core logic work was done by me.
//The system tray icon stuff was totally lifted from this: http://www.codeproject.com/Articles/290013/Formless-System-Tray-Application

namespace keeponscreener
{
    /// <summary>
    /// 
    /// </summary>
    static class Program
    {
        //declaring globals as convention recommends: http://stackoverflow.com/a/2445441/843000
        public static class Globals
        {
            public static Screen StartScreen;
            public static ProcessIcon pi = new ProcessIcon();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Show the system tray icon.
            //using (ProcessIcon pi = new ProcessIcon())
            //{
            Globals.pi.Display();

            //http://stackoverflow.com/a/9680911/843000
            IntPtr hhook = SetWinEventHook(EVENT_SYSTEM_MOVESIZESTART, EVENT_SYSTEM_MOVESIZESTART, IntPtr.Zero, procDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
            IntPtr hhook2 = SetWinEventHook(EVENT_SYSTEM_MOVESIZEEND, EVENT_SYSTEM_MOVESIZEEND, IntPtr.Zero, procDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);

            //Make sure the application runs!
            //This is our message pipe, no need for: MessageBox/GetMessage/TranslateMessage/DispatchMessage/MsgWaitForMultipleObjectsEx
            Application.Run();

            Globals.pi.Dispose();

            UnhookWinEvent(hhook);
            UnhookWinEvent(hhook2);
            //}
        }


        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr
           hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
           uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        //http://source.winehq.org/source/include/winuser.h
        //http://pinvoke.net/default.aspx/Constants/WM.html
        //http://pinvoke.net/default.aspx/user32/SetWinEventHook.html
        const uint WINEVENT_OUTOFCONTEXT = 0x0000;
        const uint EVENT_SYSTEM_MOVESIZESTART = 0x000A;
        const uint EVENT_SYSTEM_MOVESIZEEND = 0x000B;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

            public int X
            {
                get { return Left; }
                set { Right -= (Left - value); Left = value; }
            }

            public int Y
            {
                get { return Top; }
                set { Bottom -= (Top - value); Top = value; }
            }

            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(Left, Top); }
                set { X = value.X; Y = value.Y; }
            }

            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
                set { Width = value.Width; Height = value.Height; }
            }

            public static implicit operator System.Drawing.Rectangle(RECT r)
            {
                return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator RECT(System.Drawing.Rectangle r)
            {
                return new RECT(r);
            }

            public static bool operator ==(RECT r1, RECT r2)
            {
                return r1.Equals(r2);
            }

            public static bool operator !=(RECT r1, RECT r2)
            {
                return !r1.Equals(r2);
            }

            public bool Equals(RECT r)
            {
                return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
            }

            public override bool Equals(object obj)
            {
                if (obj is RECT)
                    return Equals((RECT)obj);
                else if (obj is System.Drawing.Rectangle)
                    return Equals(new RECT((System.Drawing.Rectangle)obj));
                return false;
            }

            public override int GetHashCode()
            {
                return ((System.Drawing.Rectangle)this).GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        static WinEventDelegate procDelegate = new WinEventDelegate(WinEventProc);

        /// <summary>
        /// The MoveWindow function changes the position and dimensions of the specified window. For a top-level window, the position and dimensions are relative to the upper-left corner of the screen. For a child window, they are relative to the upper-left corner of the parent window's client area.
        /// </summary>
        /// <param name="hWnd">Handle to the window.</param>
        /// <param name="X">Specifies the new position of the left side of the window.</param>
        /// <param name="Y">Specifies the new position of the top of the window.</param>
        /// <param name="nWidth">Specifies the new width of the window.</param>
        /// <param name="nHeight">Specifies the new height of the window.</param>
        /// <param name="bRepaint">Specifies whether the window is to be repainted. If this parameter is TRUE, the window receives a message. If the parameter is FALSE, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of moving a child window.</param>
        /// <returns>If the function succeeds, the return value is nonzero.
        /// <para>If the function fails, the return value is zero. To get extended error information, call GetLastError.</para></returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);


        static void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            // filter out non-HWND namechanges... (eg. items within a listbox)
            if (idObject != 0 || idChild != 0)
            {
                return;
            }

            if (eventType == EVENT_SYSTEM_MOVESIZESTART)
            {
                //set the Screen in which the drag starts to a globally accessible variable
                Globals.StartScreen = Screen.FromHandle(hwnd);
            }

            if (eventType == EVENT_SYSTEM_MOVESIZEEND)
            {
                //we assume we don't want to move the window
                bool MoveTheWindow = false;

                RECT rct = new RECT();
                GetWindowRect(hwnd, ref rct); //returns info about window on virtual screen

                Rectangle totalsizeofintersection = new Rectangle();
                int ScreenIntersectionCount = 0;

                foreach (Screen screen in Screen.AllScreens)
                {
                    //check to see if the window's reported Rectangle.Intersect() is not the rct.Size
                    Rectangle hwndinterwithscreen = Rectangle.Intersect(rct, screen.Bounds);

                    // here we should consider if the checkbox is checked on the contextmenu item: "hwnd_offscreen"
                    if (!Globals.pi.GetMenuItemCheckedStatus("hwnd_offscreen"))
                    {
                        //then we don't want to allow windows off screen
                        if (hwndinterwithscreen.Size != rct.Size && hwndinterwithscreen.Height != 0)
                        {
                            MoveTheWindow = true;
                        }
                    }
                    if (!Globals.pi.GetMenuItemCheckedStatus("hwnd_spanscreen"))
                    {
                        //then we don't want windows to span screens
                        if (hwndinterwithscreen.Size.Height > 0)
                        {
                            ScreenIntersectionCount++;
                        }

                        if (ScreenIntersectionCount > 1)
                        {
                            //we're spannin' 
                            //and i hope you like spannin' too
                            MoveTheWindow = true;
                        }
                    }
                }

                


                if (MoveTheWindow)
                {

                    /*
                     * find x,y of window
                     * find the edge of the ScreenWithWindow:
                     *  there is an, X and Y given for each edge, declared by .bounds.x,.y,.height,.width) of the ScreenWithWindow that the window is closest to.
                     *  
                     * You must understand which screens the window spans.
                     * 
                     * if the edge is the right edge, consider the window width when moving
                     * if the edge is the bottom edge consider the window height when moving
                     * 
                     * move to the edge
                     */

                    Screen ScreenWithWindow = Screen.FromHandle(hwnd);
                    Rectangle ScreenWithWindowBounds = new Rectangle();

                    if (ScreenWithWindow.Primary)
                    {
                        ScreenWithWindowBounds = ScreenWithWindow.WorkingArea;
                    }
                    else
                    {
                        ScreenWithWindowBounds = ScreenWithWindow.Bounds;
                    }

                    Rectangle hwndinterwithScreenWithWindow = Rectangle.Intersect(rct, ScreenWithWindowBounds);

                    //using a bitwise value to gather which sides the window is intersecting
                    int screensideintersect = 0;

                    if (hwndinterwithScreenWithWindow.Left == ScreenWithWindowBounds.Left)
                    {
                        //the window is spanning from the Left
                        //check if height of rct and height of hwndinterwithScreenWithWindow are the same
                        // this could mean that it is above or below
                        screensideintersect = screensideintersect + 0x0001;
                    }
                    if (hwndinterwithScreenWithWindow.Right == ScreenWithWindowBounds.Right)
                    {
                        //the window is spanning from the Right
                        screensideintersect = screensideintersect + 0x0002;
                    }
                    if (hwndinterwithScreenWithWindow.Top == ScreenWithWindowBounds.Top)
                    {
                        //the window is spanning from the top
                        screensideintersect = screensideintersect + 0x0004;
                    }
                    if (hwndinterwithScreenWithWindow.Bottom == ScreenWithWindowBounds.Bottom)
                    {
                        //the window is spanning from the bottom
                        screensideintersect = screensideintersect + 0x0008;
                    }

                    //consider the bitwise for the window edge span values
                    switch (screensideintersect)
                    {
                        case 1: //left
                            //then the window only spans the left side of the ScreenWithWindow
                            //Snap to the left side of ScreenWithWindow, keeping the same Y
                            //http://msdn.microsoft.com/en-us/library/windows/desktop/ms632599%28v=vs.85%29.aspx#functions
                            //using SetWindowPos() or MoveWindow():
                            // http://stackoverflow.com/a/4631747/843000

                            //for all the following conditions:
                            //  must also consider ScreenWithWindow.Bounds.Left would be negative if the screenwithwindow is to the left of the primary screen
                            MoveWindow(hwnd, ScreenWithWindowBounds.Left, rct.Top, rct.Width, rct.Height, true);
                            break;
                        case 2: //right
                            MoveWindow(hwnd, ScreenWithWindowBounds.Right - rct.Width, rct.Top, rct.Width, rct.Height, true);
                            break;
                        case 3: //left and right
                            MoveWindow(hwnd, ScreenWithWindowBounds.Left, rct.Top, ScreenWithWindowBounds.Width, rct.Height, true);
                            break;
                        case 4: //top
                            MoveWindow(hwnd, rct.X, rct.Top, rct.Width, rct.Height, true);
                            break;
                        case 5: //left and top
                            MoveWindow(hwnd, ScreenWithWindowBounds.Left, rct.Top, rct.Width, rct.Height, true);
                            break;
                        case 6: //right and top
                            MoveWindow(hwnd, ScreenWithWindowBounds.Left, rct.Top, rct.Width, rct.Height, true);
                            break;
                        case 7: //left, right and top
                            MoveWindow(hwnd, ScreenWithWindowBounds.Left, ScreenWithWindowBounds.Top, ScreenWithWindowBounds.Width, rct.Height, true);
                            break;
                        case 8: //bottom
                            MoveWindow(hwnd, rct.X, ScreenWithWindowBounds.Bottom - rct.Height, rct.Width, rct.Height, true);
                            break;
                        case 9: //left and bottom
                            MoveWindow(hwnd, ScreenWithWindowBounds.Left, ScreenWithWindowBounds.Bottom - rct.Height, rct.Width, rct.Height, true);
                            break;
                        case 10: //right and bottom
                            MoveWindow(hwnd, ScreenWithWindowBounds.Right - rct.Width, ScreenWithWindowBounds.Bottom - rct.Height, rct.Width, rct.Height, true);
                            break;
                        case 11: //left right and bottom
                            MoveWindow(hwnd, ScreenWithWindowBounds.Left, ScreenWithWindowBounds.Bottom - rct.Height, ScreenWithWindowBounds.Width, rct.Height, true);
                            break;
                        case 12: //top and bottom
                            MoveWindow(hwnd, rct.X, ScreenWithWindowBounds.Top, rct.Width, ScreenWithWindowBounds.Height, true);
                            break;
                        case 13: //left top and bottom
                            MoveWindow(hwnd, ScreenWithWindowBounds.Left, rct.Top, rct.Width, ScreenWithWindowBounds.Height, true);
                            break;
                        case 14: // right top and bottom
                            MoveWindow(hwnd, ScreenWithWindowBounds.Left - rct.Width, rct.Top, rct.Width, ScreenWithWindowBounds.Height, true);
                            break;
                        case 15: // left right top and bottom
                            MoveWindow(hwnd, ScreenWithWindowBounds.Left, ScreenWithWindowBounds.Top, ScreenWithWindowBounds.Width, ScreenWithWindowBounds.Height, true);
                            break;
                    }
                }
            }
        }
    }
}