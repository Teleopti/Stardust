using System;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IStardustJobFeedback
	{
		Action<string> SendProgress { get; set; }
	}
}