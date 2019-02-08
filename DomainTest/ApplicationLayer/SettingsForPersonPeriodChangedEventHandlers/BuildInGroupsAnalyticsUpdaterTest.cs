using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers
{
	[TestFixture]
	[DomainTest]
	public class BuildInGroupsAnalyticsUpdaterTest : IExtendSystem
	{
		public BuildInGroupsAnalyticsUpdater Target;
		public IAnalyticsGroupPageRepository AnalyticsGroupPageRepository;
		public ISkillRepository SkillRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;
		public IRuleSetBagRepository RuleSetBagRepository;
		public IContractRepository ContractRepository;
		public IContractScheduleRepository ContractScheduleRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		private readonly Guid _businessUnitId = Guid.NewGuid();
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<BuildInGroupsAnalyticsUpdater>();
		}

		[Test]
		public void ShouldUpdatePartTimePercentage()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";

			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId, BusinessUnitCode = _businessUnitId});
			PartTimePercentageRepository.Add(new PartTimePercentage(updateGroupName).WithId(entityId));

			Target.Handle(new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId, IdCollection = new[] { entityId } });

			AnalyticsGroupPageRepository.GetGroupPageByGroupCode(entityId, _businessUnitId)
				.GroupName.Should()
				.Be.EqualTo(updateGroupName);
		}

		[Test]
		public void ShouldUpdateRuleSetBag()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";

			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId, BusinessUnitCode = _businessUnitId });
			RuleSetBagRepository.Add(new RuleSetBag { Description = new Description(updateGroupName) }.WithId(entityId));
			Target.Handle(new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId, IdCollection = new[] { entityId } });

			AnalyticsGroupPageRepository.GetGroupPageByGroupCode(entityId, _businessUnitId)
				.GroupName.Should()
				.Be.EqualTo(updateGroupName);
		}

		[Test]
		public void ShouldUpdateContract()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";

			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId, BusinessUnitCode = _businessUnitId });
			ContractRepository.Add(new Contract(updateGroupName).WithId(entityId));

			Target.Handle(new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId, IdCollection = new[] { entityId } });

			AnalyticsGroupPageRepository.GetGroupPageByGroupCode(entityId, _businessUnitId)
				.GroupName.Should()
				.Be.EqualTo(updateGroupName);
		}

		[Test]
		public void ShouldUpdateContractSchedule()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";

			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId, BusinessUnitCode = _businessUnitId });
			ContractScheduleRepository.Add(new ContractSchedule(updateGroupName).WithId(entityId));

			Target.Handle(new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId, IdCollection = new[] { entityId } });

			AnalyticsGroupPageRepository.GetGroupPageByGroupCode(entityId, _businessUnitId)
				.GroupName.Should()
				.Be.EqualTo(updateGroupName);
		}

		[Test]
		public void ShouldUpdateSkill()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
			var entityId = Guid.NewGuid();
			var updateGroupName = "UpdateGroupName";

			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId, BusinessUnitCode = _businessUnitId });
			var skill = new Domain.Forecasting.Skill("_").WithId(entityId);
			skill.ChangeName(updateGroupName);
			SkillRepository.Add(skill);

			Target.Handle(new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId, IdCollection = new[] { entityId } });

			AnalyticsGroupPageRepository.GetGroupPageByGroupCode(entityId, _businessUnitId)
				.GroupName.Should()
				.Be.EqualTo(updateGroupName);
		}
		
		[Test]
		public void ShouldNotUpdateIfNotExisting()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
			var entityId = Guid.NewGuid();

			Target.Handle(new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId, IdCollection = new[] { entityId } });

			AnalyticsGroupPageRepository.GetGroupPageByGroupCode(entityId, _businessUnitId)
				.Should().Be.Null();
		}

		[Test]
		public void ShouldNotUpdateIfNotSpecificType()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
			var entityId = Guid.NewGuid();

			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup { GroupName = "GroupName", GroupCode = entityId, BusinessUnitCode = _businessUnitId });

			Target.Handle(new SettingsForPersonPeriodChangedEvent { LogOnBusinessUnitId = _businessUnitId, IdCollection = new[] { entityId } });

			AnalyticsGroupPageRepository.GetGroupPageByGroupCode(entityId, _businessUnitId)
				.GroupName.Should()
				.Be.EqualTo("GroupName");
		}
	}
}