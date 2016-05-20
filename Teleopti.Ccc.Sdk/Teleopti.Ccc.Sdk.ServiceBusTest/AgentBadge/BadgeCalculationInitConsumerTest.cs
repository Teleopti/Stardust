using System;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Sdk.ServiceBus.AgentBadge;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.AgentBadge
{
	[TestFixture]
	public class BadgeCalculationInitConsumerTest
	{

		private IServiceBus serviceBus;
		private BadgeCalculationInitConsumer target;
		private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;
		private IToggleManager toggleManager;
		private IPerformBadgeCalculation _performBadgeCalculation;
		private MockRepository _mock;
		private IRunningEtlJobChecker _runningEtlJobChecker;
		private INow _now;
		private IIsTeamGamificationSettingsAvailable _isTeamGamificationSettingsAvailable;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IUnitOfWork _uow;

		[SetUp]
		public void Setup()
		{

			_mock = new MockRepository();
			_performBadgeCalculation = _mock.StrictMock<IPerformBadgeCalculation>();
			toggleManager = _mock.StrictMock<IToggleManager>();
			serviceBus = _mock.StrictMock<IServiceBus>();
			currentUnitOfWorkFactory = _mock.StrictMock<ICurrentUnitOfWorkFactory>();
			_runningEtlJobChecker = _mock.StrictMock<IRunningEtlJobChecker>();
			_unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
			_uow = _mock.StrictMock<IUnitOfWork>();
			_isTeamGamificationSettingsAvailable = _mock.StrictMock<IIsTeamGamificationSettingsAvailable>();
			_now = new MutableNow();

			target = new BadgeCalculationInitConsumer(_performBadgeCalculation, _runningEtlJobChecker, serviceBus, _now,
				toggleManager, _isTeamGamificationSettingsAvailable, currentUnitOfWorkFactory);
		}

		[Test]
		public void ShouldResendMessageIfNoSettingIsAvailable()
		{

			using (_mock.Record())
			{
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
				Expect.Call(toggleManager.IsEnabled(Toggles.Portal_DifferentiateBadgeSettingForAgents_31318)).Return(true);
				Expect.Call(_isTeamGamificationSettingsAvailable.Satisfy()).Return(false);
				Expect.Call(() => serviceBus.DelaySend(DateTime.Now, null)).IgnoreArguments();
				Expect.Call(() => _uow.Dispose());

			}

			using (_mock.Playback())
			{
				target.Consume(new BadgeCalculationInitMessage());
			}
		}

		[Test]
		public void ShouldResendMessageIfNightlyIsRunning()
		{

			using (_mock.Record())
			{
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
				Expect.Call(toggleManager.IsEnabled(Toggles.Portal_DifferentiateBadgeSettingForAgents_31318)).Return(true);
				Expect.Call(_isTeamGamificationSettingsAvailable.Satisfy()).Return(true);
				Expect.Call(_runningEtlJobChecker.NightlyEtlJobStillRunning()).Return(true);
				Expect.Call(() => serviceBus.DelaySend(DateTime.Now, null)).IgnoreArguments();
				Expect.Call(() => _uow.Dispose());

			}

			using (_mock.Playback())
			{
				target.Consume(new BadgeCalculationInitMessage());
			}
		}

		[Test]
		public void ShouldPerformBadgeCalculations()
		{

			using (_mock.Record())
			{
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
				Expect.Call(toggleManager.IsEnabled(Toggles.Portal_DifferentiateBadgeSettingForAgents_31318)).Return(true);
				Expect.Call(_isTeamGamificationSettingsAvailable.Satisfy()).Return(true);
				Expect.Call(_runningEtlJobChecker.NightlyEtlJobStillRunning()).Return(false);
				Expect.Call(() => _performBadgeCalculation.Calculate(Guid.NewGuid(), false)).IgnoreArguments();
				Expect.Call(() => serviceBus.DelaySend(DateTime.Now, null)).IgnoreArguments();
				Expect.Call(() => _uow.PersistAll());
				Expect.Call(() => _uow.Dispose());

			}

			using (_mock.Playback())
			{
				target.Consume(new BadgeCalculationInitMessage());
			}
		}
	}
}
