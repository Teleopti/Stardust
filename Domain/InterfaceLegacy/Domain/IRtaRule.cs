using System.Drawing;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{

	public enum Adherence
	{
		Neutral,
		In,
		Out,
	}

    public interface IRtaRule : IPayload, IAggregateRootWithEvents
    {
	    Description Description { get; set; }
	    Color DisplayColor { get; set; }
		int ThresholdTime { get; set; }
        double StaffingEffect { get; set; }

		Adherence? Adherence { get; set; }
		void SetAdherenceByText(string text);
		string AdherenceText { get; }
	    bool IsAlarm { get; set; }
	    Color AlarmColor { get; set; }
    }
	
}