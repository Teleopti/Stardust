﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Rhino.ServiceBus.DataStructures;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Sdk.ServiceBus.AgentBadge;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.AgentBadge
{
	[TestFixture]
	public class CalculateOrganizationalSettingBasedBadgeConsumerTest
	{
		private ICurrentUnitOfWorkFactory unitOfWorkFactory;
		private IServiceBus serviceBus;
		private ITeamGamificationSettingRepository teamSettingsRepository;
		private IAgentBadgeRepository badgeRepository;
		private IAgentBadgeWithRankRepository badgeWithRankRepository;
		private IPersonRepository personRepository;
		private IPushMessagePersister msgRepository;
		private CalculateOrganizationalSettingBasedBadgeConsumer target;
		private IAgentBadgeCalculator calculator;
		private INow now;
		private IUnitOfWorkFactory loggedOnUnitOfWorkFactory;
		private IUnitOfWork unitOfWork;
		private IToggleManager toggleManager;
		private IAgentBadgeWithRankCalculator badgeWithRankCalculator;
		private IGlobalSettingDataRepository globalSettingRep;

		[SetUp]
		public void Setup()
		{
			unitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			loggedOnUnitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			unitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(loggedOnUnitOfWorkFactory);
			loggedOnUnitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			serviceBus = MockRepository.GenerateMock<IServiceBus>();
			teamSettingsRepository = MockRepository.GenerateMock<ITeamGamificationSettingRepository>();

			badgeRepository = MockRepository.GenerateMock<IAgentBadgeRepository>();
			badgeWithRankRepository = MockRepository.GenerateMock<IAgentBadgeWithRankRepository>();
			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			toggleManager = MockRepository.GenerateMock<IToggleManager>();
			toggleManager.Stub(x => x.IsEnabled(Toggles.Gamification_NewBadgeCalculation_31185)).Return(false);

			personRepository.Stub(
				x => x.FindPeopleInOrganization(new DateOnlyPeriod(new DateOnly(2014, 8, 7), new DateOnly(2014, 8, 9)), false))
				.Return(new List<IPerson> {new Person()});

			msgRepository = MockRepository.GenerateMock<IPushMessagePersister>();

			globalSettingRep = MockRepository.GenerateMock<IGlobalSettingDataRepository>();
			globalSettingRep.Stub(x => x.FindValueByKey(AdherenceReportSetting.Key, new AdherenceReportSetting()))
				.IgnoreArguments()
				.Return(new AdherenceReportSetting());
			now = MockRepository.GenerateMock<INow>();

			// Mock badge calculator
			calculator = MockRepository.GenerateMock<IAgentBadgeCalculator>();
			calculator.Stub(x => x.CalculateAHTBadges(new List<IPerson>(), "", DateOnly.Today, new GamificationSetting("test")))
				.Return(new List<IAgentBadgeTransaction>()).IgnoreArguments();
			calculator.Stub(x => x.CalculateAdherenceBadges(new List<IPerson>(), "", DateOnly.Today,
				AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime, new GamificationSetting("test")))
				.Return(new List<IAgentBadgeTransaction>()).IgnoreArguments();
			calculator.Stub(x => x.CalculateAnsweredCallsBadges(new List<IPerson>(), "", DateOnly.Today, new GamificationSetting("test")))
				.Return(new List<IAgentBadgeTransaction>()).IgnoreArguments();

			// Mock badge with rank calculator
			badgeWithRankCalculator = MockRepository.GenerateMock<IAgentBadgeWithRankCalculator>();

			target = new CalculateOrganizationalSettingBasedBadgeConsumer(serviceBus, teamSettingsRepository, 
				msgRepository, unitOfWorkFactory, calculator, badgeWithRankCalculator, badgeRepository, badgeWithRankRepository, now, toggleManager, globalSettingRep,personRepository);

		}

		[Test]
		public void ShouldSendCalculateBadgeMessageAtRightTime()
		{
			var timezone = TimeZoneInfo.Utc;
			var today = new DateTime(2014, 8, 8);
			var tomorrowUnspecified = new DateTime(2014, 8, 9, 0, 0, 0, DateTimeKind.Unspecified);
			var expectedNextMessageShouldBeProcessed =
				TimeZoneInfo.ConvertTime(tomorrowUnspecified.AddHours(5), timezone, TimeZoneInfo.Local);

			now.Stub(x => x.UtcDateTime()).Return(today);

			var newSetting = new GamificationSetting("test")
			{
				AdherenceBadgeEnabled = true,
				AHTBadgeEnabled = false,
				AnsweredCallsBadgeEnabled = true
			};

			var team = TeamFactory.CreateSimpleTeam("team");
			team.SetId(Guid.NewGuid());

			var persons = new List<IPerson> {new Person()};
			personRepository.Stub(
				x => x.FindPeopleBelongTeam(team, new DateOnlyPeriod(new DateOnly(today).AddDays(-1), new DateOnly(today.AddDays(1)))))
				.Return(persons);

			teamSettingsRepository.Stub(x => x.FindAllTeamGamificationSettingsSortedByTeam())
				.Return(new[] {new TeamGamificationSetting {Team = team, GamificationSetting = newSetting}});

			var calculationDate = TimeZoneInfo.ConvertTime(now.LocalDateOnly().AddDays(-1), TimeZoneInfo.Local, timezone);
			var message = new CalculateBadgeMessage
			{
				TimeZoneCode = timezone.Id,
				CalculationDate = new DateOnly(calculationDate)
			};
		
			target.Consume(message);

			calculator.AssertWasNotCalled(
				x => x.CalculateAHTBadges(persons, "", DateOnly.Today, newSetting),
				o => o.IgnoreArguments());

			calculator.AssertWasCalled(
				x => x.CalculateAdherenceBadges(persons, "", DateOnly.Today,
					AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime, newSetting),
				o => o.IgnoreArguments());

			calculator.AssertWasCalled(
				x => x.CalculateAnsweredCallsBadges(persons, "", DateOnly.Today, newSetting),
				o => o.IgnoreArguments());

			serviceBus.AssertWasCalled(x => x.DelaySend(new DateTime(), new object()),
				o =>
					o.Constraints(
						Rhino.Mocks.Constraints.Is.Matching(new Predicate<DateTime>(m => m == expectedNextMessageShouldBeProcessed)),
						Rhino.Mocks.Constraints.Is.Matching(new Predicate<object[]>(m =>
						{
							var msg = ((CalculateBadgeMessage)m[0]);
							return msg.TimeZoneCode == TimeZoneInfo.Utc.Id
								&& msg.CalculationDate == message.CalculationDate.AddDays(1);
						}))));
		}
		
		[Test]
		public void ShouldNotCalculateBadgeWhenAppliedSettingIsDeleted()
		{
			var timezone = TimeZoneInfo.Utc;
			var today = new DateTime(2014, 8, 8);
			var tomorrowUnspecified = new DateTime(2014, 8, 9, 0, 0, 0, DateTimeKind.Unspecified);
			var expectedNextMessageShouldBeProcessed =
				TimeZoneInfo.ConvertTime(tomorrowUnspecified.AddHours(5), timezone, TimeZoneInfo.Local);

			now.Stub(x => x.UtcDateTime()).Return(today);

			var deletedSetting = new GamificationSetting("test")
			{
				AdherenceBadgeEnabled = true,
				AHTBadgeEnabled = false,
				AnsweredCallsBadgeEnabled = true
			};
			deletedSetting.SetDeleted();

			var team = TeamFactory.CreateSimpleTeam("team");
			team.SetId(Guid.NewGuid());

			var persons = new List<IPerson> {new Person()};
			personRepository.Stub(
				x => x.FindPeopleBelongTeam(team, new DateOnlyPeriod(new DateOnly(today).AddDays(-1), new DateOnly(today.AddDays(1)))))
				.Return(persons);

			teamSettingsRepository.Stub(x => x.FindAllTeamGamificationSettingsSortedByTeam())
				.Return(new[] {new TeamGamificationSetting {Team = team, GamificationSetting = deletedSetting}});

			var calculationDate = TimeZoneInfo.ConvertTime(now.LocalDateOnly().AddDays(-1), TimeZoneInfo.Local, timezone);
			var message = new CalculateBadgeMessage
			{
				TimeZoneCode = timezone.Id,
				CalculationDate = new DateOnly(calculationDate)
			};
		
			target.Consume(message);

			calculator.AssertWasNotCalled(
				x => x.CalculateAHTBadges(persons, "", DateOnly.Today, deletedSetting),
				o => o.IgnoreArguments());

			calculator.AssertWasNotCalled(
				x => x.CalculateAdherenceBadges(persons, "", DateOnly.Today,
					AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime, deletedSetting),
				o => o.IgnoreArguments());

			calculator.AssertWasNotCalled(
				x => x.CalculateAnsweredCallsBadges(persons, "", DateOnly.Today, deletedSetting),
				o => o.IgnoreArguments());

			serviceBus.AssertWasCalled(x => x.DelaySend(new DateTime(), new object()),
				o =>
					o.Constraints(
						Rhino.Mocks.Constraints.Is.Matching(new Predicate<DateTime>(m => m == expectedNextMessageShouldBeProcessed)),
						Rhino.Mocks.Constraints.Is.Matching(new Predicate<object[]>(m =>
						{
							var msg = ((CalculateBadgeMessage)m[0]);
							return msg.TimeZoneCode == TimeZoneInfo.Utc.Id
								&& msg.CalculationDate == message.CalculationDate.AddDays(1);
						}))));
		}

		[Test]
		public void ShouldNotCalculateBadgeWhenNoSettingAppliedToAnyTeam()
		{
			var timezone = TimeZoneInfo.Utc;
			var today = new DateTime(2014, 8, 8);
			var tomorrowUnspecified = new DateTime(2014, 8, 9, 0, 0, 0, DateTimeKind.Unspecified);
			var expectedNextMessageShouldBeProcessed =
				TimeZoneInfo.ConvertTime(tomorrowUnspecified.AddHours(5), timezone, TimeZoneInfo.Local);

			now.Stub(x => x.UtcDateTime()).Return(today);
			var newSetting = new GamificationSetting("test")
			{
				AdherenceBadgeEnabled = false,
				AHTBadgeEnabled = false,
				AnsweredCallsBadgeEnabled = false
			};

			var calculationDate = TimeZoneInfo.ConvertTime(now.LocalDateOnly().AddDays(-1), TimeZoneInfo.Local, timezone);
			var message = new CalculateBadgeMessage
			{
				TimeZoneCode = timezone.Id,
				CalculationDate = new DateOnly(calculationDate)
			};

			target.Consume(message);

			calculator.AssertWasNotCalled(
				x => x.CalculateAHTBadges(new List<IPerson>(), "", DateOnly.Today, newSetting),
				o => o.IgnoreArguments());
			calculator.AssertWasNotCalled(
				x => x.CalculateAdherenceBadges(new List<IPerson>(), "", DateOnly.Today,
					AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime, newSetting),
				o => o.IgnoreArguments());
			calculator.AssertWasNotCalled(
				x => x.CalculateAnsweredCallsBadges(new List<IPerson>(), "", DateOnly.Today, newSetting),
				o => o.IgnoreArguments());

			serviceBus.AssertWasCalled(x => x.DelaySend(new DateTime(), new object()),
				o =>
					o.Constraints(
						Rhino.Mocks.Constraints.Is.Matching(new Predicate<DateTime>(m => m == expectedNextMessageShouldBeProcessed)),
						Rhino.Mocks.Constraints.Is.Matching(new Predicate<object[]>(m =>
						{
							var msg = ((CalculateBadgeMessage)m[0]);
							return msg.TimeZoneCode == TimeZoneInfo.Utc.Id
								&& msg.CalculationDate == message.CalculationDate.AddDays(1);
						}))));
		}

	
	
	[Test]
		public void ShouldNotCalculateBadgeWhenNoBadgeTypeEnableForAppliedSetting()
		{
			var timezone = TimeZoneInfo.Utc;
			var today = new DateTime(2014, 8, 8);
			var tomorrowUnspecified = new DateTime(2014, 8, 9, 0, 0, 0, DateTimeKind.Unspecified);
			var expectedNextMessageShouldBeProcessed =
				TimeZoneInfo.ConvertTime(tomorrowUnspecified.AddHours(5), timezone, TimeZoneInfo.Local);

			now.Stub(x => x.UtcDateTime()).Return(today);
			var newSetting = new GamificationSetting("test")
			{
				AdherenceBadgeEnabled = false,
				AHTBadgeEnabled = false,
				AnsweredCallsBadgeEnabled = false
			};

			var team = TeamFactory.CreateSimpleTeam("team");
			team.SetId(Guid.NewGuid());

			teamSettingsRepository.Stub(x => x.FindAllTeamGamificationSettingsSortedByTeam())
				.Return(new[] { new TeamGamificationSetting { Team = team, GamificationSetting = newSetting } });

			var calculationDate = TimeZoneInfo.ConvertTime(now.LocalDateOnly().AddDays(-1), TimeZoneInfo.Local, timezone);
			var message = new CalculateBadgeMessage
			{
				TimeZoneCode = timezone.Id,
				CalculationDate = new DateOnly(calculationDate)
			};

			target.Consume(message);

			calculator.AssertWasNotCalled(
				x => x.CalculateAHTBadges(new List<IPerson>(), "", DateOnly.Today, newSetting),
				o => o.IgnoreArguments());
			calculator.AssertWasNotCalled(
				x => x.CalculateAdherenceBadges(new List<IPerson>(), "", DateOnly.Today,
					AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime, newSetting),
				o => o.IgnoreArguments());
			calculator.AssertWasNotCalled(
				x => x.CalculateAnsweredCallsBadges(new List<IPerson>(), "", DateOnly.Today, newSetting),
				o => o.IgnoreArguments());

			serviceBus.AssertWasCalled(x => x.DelaySend(new DateTime(), new object()),
				o =>
					o.Constraints(
						Rhino.Mocks.Constraints.Is.Matching(new Predicate<DateTime>(m => m == expectedNextMessageShouldBeProcessed)),
						Rhino.Mocks.Constraints.Is.Matching(new Predicate<object[]>(m =>
						{
							var msg = ((CalculateBadgeMessage)m[0]);
							return msg.TimeZoneCode == TimeZoneInfo.Utc.Id
								&& msg.CalculationDate == message.CalculationDate.AddDays(1);
						}))));
		}

	}
}
