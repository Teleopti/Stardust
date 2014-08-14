using System;
using System.Collections.Generic;

public class TimeFilterInfo
{
	public IEnumerable<DateTime> StartTimeStarts { get; set; }
	public IEnumerable<DateTime> StartTimeEnds { get; set; }
	public IEnumerable<DateTime> EndTimeStarts { get; set; }
	public IEnumerable<DateTime> EndTimeEnds { get; set; }
	public bool IsDayOff { get; set; }
}