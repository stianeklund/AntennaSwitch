namespace AntennaSwitch;

public partial class AntennaSwitchWindow
{
    private readonly BandDecoder? _bd;
    private readonly AntennaSwitchClient? _sw;

    public AntennaSwitchWindow()
    {
        InitializeComponent();
    }

    public AntennaSwitchWindow(AntennaSwitchClient? sw, BandDecoder? bd)
    {
        InitializeComponent();
        _sw = sw;
        _bd = bd;
        comPortValueLabel.Text = _bd?.SerialPort.PortName;
    }

    public void UpdateValues()
    {
        try
        {
            if (_sw != null)
            {
                currentAntennaValueLabel.Text = _sw.SelectedAntenna.ToString();
                wantedAntennaValueLabel.Text = _sw.WantedAntenna.ToString();
                supportsBandCheckBox.Checked = _sw.SupportsBand;
            }

            frequencyValueLabel.Text = _bd?.Frequency;
            bandValueLabel.Text = _bd?.BandName;
        }
        catch
        {
        }
    }
}