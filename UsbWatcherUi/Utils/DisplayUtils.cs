using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace UsbWatcherUi.Utils;

public class DisplayUtils
{
    public DisplayUtils(ILogger logger, DisplayConfig displayConfig)
    {
        Logger = logger;
        DisplayConfig = displayConfig;
    }

    private ILogger Logger { get; }
    private DisplayConfig DisplayConfig { get; }

    public void SwitchDisplayInput(UsbDeviceStatus usbDeviceStatus)
    {
        string input = usbDeviceStatus == UsbDeviceStatus.Connected
            ? DisplayConfig.UsbDeviceStatusConnected_DisplayInputSource!
            : DisplayConfig.UsbDeviceStatusDisconnected_DisplayInputSource!;

        // TODO Vlad Shcherbyna: get current input, and if the same, don't do anything

        var exe = DisplayConfig.ControlMyMonitor_Exe!;
        var exeParams = DisplayConfig.ControlMyMonitor_Params!.Replace("{input}", input);
        Logger?.LogDebug($"SwitchDisplayInput to input: {exe} {exeParams}");
        ProcessStartInfo processStartInfo = new(exe, exeParams)
        {
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
        Process.Start(processStartInfo);
    }
}
