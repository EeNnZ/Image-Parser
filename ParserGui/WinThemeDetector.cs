using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace ParserGui
{
    [SupportedOSPlatform("windows")]
    public static  class WinThemeDetector
    {
        [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
        private static extern bool ShouldSystemUseDarkMode();
        public static bool ShouldUseDarkMode()
        {
            try
            {
                return ShouldSystemUseDarkMode();
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
