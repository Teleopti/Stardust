using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers
{
	[TestFixture]
	public class BuildInGroupsAnalyticsUpdaterTest
	{
		private BuildInGroupsAnalyticsUpdater _target;
		private IAnalyticsGroupPageRepository _analyticsGroupPageRepository;
		private ISkillRepository _skillRepository;
		private IPartTimePercentageRepository _partTimePercentageRepository;
		private IRuleSetBagRepository _ruleSetBagRepository;
		private IContractRepository _contractRepository;
		private IContractScheduleRepository _contractScheduleRepository;
		private Guid _businessUnitId;

		[SetUp]
		public void Setup()
		{
			_analyticsGroupPageRepository = MockRepository.GenerateMock<IAnalyticsGroupPageRepository>();
			_skillRepository = new FakeSkillRepository();
			_partTimePercentageRepository = new FakePartTimePercentageRepository();
			_ruleSetBagRepository = new FakeRuleSetBagRepository();
			_contractRepository = new FakeContractRepository();
			_contractScheduleRepository = new FakeContractScheduleRepository();

			_target = new BuildInGroupsAnalyticsUpdater(_analyticsGroupPageRepository, _skillRepository, _partTimePercentageRepository, _ruleSetBagRepository, _contractRepository, _contractScheduleRepository);
			_businessUnitId = Guid.NewGuid();
		}

		[Test]
		public void ShouldUpdatePartTimePercentage()
		{
			var @event = new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";
			@event.SetIdCollection(new [] {entityId});

			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(entityId, _businessUnitId))
				.Return(new AnalyticsGroup {GroupName = "GroupName", GroupCode = entityId});
			_partTimePercentageRepository.Add(new PartTimePercentage(updateGroupName).WithId(entityId));

			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.UpdateGroupPage(Arg<AnalyticsGroup>.Matches(a => a.GroupName == updateGroupName)));
		}

		[Test]
		public void ShouldUpdateRuleSetBag()
		{
			var @event = new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";
			@event.SetIdCollection(new[] { entityId });

			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(entityId, _businessUnitId))
				.Return(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId });
			_ruleSetBagRepository.Add(new RuleSetBag { Description = new Description(updateGroupName) }.WithId(entityId));
			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.UpdateGroupPage(Arg<AnalyticsGroup>.Matches(a => a.GroupName == updateGroupName)));
		}

		[Test]
		public void ShouldUpdateContract()
		{
			var @event = new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";
			@event.SetIdCollection(new[] { entityId });

			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(entityId, _businessUnitId))
				.Return(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId });
			_contractRepository.Add(new Contract(updateGroupName).WithId(entityId));

			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.UpdateGroupPage(Arg<AnalyticsGroup>.Matches(a => a.GroupName == updateGroupName)));
		}

		[Test]
		public void ShouldUpdateContractSchedule()
		{
			var @event = new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";
			@event.SetIdCollection(new[] { entityId });

			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(entityId, _businessUnitId))
				.Return(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId });
			_contractScheduleRepository.Add(new ContractSchedule(updateGroupName).WithId(entityId));

			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.UpdateGroupPage(Arg<AnalyticsGroup>.Matches(a => a.GroupName == updateGroupName)));
		}

		[Test]
		public void ShouldUpdateSkill()
		{
			var @event = new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";
			@event.SetIdCollection(new[] { entityId });

			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(entityId, _businessUnitId))
				.Return(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId });
			var skill = new Domain.Forecasting.Skill().WithId(entityId);
			skill.ChangeName(updateGroupName);
			_skillRepository.Add(skill);

			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.UpdateGroupPage(Arg<AnalyticsGroup>.Matches(a => a.GroupName == updateGroupName)));
		}

		[Test]
		public void ShouldNotUpdateIfNotExisting()
		{
			var @event = new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";
			@event.SetIdCollection(new[] { entityId });

			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(entityId, _businessUnitId))
				.Return(null);

			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasNotCalled(r => r.UpdateGroupPage(Arg<AnalyticsGroup>.Matches(a => a.GroupName == updateGroupName)));
		}

		[Test]
		public void ShouldNotUpdateIfNotSpecificType()
		{
			var @event = new SettingsForPersonPeriodChangedEvent {LogOnBusinessUnitId = _businessUnitId};
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";
			@event.SetIdCollection(new[] { entityId });

			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(entityId, _businessUnitId))
				.Return(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId });

			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasNotCalled(r => r.UpdateGroupPage(Arg<AnalyticsGroup>.Matches(a => a.GroupName == updateGroupName)));
		}
	}
}