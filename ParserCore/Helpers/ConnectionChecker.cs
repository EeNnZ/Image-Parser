using System.Runtime.InteropServices;

namespace ParserCore.Helpers;
public static class ConnectionChecker
{
    [DllImport("wininet.dll")]
    private static extern bool InternetGetConnectedState(out int description, int reservedValue);
    public static bool CheckIfConnected()
    {
        return InternetGetConnectedState(out int desc, 0);
    }
}