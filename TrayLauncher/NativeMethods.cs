using System.Runtime.InteropServices;

namespace TrayLauncher
{
    internal static class NativeMethods
    {
        // Get double-click delay time in milliseconds
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern int GetDoubleClickTime();
    }
}
