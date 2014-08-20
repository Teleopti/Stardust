using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

public class TimeFilterInfo
{
	public IEnumerable<DateTimePeriod> StartTimes { get; set; }
	public IEnumerable<DateTimePeriod> EndTimes { get; set; } 
	public bool IsDayOff { get; set; }
}