using Terminal.Gui;

namespace AntennaSwitch.View;

public partial class AntennaSwitchWindow
{
    private readonly BandDecoder? _bd;

    public AntennaSwitchWindow()
    {
        InitializeComponent();
    }

    public AntennaSwitchWindow(BandDecoder bd)
    {
        InitializeComponent();
        UseOmniRig = useOmniRigCheckbox.Checked;
        useOmniRigCheckbox.Toggled += delegate(bool b) { UseOmniRig = b; };

        _bd = bd;

        switch (UseOmniRig)
        {
            case false:
                comPortValueLabel.Text = _bd?.SerialPort?.PortName;
                useOmniRigCheckbox.Visible = false;
                break;
            case true:
                comPortLabel.Visible = false;
                comPortValueLabel.Visible = false;
                useOmniRigCheckbox.Visible = true;
                useOmniRigCheckbox.Checked = true;
                break;
        }
    }

    public bool UseOmniRig { get; set; }

    private void OmniRigradioGroupOnSelectedItemChanged(SelectedItemChangedArgs obj)
    {
        if (obj.SelectedItem == 1)
        {
            var bandDecoder = _bd;
            if (bandDecoder != null) bandDecoder.UseOmniRig = false;
        }

        if (obj.SelectedItem == 0)
        {
            var bandDecoder = _bd;
            if (bandDecoder != null) bandDecoder.UseOmniRig = true;
            bandDecoder?.OmniRigClient?.StartOmniRig();
        }
    }

    public void UpdateValues(BandDecoder bd)
    {
        try
        {
            if (_bd == null) return;

            currentAntennaValueLabel.Text = bd.AntennaSwitchClient.SelectedAntenna.ToString();
            wantedAntennaValueLabel.Text = bd.AntennaSwitchClient.WantedAntenna.ToString();
            supportsBandCheckBox.Checked = bd.AntennaSwitchClient.SupportsBand;

            frequencyValueLabel.Text = bd.Frequency;
            bandValueLabel.Text = bd.BandName;
        }
        catch
        {
        }
    }
}