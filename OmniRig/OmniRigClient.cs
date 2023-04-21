using OmniRigWrapper;

namespace AntennaSwitch.OmniRig;

public class OmniRigClient : IDisposable
{
    private OmniRigWrapperClass OmniRigWrapper;
    // private string RxFrequency;
    // private string TxFrequency;
    private bool _disposed;

    public OmniRigClient()
    {
        OmniRigWrapper = new OmniRigWrapperClass();
        OmniRigWrapper.StartOmniRig();
        CurrentRigNumber = OmniRigWrapper.CurrentRigNo;
        Status = OmniRigWrapper.Status;
        Frequency = OmniRigWrapper.Frequency;
        Mode = OmniRigWrapper.Mode;
    }

    public string? Rig { get; private set; }

    public int CurrentRigNumber { get; set; }

    public string? Status { get; set; }

    public string? Frequency { get; set; }

    public string? Mode { get; private set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    public static OmniRigClient CreateInstance()
    {
        return new OmniRigClient();
    }

    public void StartOmniRig()
    {
        try
        {
            OmniRigWrapper.StartOmniRig();
        }
        catch (Exception e)
        {
            Console.WriteLine($"{e.Message} {e.StackTrace}");
        }
    }

    private void SelectRig(int rigNumber)
    {
       OmniRigWrapper.SelectRig(rigNumber);
    }

    private void ShowRigStatus()
    {
        OmniRigWrapper.ShowRigStatus();
        Status = OmniRigWrapper.Status;
    }

    // Only a small subset commands are supported
    public void ShowRigParams()
    {
        OmniRigWrapper.ShowRigParams();
        if (Rig == null)
        {
            Status = "ShowRigParams: Rig is null";
            return;
        }
        

        // RxFrequency = Rig.GetRxFrequency().ToString();
        // TxFrequency = Rig.GetTxFrequency().ToString();
        Frequency = OmniRigWrapper.Frequency;
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
                OmniRigWrapper.Dispose();
        }

        _disposed = true;
    }

    ~OmniRigClient()
    {
        Dispose(false);
    }
}