using System.Runtime.InteropServices;
using UsbWatcherUi.Utils;


namespace UsbWatcherUi
{
    public partial class AppForm : Form
    {
        private const int WM_DEVICECHANGE = 0x0219;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr notificationFilter, int flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnregisterDeviceNotification(IntPtr handle);

        private readonly IntPtr notificationHandle;

        public AppForm(StringAppendLogger? logger, UsbDeviceUtils usbDeviceUtils)
        {
            InitializeComponent();

            UsbDeviceUtils = usbDeviceUtils;

            if (logger != null)
            {
                logger.LogUpdated += (log, newLogLine) => textBox1.Text = log.ToString();
            }

            notificationHandle = RegisterDeviceNotification(Handle, IntPtr.Zero, 0x000000B0);

            FormClosed += (_, _) => Application.Exit();

            UsbDeviceUtils.GetUsbDeviceStatus();
        }

        private UsbDeviceUtils UsbDeviceUtils { get; }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_DEVICECHANGE)
            {
                //    int wParam = m.WParam.ToInt32(); // device-change event
                //    int lParam = m.LParam.ToInt32();
                //    int msg = m.Msg;
                //    string debugText = $"usbDeviceStatus: {usbDeviceStatus}; _lastUsbDeviceStatus: {_lastUsbDeviceStatus}; wParam: {wParam}; lParam: {lParam}; msg: {msg}";

                UsbDeviceUtils.HandleDeviceChange();
            }
        }

        protected void Dispose1(bool disposing)
        {
            if (disposing)
            {
                UnregisterDeviceNotification(notificationHandle);
            }
            base.Dispose(disposing);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void AppForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                ShowInTaskbar = false;
            }
            else
            {
                ShowInTaskbar = true;
            }
        }
    }
}