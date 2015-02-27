using System;
using System.Drawing;

namespace Teleopti.Interfaces.Domain
{

	public enum Adherence
	{
		Neutral,
		In,
		Out,
	}

    public interface IAlarmType : IPayload
    {
	    Description Description { get; set; }
	    Color DisplayColor { get; set; }
	    TimeSpan ThresholdTime { get; set; }
        double StaffingEffect { get; set; }

		Adherence Adherence { get; set; }
		void SetAdherenceByText(string text);
		string AdherenceText { get; }
    }
	
}