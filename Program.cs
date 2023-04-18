using System.Runtime.InteropServices;
using AntennaSwitch.View;
using Terminal.Gui;

namespace AntennaSwitch;

public static class Program
{
    public const string FallbackIp = "192.168.4.1";
    private const string SwitchIp = "192.168.8.140";
    private static AntennaSwitchWindow? _window;

    private static void Main()
    {
        Application.Init();
        Console.Clear();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Console.SetWindowSize(48, 7);

        var sw = new AntennaSwitchClient(SwitchIp);
        var bd = new BandDecoder(sw, true);
        _window = new AntennaSwitchWindow(bd);

        Task.Run(async () =>
        {
            sw.SelectedAntenna = await sw.GetSelectedAntennaFromSwitch(AntennaSwitchClient.Direction.None);
        });

        Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(1000), _ =>

        {
            try
            {
                _window.UpdateValues(bd);
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