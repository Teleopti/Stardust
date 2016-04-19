using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers
{
	[TestFixture]
	public class BuildInGroupsAnalyticsUpdaterTest
	{
		private BuildInGroupsAnalyticsUpdaterBase _target;
		private IAnalyticsGroupPageRepository _analyticsGroupPageRepository;
		private ISkillRepository _skillRepository;
		private IPartTimePercentageRepository _partTimePercentageRepository;
		private IRuleSetBagRepository _ruleSetBagRepository;
		private IContractRepository _contractRepository;
		private IContractScheduleRepository _contractScheduleRepository;

		[SetUp]
		public void Setup()
		{
			_analyticsGroupPageRepository = MockRepository.GenerateMock<IAnalyticsGroupPageRepository>();
			_skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			_partTimePercentageRepository = MockRepository.GenerateMock<IPartTimePercentageRepository>();
			_ruleSetBagRepository = MockRepository.GenerateMock<IRuleSetBagRepository>();
			_contractRepository = MockRepository.GenerateMock<IContractRepository>();
			_contractScheduleRepository = MockRepository.GenerateMock<IContractScheduleRepository>();

			_target = new BuildInGroupsAnalyticsUpdaterBase(_analyticsGroupPageRepository, _skillRepository, _partTimePercentageRepository, _ruleSetBagRepository, _contractRepository, _contractScheduleRepository);
		}

		[Test]
		public void ShouldUpdatePartTimePercentage()
		{
			var @event = new SettingsForPersonPeriodChangedEvent();
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";
			@event.SetIdCollection(new [] {entityId});

			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(entityId))
				.Return(new AnalyticsGroup {GroupName = "GroupName", GroupCode = entityId});
			_partTimePercentageRepository.Stub(r => r.Get(entityId)).Return(new PartTimePercentage(updateGroupName));
			_ruleSetBagRepository.Stub(r => r.Get(entityId)).Return(null);
			_contractRepository.Stub(r => r.Get(entityId)).Return(null);
			_contractScheduleRepository.Stub(r => r.Get(entityId)).Return(null);
			_skillRepository.Stub(r => r.Get(entityId)).Return(null);

			_target.Handle(@event);

			_partTimePercentageRepository.AssertWasCalled(r => r.Get(entityId));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.UpdateGroupPage(Arg<AnalyticsGroup>.Matches(a => a.GroupName == updateGroupName)));
		}

		[Test]
		public void ShouldUpdateRuleSetBag()
		{
			var @event = new SettingsForPersonPeriodChangedEvent();
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";
			@event.SetIdCollection(new[] { entityId });

			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(entityId))
				.Return(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId });
			_partTimePercentageRepository.Stub(r => r.Get(entityId)).Return(null);
			_ruleSetBagRepository.Stub(r => r.Get(entityId)).Return(new RuleSetBag {Description = new Description(updateGroupName)});
			_contractRepository.Stub(r => r.Get(entityId)).Return(null);
			_contractScheduleRepository.Stub(r => r.Get(entityId)).Return(null);
			_skillRepository.Stub(r => r.Get(entityId)).Return(null);
			_target.Handle(@event);

			_partTimePercentageRepository.AssertWasCalled(r => r.Get(entityId));
			_ruleSetBagRepository.AssertWasCalled(r => r.Get(entityId));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.UpdateGroupPage(Arg<AnalyticsGroup>.Matches(a => a.GroupName == updateGroupName)));
		}

		[Test]
		public void ShouldUpdateContract()
		{
			var @event = new SettingsForPersonPeriodChangedEvent();
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";
			@event.SetIdCollection(new[] { entityId });

			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(entityId))
				.Return(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId });
			_partTimePercentageRepository.Stub(r => r.Get(entityId)).Return(null);
			_ruleSetBagRepository.Stub(r => r.Get(entityId)).Return(null);
			_contractRepository.Stub(r => r.Get(entityId)).Return(new Contract(updateGroupName));
			_contractScheduleRepository.Stub(r => r.Get(entityId)).Return(null);
			_skillRepository.Stub(r => r.Get(entityId)).Return(null);

			_target.Handle(@event);

			_partTimePercentageRepository.AssertWasCalled(r => r.Get(entityId));
			_ruleSetBagRepository.AssertWasCalled(r => r.Get(entityId));
			_contractRepository.AssertWasCalled(r => r.Get(entityId));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.UpdateGroupPage(Arg<AnalyticsGroup>.Matches(a => a.GroupName == updateGroupName)));
		}

		[Test]
		public void ShouldUpdateContractSchedule()
		{
			var @event = new SettingsForPersonPeriodChangedEvent();
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";
			@event.SetIdCollection(new[] { entityId });

			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(entityId))
				.Return(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId });
			_partTimePercentageRepository.Stub(r => r.Get(entityId)).Return(null);
			_ruleSetBagRepository.Stub(r => r.Get(entityId)).Return(null);
			_contractRepository.Stub(r => r.Get(entityId)).Return(null);
			_contractScheduleRepository.Stub(r => r.Get(entityId)).Return(new ContractSchedule(updateGroupName));
			_skillRepository.Stub(r => r.Get(entityId)).Return(null);

			_target.Handle(@event);

			_partTimePercentageRepository.AssertWasCalled(r => r.Get(entityId));
			_ruleSetBagRepository.AssertWasCalled(r => r.Get(entityId));
			_contractRepository.AssertWasCalled(r => r.Get(entityId));
			_contractScheduleRepository.AssertWasCalled(r => r.Get(entityId));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.UpdateGroupPage(Arg<AnalyticsGroup>.Matches(a => a.GroupName == updateGroupName)));
		}

		[Test]
		public void ShouldUpdateSkill()
		{
			var @event = new SettingsForPersonPeriodChangedEvent();
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";
			@event.SetIdCollection(new[] { entityId });

			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(entityId))
				.Return(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId });
			_partTimePercentageRepository.Stub(r => r.Get(entityId)).Return(null);
			_ruleSetBagRepository.Stub(r => r.Get(entityId)).Return(null);
			_contractRepository.Stub(r => r.Get(entityId)).Return(null);
			_contractScheduleRepository.Stub(r => r.Get(entityId)).Return(null);
			_skillRepository.Stub(r => r.Get(entityId)).Return(new Skill {Name = updateGroupName});

			_target.Handle(@event);

			_partTimePercentageRepository.AssertWasCalled(r => r.Get(entityId));
			_ruleSetBagRepository.AssertWasCalled(r => r.Get(entityId));
			_contractRepository.AssertWasCalled(r => r.Get(entityId));
			_contractScheduleRepository.AssertWasCalled(r => r.Get(entityId));
			_skillRepository.AssertWasCalled(r => r.Get(entityId));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.UpdateGroupPage(Arg<AnalyticsGroup>.Matches(a => a.GroupName == updateGroupName)));
		}

		[Test]
		public void ShouldNotUpdateIfNotExisting()
		{
			var @event = new SettingsForPersonPeriodChangedEvent();
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";
			@event.SetIdCollection(new[] { entityId });

			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(entityId))
				.Return(null);

			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasNotCalled(r => r.UpdateGroupPage(Arg<AnalyticsGroup>.Matches(a => a.GroupName == updateGroupName)));
		}

		[Test]
		public void ShouldNotUpdateIfNotSpecificType()
		{
			var @event = new SettingsForPersonPeriodChangedEvent();
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";
			@event.SetIdCollection(new[] { entityId });

			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(entityId))
				.Return(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId });
			_partTimePercentageRepository.Stub(r => r.Get(entityId)).Return(null);
			_ruleSetBagRepository.Stub(r => r.Get(entityId)).Return(null);
			_contractRepository.Stub(r => r.Get(entityId)).Return(null);
			_contractScheduleRepository.Stub(r => r.Get(entityId)).Return(null);
			_skillRepository.Stub(r => r.Get(entityId)).Return(null);

			_target.Handle(@event);

			_partTimePercentageRepository.AssertWasCalled(r => r.Get(entityId));
			_ruleSetBagRepository.AssertWasCalled(r => r.Get(entityId));
			_contractRepository.AssertWasCalled(r => r.Get(entityId));
			_contractScheduleRepository.AssertWasCalled(r => r.Get(entityId));
			_skillRepository.AssertWasCalled(r => r.Get(entityId));
			_analyticsGroupPageRepository.AssertWasNotCalled(r => r.UpdateGroupPage(Arg<AnalyticsGroup>.Matches(a => a.GroupName == updateGroupName)));
		}
	}
}