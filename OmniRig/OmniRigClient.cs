using System.Runtime.InteropServices;
using OmniRig;

namespace AntennaSwitch.OmniRig;

public class OmniRigClient: IDisposable
{
    private string RxFrequency;
    private string TxFrequency;
    private bool _disposed;

    public OmniRigX? OmniRigEngine { get; private set; }
    public RigX? Rig { get; private set; }

    private int CurrentRigNumber { get; set; }

    private string? Status { get; set; }

    private string? Frequency { get; set; }

    public string? Mode { get; private set; }

    public static OmniRigClient CreateInstance()
    {
        return new OmniRigClient();
    }

    public void StartOmniRig()
    {
        try
        {
            if (OmniRigEngine != null)
            {
                Console.WriteLine("OmniRig is already running");
            }
            else
            {
                OmniRigEngine = Activator.CreateInstance(typeof(MarshalByRefObject) ??
                                                         throw new InvalidOperationException()) as OmniRigX;

                //  Supported versions 1.01 to 2.99 anything outside of that is probably not compatible
                if (OmniRigEngine?.InterfaceVersion < 0x101 &&
                    OmniRigEngine?.InterfaceVersion > 0x299)
                {
                    OmniRigEngine = null;
                }

                SelectRig(1);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"{e.Message} {e.StackTrace}");
        }
    }

    private void SelectRig(int rigNumber)
    {
        if (OmniRigEngine == null)
            return;

        CurrentRigNumber = rigNumber;

        Rig = rigNumber switch
        {
            1 => OmniRigEngine.Rig1,
            2 => OmniRigEngine.Rig2,
            _ => Rig
        };

        ShowRigStatus();
        ShowRigParams();
    }

    private void ShowRigStatus()
    {
        if (Rig != null)
            Status = Rig.StatusStr;
    }

    // Only a small subset commands are supported
    public void ShowRigParams()
    {
        if (Rig == null) return;

        RxFrequency = Rig.GetRxFrequency().ToString();
        TxFrequency = Rig.GetTxFrequency().ToString();
        Frequency = Rig.Freq.ToString();

        Mode = Rig.Mode switch
        {
            RigParamX.PM_CW_L => "CW",
            RigParamX.PM_CW_U => "CW-R",
            RigParamX.PM_SSB_L => "LSB",
            RigParamX.PM_SSB_U => "USB",
            RigParamX.PM_FM => "FM",
            RigParamX.PM_AM => "AM",
            _ => "Other"
        };
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            if (OmniRigEngine != null)
            {
                // TODO Not sure if this is the correct way to do this..
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    int ret = Marshal.ReleaseComObject(OmniRigEngine);
                }

                OmniRigEngine = null;
            }

            if (Rig != null)
            {
                Rig = null;
            }
        }

        _disposed = true;
    }
    
        ~OmniRigClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }
}