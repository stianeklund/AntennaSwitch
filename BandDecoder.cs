using System.IO.Ports;
using System.Text;
using System.Timers;
using AntennaSwitch.OmniRig;
using Terminal.Gui;
using Timer = System.Timers.Timer;

namespace AntennaSwitch;

/// <summary>
///     A rudimentary band decoder that uses CAT data (FA & IF CAT commands) to switch a LZ2RR web antenna switch
///     Written for Kenwood but should work with most radio's, in my use case it's used with a split virtual serial port
/// </summary>
public sealed class BandDecoder : IDisposable
{
    private readonly AntennaSwitchClient _antennaSwitch;
    public readonly OmniRigClient? OmniRigClient = OmniRigClient.CreateInstance();
    private bool _disposed;
    public bool UseOmniRig { get; set; }

    public BandDecoder(AntennaSwitchClient antennaSwitch)
    {
        _antennaSwitch = antennaSwitch;
        _antennaSwitch.SelectedAntenna = AntennaSwitchClient.AntennaType.None;
        _antennaSwitch.WantedAntenna = AntennaSwitchClient.AntennaType.None;

        var bandDecoder = new Timer();
        bandDecoder.Enabled = true;
        bandDecoder.Interval = 1000;
        bandDecoder.AutoReset = true;
        bandDecoder.Elapsed += BandDecoderTimer;

        switch (UseOmniRig)
        {
            case true:
                OmniRigClient.StartOmniRig();
                if (OmniRigClient?.OmniRigEngine != null)
                {
                    OmniRigClient.OmniRigEngine.StatusChange += OmniRigEngineOnStatusChange;
                }
                break;
            case false:
                SerialPort = new();
                OpenSerialPort();
                break;
        }
    }

    public SerialPort? SerialPort { get; }

    public string? BandName { get; private set; }
    public string? Mode { get; private set; }
    public string? Frequency { get; private set; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void OmniRigEngineOnStatusChange(int rigNumber)
    {
        Console.WriteLine($"Status change:{rigNumber}");
    }

    private void BandDecoderTimer(object? sender, ElapsedEventArgs e)
    {
        if (Frequency == null && UseOmniRig) Frequency = OmniRigClient?.Rig?.GetRxFrequency().ToString();
        if (Frequency != null && !_antennaSwitch.Switching) DecodeBand(Frequency);
        OmniRigClient?.ShowRigParams();
        Mode = OmniRigClient?.Mode;
    }

    private void OpenSerialPort()
    {
        if (SerialPort is null) return;
        if (SerialPort.IsOpen) return;

        SerialPort.PortName = "COM10";
        SerialPort.BaudRate = 115200;
        SerialPort.Parity = Parity.None;
        SerialPort.DataBits = 8;
        SerialPort.StopBits = StopBits.One;
        SerialPort.RtsEnable = true;
        SerialPort.Handshake = Handshake.None;
        SerialPort.Encoding = Encoding.UTF8;
        SerialPort.NewLine = "\n";
        SerialPort.DataReceived += SerialPortOnDataReceived;
        try
        {
            SerialPort.Close();
            SerialPort.Open();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error when opening the serial port: {SerialPort},{e}");
            SerialPort.Close();
            Application.RequestStop();
        }
    }

    private void SerialPortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (sender is not SerialPort sp) return;
        List<byte> parsed = new();
        if (!sp.IsOpen) return;
        lock (sp)
        {
            var bytes = sp.BytesToRead;
            var buffer = new byte[bytes];
            sp.Read(buffer, 0, bytes);
            foreach (var b in buffer)
            {
                parsed.Add(b);
                if (b == 0x3B) // ;
                    break;
            }

            var data = Encoding.UTF8.GetString(parsed.ToArray());
            ParseSerialData(data);
        }
    }


    private void DecodeBand(string frequency)
    {
        if (string.IsNullOrEmpty(frequency)) return;
        var freq = Convert.ToInt64(frequency);
        switch (freq)
        {
            case >= 1810000 and <= 2000000:
                BandName = "160m";
                _antennaSwitch.SupportsBand = false;
                break;
            case >= 3500000 and <= 3800000:
                BandName = "80m";
                _antennaSwitch.SupportsBand = true;
                _antennaSwitch.WantedAntenna = AntennaSwitchClient.AntennaType.EndFed;
                break;
            // Not sure if 590 can use this band w/o memory setup but nevertheless it's supported, although no antenna for it currently
            case >= 5102000 and <= 5406500:
                BandName = "60m";
                _antennaSwitch.SupportsBand = false;
                _antennaSwitch.WantedAntenna = AntennaSwitchClient.AntennaType.EndFed;
                break;
            case >= 7000000 and <= 7200000:
                BandName = "40m";
                _antennaSwitch.SupportsBand = false;
                break;
            case >= 10100000 and <= 10150000:
                BandName = "30m";
                _antennaSwitch.SupportsBand = false;
                break;
            case >= 14000000 and <= 14350000:
                BandName = "20m";
                _antennaSwitch.SupportsBand = true;
                _antennaSwitch.WantedAntenna = AntennaSwitchClient.AntennaType.EndFed;
                break;
            case >= 18068000 and <= 18168000:
                BandName = "17m";
                _antennaSwitch.SupportsBand = true;
                _antennaSwitch.WantedAntenna = AntennaSwitchClient.AntennaType.Vertical;
                break;
            case >= 21000000 and <= 21450000:
                BandName = "15m";
                _antennaSwitch.SupportsBand = true;
                _antennaSwitch.WantedAntenna = AntennaSwitchClient.AntennaType.EndFed;
                break;
            case >= 24890000 and <= 24990000:
                BandName = "12m";
                _antennaSwitch.SupportsBand = true;
                _antennaSwitch.WantedAntenna = AntennaSwitchClient.AntennaType.EndFed;
                break;
            case >= 28000000 and <= 29700000:
                BandName = "10m";
                _antennaSwitch.SupportsBand = true;
                _antennaSwitch.WantedAntenna = AntennaSwitchClient.AntennaType.Dipole;
                break;
            // Just so we can use the 6m beam for rx, it's better than anything else..
            case >= 40660000 and <= 40700000:
                BandName = "8m";
                _antennaSwitch.SupportsBand = true;
                _antennaSwitch.WantedAntenna = AntennaSwitchClient.AntennaType.Beam;
                break;
            case >= 50000000 and <= 52000000:
                BandName = "6m";
                _antennaSwitch.SupportsBand = true;
                _antennaSwitch.WantedAntenna = AntennaSwitchClient.AntennaType.Beam;
                break;
            default:
                //>= 144000000 and <= 146000000 => 11 //   2m
                BandName = "GEN";
                Console.WriteLine($"General or out of band? {freq}");
                break;
        }

        // Switch only if the band is supported by the switch and we're not actively switching inputs
        if (_antennaSwitch is { SupportsBand: true, Switching: false })
            _antennaSwitch.SetAntennaSwitchToWantedAntenna();
    }


    private void ParseSerialData(string msg)
    {
        try
        {
            switch (msg[..2])
            {
                case "FA":
                case "IF":
                    var freq = msg.Substring(2, 11);
                    // if we've already set this and it hasn't changed no need to update again
                    if (Frequency == freq) return;
                    Frequency = freq;
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error when parsing serial data: {e.Message} {e.StackTrace} {e.Data}");
        }
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            if (SerialPort != null)
            {
                SerialPort.DataReceived -= SerialPortOnDataReceived;
                SerialPort.Dispose();
            }
        }

        _disposed = true;
    }

    ~BandDecoder()
    {
        Dispose(false);
    }
}