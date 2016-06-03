using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IResourceCalculationContextFactory
	{
		IDisposable Create();
		IDisposable Create(DateOnlyPeriod period);
	}
}