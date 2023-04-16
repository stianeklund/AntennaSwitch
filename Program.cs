using System.Runtime.InteropServices;
using AntennaSwitch.View;
using Terminal.Gui;

namespace AntennaSwitch;

public static class Program
{
    private const string SwitchIp = "192.168.8.140";
    private static AntennaSwitchWindow? _window;
    private static void Main()
    {
        Application.Init();
        Console.Clear();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Console.SetWindowSize(48, 7);

        var sw = new AntennaSwitchClient(SwitchIp);
        var bd = new BandDecoder(sw);
        _window = new AntennaSwitchWindow(sw, bd);

        sw.GetSelectedAntennaFromSwitch(AntennaSwitchClient.Direction.None);

        Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(1000), _ =>
        {
            try
            {
                _window.UpdateValues();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error updating the window with new values: {e.Message} {e.StackTrace}");
                Application.RequestStop();
            }

            return true;
        });

        Application.Run(_window);
    }
}