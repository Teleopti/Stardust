using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class UpdateStaffingLevelReadModel2WeeksHandler : IHandle<UpdateStaffingLevelReadModel2WeeksEvent>
	{
		private readonly IComponentContext _componentContext;
		private readonly IStardustJobFeedback _stardustJobFeedback;

		public UpdateStaffingLevelReadModel2WeeksHandler(IComponentContext componentContext, IStardustJobFeedback stardustJobFeedback)
		{
			_componentContext = componentContext;
			_stardustJobFeedback = stardustJobFeedback;
		}

		public void Handle(UpdateStaffingLevelReadModel2WeeksEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
			_stardustJobFeedback.SendProgress = sendProgress;
			var theRealOne = _componentContext.Resolve<IHandleEvent<UpdateStaffingLevelReadModel2WeeksEvent>>();
			theRealOne.Handle(parameters);
			_stardustJobFeedback.SendProgress = null;
		}
	}
}