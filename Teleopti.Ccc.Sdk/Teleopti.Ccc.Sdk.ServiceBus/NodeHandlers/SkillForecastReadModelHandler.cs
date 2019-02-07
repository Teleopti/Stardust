using System;
using System.Collections.Generic;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class SkillForecastReadModelHandler : IHandle<ForecastChangedEvent>,IHandle<UpdateSkillForecastReadModelEvent>
	{
		private readonly IComponentContext _componentContext;

		public SkillForecastReadModelHandler(IComponentContext componentContext)
		{
			_componentContext = componentContext;
		}

		[AsSystem]
		public virtual void Handle(ForecastChangedEvent parameters,
			CancellationTokenSource cancellationTokenSource,
			Action<string> sendProgress,
			ref IEnumerable<object> returnObjects)
		{
			var theRealOne = _componentContext.Resolve<IHandleEvent<ForecastChangedEvent>>();
			theRealOne.Handle(parameters);
		}

		[AsSystem]
		public virtual void Handle(UpdateSkillForecastReadModelEvent parameters, CancellationTokenSource cancellationTokenSource,
			Action<string> sendProgress, ref IEnumerable<object> returnObjects)
		{
			var theRealOne = _componentContext.Resolve<IHandleEvent<UpdateSkillForecastReadModelEvent>>();
			theRealOne.Handle(parameters);
		}
	}
}