namespace AntennaSwitch;

/// <summary>
///     A very WIP / hacky "client" for the LZ2RR web antenna switch
///     It'd be better to interface with the switch directly instead of WiFi but this does work.
///     There is no ptt protection or anything here.
/// </summary>
public class AntennaSwitchClient
{
    public enum AntennaType
    {
        EndFed = 4,
        Vertical = 3,
        Dipole = 2,
        Beam = 1,
        Ground = 5,
        None = 0
    }

    public enum Direction
    {
        Up,
        Down,
        None
    }

    private readonly HttpClient _client;
    private readonly string _down = new($"{Url}4/on");
    private readonly string _up = new($"{Url}/5/on");
    public bool Switching;

    public AntennaSwitchClient(string ip, int port)
    {
        Url = $"http://{ip}:{port}";
        _client = new HttpClient();
        SetupHttpClient(Url);
    }

    public AntennaSwitchClient(string ip)
    {
        Url = $"http://{ip}";
        _client = new HttpClient();
        SetupHttpClient(Url);
    }

    // Local antenna switch IP
    private static string? Url { get; set; }

    /// <summary>
    ///     The wanted antenna (to be selected) on the antenna switch
    /// </summary>
    public AntennaType WantedAntenna { get; set; }

    /// <summary>
    ///     The currently selected antenna
    /// </summary>
    public AntennaType SelectedAntenna { get; set; }

    public bool SupportsBand { get; set; }

    private void SetupHttpClient(string url)
    {
        _client.BaseAddress = new Uri(url);
        _client.DefaultRequestVersion = new Version(1, 0);
        _client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        _client.MaxResponseContentBufferSize = 5000;
        _client.Timeout = new TimeSpan(0, 1, 0);
    }

    public void GetSelectedAntennaFromSwitch(Direction direction)
    {
        var s = direction switch
        {
            Direction.Up => _client.GetStringAsync(_up).Result,
            Direction.Down => _client.GetStringAsync(_down).Result,
            Direction.None or _ => _client.GetStringAsync(Url).Result
        };

        if (s.Substring(577, 6).Contains("GROUND"))
        {
            SelectedAntenna = AntennaType.Ground;
            return;
        }

        var selected = s.Substring(574, 1);

        // Set the selected antenna based on the parsed result from the switch
        SelectedAntenna = selected switch
        {
            "4" => AntennaType.EndFed,
            "3" => AntennaType.Vertical,
            "2" => AntennaType.Dipole,
            "1" => AntennaType.Beam,
            _ => AntennaType.None
        };
    }

    /// <summary>
    ///     Sets the selected antenna switch to the wanted input based on the currently selected antenna and
    ///     the wanted antenna
    ///     The wanted antenna is set by the band indexing / band decoder logic and this simply moves the switch
    ///     in the direction necessary to get the correct antenna
    /// </summary>
    public void SetAntennaSwitchToWantedAntenna()
    {
        // Make sure we have the latest antenna info to work with
        if (SelectedAntenna is AntennaType.None && !Switching) GetSelectedAntennaFromSwitch(Direction.None);
        if (SelectedAntenna == WantedAntenna) return;

        if (SelectedAntenna > WantedAntenna)
        {
            var steps = (int)SelectedAntenna - (int)WantedAntenna;
            SetAntennaSwitchToPosition(Direction.Down, steps);
        }

        if (WantedAntenna > SelectedAntenna)
        {
            var steps = (int)WantedAntenna - (int)SelectedAntenna;
            SetAntennaSwitchToPosition(Direction.Up, steps);
        }
    }

    /// <summary>
    ///     Sets the antenna switch to the requested position
    /// </summary>
    /// <param name="direction"> Up or down, or just querying the website</param>
    /// <param name="steps"> the amount of times we need to go in either direction</param>
    private void SetAntennaSwitchToPosition(Direction direction, int steps = 1)
    {
        for (var j = 0; j < steps; j++)
        {
            if (SelectedAntenna == WantedAntenna)
                return;
            GetSelectedAntennaFromSwitch(direction);
            Thread.Sleep(200);
            Switching = true;
        }

        Switching = false;
    }
}