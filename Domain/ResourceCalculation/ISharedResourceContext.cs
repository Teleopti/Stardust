using System;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	[RemoveMeWithToggle(@"
	* no need to keep period as param when toggle is gone - then it should always be full period
	* no need to return IDisposable
	",
	Toggles.ResourcePlanner_SpeedUpManualChanges_37029)]
	public interface ISharedResourceContext
	{
		IDisposable MakeSureExists(DateOnlyPeriod period, bool forceNewContext); 
	}
}