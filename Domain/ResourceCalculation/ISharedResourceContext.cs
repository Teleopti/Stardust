using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface ISharedResourceContext
	{
		//no need to keep period as param when toggle is gone - then it should always be "full" period
		//also - no need to return IDisposable
		IDisposable MakeSureExists(DateOnlyPeriod period); 
	}
}