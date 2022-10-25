using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace ParserCore
{
    public static class ConnectionChecker
    {
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);
        public static async Task<bool> CheckIfConnected() => await Task.Run(() => InternetGetConnectedState(out int Desc, 0));
    }
}
