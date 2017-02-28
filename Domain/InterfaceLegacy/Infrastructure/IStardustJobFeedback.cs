using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface IStardustJobFeedback
	{
		Action<string> SendProgress { get; set; }
	}
}