using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Logon.Aspects;

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
			source.AsParallel().ForAll(s => Execute(s, action));
		}

		[TenantScope]
		protected virtual void Execute(ExternalUserStateInputModel input, Action<ExternalUserStateInputModel> action)
		{
			action.Invoke(input);
		}

	}
}