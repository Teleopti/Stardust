using System;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public interface IActionThrottler
	{
		BlockToken Block(string action);
		void Resume(BlockToken blockToken);
		void Pause(BlockToken blockToken, TimeSpan allottedPause);
		void Finish(BlockToken blockToken);
		bool IsBlocked(string action);
	}
}