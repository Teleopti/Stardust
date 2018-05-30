using System;

namespace Teleopti.Wfm.Administration.Core
{
	interface IRecurrentEventTimer
	{
		void Init(TimeSpan tickInterval);
	}
}
