using Terminal.Gui;

namespace AntennaSwitch.View
{
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
            if (bd == null) return;
            bd.UseOmniRig = useOmniRigCheckbox.Checked;
            useOmniRigCheckbox.Toggled += delegate(bool b)
            {
                bd.UseOmniRig = b;
            };
            

            switch (bd)
            {
                case { UseOmniRig: false }:
                    comPortValueLabel.Text = _bd?.SerialPort?.PortName;
                    useOmniRigCheckbox.Visible = false;
                    break;
                case { UseOmniRig: true }:
                    comPortLabel.Visible = false;
                    comPortValueLabel.Visible = false;
                    useOmniRigCheckbox.Visible = true;
                    useOmniRigCheckbox.Checked = true;
                    break;
            }
        }

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
}