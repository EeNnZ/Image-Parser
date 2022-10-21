using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Scanner.Parts
{
    public static class Notepad
    {
        #region Imports
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string? lpszWindow);
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        private static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

        //this is a constant indicating the window that we want to send a text message
        const int WM_SETTEXT = 0X000C;
        #endregion

        public static void SendText(string text)
        {
            Process notepad = Process.Start(@"notepad.exe");
            Thread.Sleep(300);
            IntPtr notepadTextbox = FindWindowEx(notepad.MainWindowHandle, IntPtr.Zero, "Edit", null);
            SendMessage(notepadTextbox, WM_SETTEXT, 0, text);
        }
    }

}
