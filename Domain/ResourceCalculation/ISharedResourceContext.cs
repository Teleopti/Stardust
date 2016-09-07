using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface ISharedResourceContext
	{
		IDisposable Use(DateOnlyPeriod period); //no need to keep period as param when toggle is gone - then it should always be "full" period
	}
}