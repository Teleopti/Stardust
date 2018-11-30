using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Domain.Configuration
{
	public interface IRtaRule : IPayload, IAggregateRootWithEvents
    {
	    Description Description { get; set; }
	    Color DisplayColor { get; set; }
		int ThresholdTime { get; set; }
        double StaffingEffect { get; set; }

		Adherence? Adherence { get; set; }
		void SetAdherenceByText(string text);
		string AdherenceTextResource { get; }
	    bool IsAlarm { get; set; }
	    Color AlarmColor { get; set; }
    }
	
}