using System.Runtime.InteropServices;
using System.Runtime.Versioning;
namespace ParserCore.Helpers;
[SupportedOSPlatform("windows")]
internal static class DisplayResolution
{
    public static string Resolution => GetResolution();
    private static string GetResolution()
    {
        int width = GetSystemMetrics(0);
        int height = GetSystemMetrics(1);
        return $"{width}x{height}";
    }
    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    private static extern int GetSystemMetrics(int nIndex);
}