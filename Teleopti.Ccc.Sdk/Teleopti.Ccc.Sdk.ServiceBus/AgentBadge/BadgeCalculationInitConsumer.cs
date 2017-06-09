using Rhino.ServiceBus;
using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class BadgeCalculationInitConsumer : ConsumerOf<BadgeCalculationInitMessage>
	{
		private readonly IIsTeamGamificationSettingsAvailable _isTeamGamificationSettingsAvailable;
		private readonly INow _now;
		private readonly IPerformBadgeCalculation _performBadgeCalculation;
		private readonly IRunningEtlJobChecker _runningEtlJobChecker;
		private readonly IServiceBus _serviceBus;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public BadgeCalculationInitConsumer(IPerformBadgeCalculation performBadgeCalculation,
			IRunningEtlJobChecker runningEtlJobChecker, IServiceBus serviceBus, INow now,
			IIsTeamGamificationSettingsAvailable isTeamGamificationSettingsAvailable, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_performBadgeCalculation = performBadgeCalculation;
			_runningEtlJobChecker = runningEtlJobChecker;
			_serviceBus = serviceBus;
			_now = now;
			_isTeamGamificationSettingsAvailable = isTeamGamificationSettingsAvailable;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public void Consume(BadgeCalculationInitMessage message)
		{
			using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				if (!_isTeamGamificationSettingsAvailable.Satisfy())
				{
					resendMessage(message);
					return;
				}

				if (checkIfNightlyIsRunning(message)) return;

				_performBadgeCalculation.Calculate(message.LogOnBusinessUnitId);

				resendMessage(message);
				uow.PersistAll();
			}
		}

		private bool checkIfNightlyIsRunning(BadgeCalculationInitMessage message)
		{
			if (_runningEtlJobChecker.NightlyEtlJobStillRunning())
			{
				var utcNow = DateTime.UtcNow;
				// If the ETL nightly job still running, then delay badge calculation 5 minutes later.
				delaySend(message, utcNow.AddMinutes(5));
				return true;
			}
			return false;
		}

		private void delaySend(BadgeCalculationInitMessage message, DateTime delayTime)
		{
			_serviceBus.DelaySend(delayTime, message);
		}

		private void resendMessage(BadgeCalculationInitMessage message)
		{
			var today = _now.ServerDate_DontUse();
			var tomorrow = new DateTime(today.AddDays(1).Date.Ticks, DateTimeKind.Local);
			delaySend(message, tomorrow.AddHours(5));
		}
	}
}