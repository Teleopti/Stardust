using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Preference
{
	public class PreferenceChangedHandler :
		IHandleEvent<PreferenceChangedEvent>,
		IRunOnServiceBus
	{
		private readonly static ILog logger = LogManager.GetLogger(typeof(PreferenceChangedHandler));

		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICurrentScenario _scenarioRepository;

		public PreferenceChangedHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory,
			ICurrentScenario scenarioRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scenarioRepository = scenarioRepository;

			if (logger.IsInfoEnabled)
			{
				logger.Info("New instance of handler was created");
			}
		}

		public void Handle(PreferenceChangedEvent @event)
		{
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Consuming event for preference Id = {0}. (Message timestamp = {1})",
								   @event.PreferenceId, @event.Timestamp);
			}

			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{

			}
		}
	}
}
