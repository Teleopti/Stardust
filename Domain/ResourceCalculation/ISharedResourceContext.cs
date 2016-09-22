using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	//TODO: remove unnecessary params
	public interface ISharedResourceContext
	{
		IDisposable MakeSureExists(DateOnlyPeriod period, bool forceNewContext); 
	}
}