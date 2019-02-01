namespace Teleopti.Wfm.Adherence.Configuration
{
    public interface IRtaState
    {
        string Name { get; set; }
        string StateCode { get; set; }
        IRtaStateGroup Parent { get; set;  }
	}
}