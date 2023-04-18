using ManagedNativeWifi;

namespace AntennaSwitch;

public abstract class Networking
{
    public static bool IsConnected { get; set; }

    public static void ConnectToFallBackNetwork()
    {
        var ant = NativeWifi
            .EnumerateAvailableNetworks().Where(v => !string.IsNullOrEmpty(v.ProfileName))
            .FirstOrDefault(v => v.Ssid.ToString() == "ESP-DC01A7");
        try
        {
            if (ConnectAsync(ant).Result) IsConnected = true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"ConnectToFallBackNetwork():{e}");
            throw;
        }
    }

    private static async Task<bool> ConnectAsync(AvailableNetworkPack? availableNetwork)
    {
        if (availableNetwork is null)
            return false;

        return await NativeWifi.ConnectNetworkAsync(
            availableNetwork.Interface.Id,
            availableNetwork.ProfileName,
            availableNetwork.BssType,
            TimeSpan.FromSeconds(10));
    }
}