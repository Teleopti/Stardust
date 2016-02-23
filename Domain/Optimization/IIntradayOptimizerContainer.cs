using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IIntradayOptimizerContainer
	{
		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
		void Execute(IEnumerable<IIntradayOptimizer2> optimizers);
	}
}