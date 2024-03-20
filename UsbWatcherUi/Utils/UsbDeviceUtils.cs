using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Management;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace UsbWatcherUi.Utils;

public enum UsbDeviceStatus
{
    Connected,
    Disconnected,
}

public class UsbDeviceUtils
{
    public UsbDeviceUtils(ILogger logger, UsbDeviceConfig usbDeviceConfig, DisplayUtils displayUtils)
    {
        Logger = logger;
        UsbDeviceConfig = usbDeviceConfig;
        DisplayUtils = displayUtils;
    }

    private ILogger Logger { get; }
    private UsbDeviceConfig UsbDeviceConfig { get; }
    private DisplayUtils DisplayUtils { get; }

    private DateTime lastHandleDeviceChangeRun = DateTime.MinValue;
    private UsbDeviceStatus lastUsbDeviceStatus;

    public void HandleDeviceChange()
    {
        DateTime now = DateTime.Now;
        if ((now - lastHandleDeviceChangeRun).TotalSeconds < UsbDeviceConfig.DebounceSeconds)
        {
            return;
        }

        lastHandleDeviceChangeRun = now;

        UsbDeviceStatus usbDeviceStatus = UsbDeviceStatus.Connected;
        for (int i = 0; i < 10; i++)
        {
            Thread.Sleep(400);
            //usbDeviceStatus = GetUsbDeviceStatusPnpUtil();
            usbDeviceStatus = GetUsbDeviceStatus();
            if (usbDeviceStatus == lastUsbDeviceStatus)
            {
                continue;
            }
            else
            {
                break;
            }
        }

        Logger?.LogDebug($"usbDeviceStatus={usbDeviceStatus}; lastUsbDeviceStatus={lastUsbDeviceStatus};");

        if (usbDeviceStatus != lastUsbDeviceStatus)
        {
            DisplayUtils.SwitchDisplayInput(usbDeviceStatus);
        }

        lastUsbDeviceStatus = usbDeviceStatus;
    }

    public UsbDeviceStatus GetUsbDeviceStatus() => GetUsbDeviceStatusWmi();

    private UsbDeviceStatus GetUsbDeviceStatusPnpUtil()
    {
        ProcessStartInfo processStartInfo = new("pnputil", $"/enum-devices /instanceid \"{UsbDeviceConfig.UsbDeviceId}\" /ids")
        {
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
        Process process = Process.Start(processStartInfo) ?? throw new InvalidOperationException("Could not start pnputil");
        process.WaitForExit();
        string output = process.StandardOutput.ReadToEnd();
        string usbDeviceStatusStr = Regex.Replace(output, "^.+Status:\\s+(\\w+)\\r?\\n.+$", "$1", RegexOptions.Singleline);
        Logger?.LogDebug(usbDeviceStatusStr);
        return usbDeviceStatusStr == "Started" ? UsbDeviceStatus.Connected : UsbDeviceStatus.Disconnected; ;
    }

    private UsbDeviceStatus GetUsbDeviceStatusWmi()
    {
        var deviceFound = false;

        var query = $"SELECT * FROM Win32_PnPEntity WHERE DeviceID=\"{UsbDeviceConfig.UsbDeviceId?.Replace("\\", "\\\\")}\"";
        //Logger?.LogDebug($"GetUsbDeviceStatusWmi query: {query}");
        Thread thread = new(() =>
        {
            using ManagementObjectSearcher theSearcher = new(query);
            foreach (ManagementObject usbDevice in theSearcher.Get())
            {
                deviceFound = true;
                usbDevice.Dispose();
                break;
            }
        });
        thread.Start();
        thread.Join();

        var usbDeviceStatus = deviceFound ? UsbDeviceStatus.Connected : UsbDeviceStatus.Disconnected;
        Logger?.LogDebug($"GetUsbDeviceStatusWmi usbDeviceStatus: {usbDeviceStatus}");
        return usbDeviceStatus;
    }
}
