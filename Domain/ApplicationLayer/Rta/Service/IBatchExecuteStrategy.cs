using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IBatchExecuteStrategy
	{
		void Execute(IEnumerable<ExternalUserStateInputModel> source, Action<ExternalUserStateInputModel> action);
	}
	
	public class InSequence : IBatchExecuteStrategy
	{
		public void Execute(IEnumerable<ExternalUserStateInputModel> source, Action<ExternalUserStateInputModel> action)
		{
			source.ForEach(action);
		}
	}

	public class InParallel : IBatchExecuteStrategy
	{
		public void Execute(IEnumerable<ExternalUserStateInputModel> source, Action<ExternalUserStateInputModel> action)
		{
			var closingSnapshot = source.Where(x => x.IsSnapshot && string.IsNullOrEmpty(x.UserCode));
			if (closingSnapshot.Any())
			{
				source.Except(closingSnapshot).AsParallel().ForAll(action);
				action(closingSnapshot.First());
			}
			else
				source.AsParallel().ForAll(action);
		}
	}
}