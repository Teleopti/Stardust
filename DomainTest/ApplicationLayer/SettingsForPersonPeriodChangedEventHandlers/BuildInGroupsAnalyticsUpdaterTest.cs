using System;
using NUnit.Framework;
using SharpTestsEx;
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
			_analyticsGroupPageRepository = new FakeAnalyticsGroupPageRepository();
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
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";

			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId, BusinessUnitCode = _businessUnitId});
			_partTimePercentageRepository.Add(new PartTimePercentage(updateGroupName).WithId(entityId));

			_target.Handle(new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId, IdCollection = { entityId } });

			_analyticsGroupPageRepository.GetGroupPageByGroupCode(entityId, _businessUnitId)
				.GroupName.Should()
				.Be.EqualTo(updateGroupName);
		}

		[Test]
		public void ShouldUpdateRuleSetBag()
		{
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";

			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId, BusinessUnitCode = _businessUnitId });
			_ruleSetBagRepository.Add(new RuleSetBag { Description = new Description(updateGroupName) }.WithId(entityId));
			_target.Handle(new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId, IdCollection = { entityId } });

			_analyticsGroupPageRepository.GetGroupPageByGroupCode(entityId, _businessUnitId)
				.GroupName.Should()
				.Be.EqualTo(updateGroupName);
		}

		[Test]
		public void ShouldUpdateContract()
		{
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";

			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId, BusinessUnitCode = _businessUnitId });
			_contractRepository.Add(new Contract(updateGroupName).WithId(entityId));

			_target.Handle(new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId, IdCollection = { entityId } });

			_analyticsGroupPageRepository.GetGroupPageByGroupCode(entityId, _businessUnitId)
				.GroupName.Should()
				.Be.EqualTo(updateGroupName);
		}

		[Test]
		public void ShouldUpdateContractSchedule()
		{
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";

			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId, BusinessUnitCode = _businessUnitId });
			_contractScheduleRepository.Add(new ContractSchedule(updateGroupName).WithId(entityId));

			_target.Handle(new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId, IdCollection = { entityId } });

			_analyticsGroupPageRepository.GetGroupPageByGroupCode(entityId, _businessUnitId)
				.GroupName.Should()
				.Be.EqualTo(updateGroupName);
		}

		[Test]
		public void ShouldUpdateSkill()
		{
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";

			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId, BusinessUnitCode = _businessUnitId });
			var skill = new Domain.Forecasting.Skill().WithId(entityId);
			skill.ChangeName(updateGroupName);
			_skillRepository.Add(skill);

			_target.Handle(new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId, IdCollection = { entityId } });

			_analyticsGroupPageRepository.GetGroupPageByGroupCode(entityId, _businessUnitId)
				.GroupName.Should()
				.Be.EqualTo(updateGroupName);
		}
		
		[Test]
		public void ShouldNotUpdateIfNotExisting()
		{
			var entityId = Guid.NewGuid();

			_target.Handle(new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId, IdCollection = { entityId } });

			_analyticsGroupPageRepository.GetGroupPageByGroupCode(entityId, _businessUnitId)
				.Should().Be.Null();
		}

		[Test]
		public void ShouldNotUpdateIfNotSpecificType()
		{
			var entityId = Guid.NewGuid();

			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId, BusinessUnitCode = _businessUnitId });

			_target.Handle(new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId, IdCollection = { entityId } });

			_analyticsGroupPageRepository.GetGroupPageByGroupCode(entityId, _businessUnitId)
				.GroupName.Should()
				.Be.EqualTo("GroupName");
		}
	}
}