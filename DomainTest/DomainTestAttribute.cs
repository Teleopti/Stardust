﻿using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest
{
	public class DomainTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			//TODO: move this to common
			system.AddModule(new SchedulingCommonModule(configuration));
			system.AddModule(new RuleSetModule(configuration, true));
			system.AddModule(new OutboundScheduledResourcesProviderModule());
			//
			system.UseTestDouble<FakeScheduleDictionaryPersister>().For<IScheduleDictionaryPersister>();
			system.UseTestDouble<FakePersonAssignmentRepository>().For<IPersonAssignmentRepository>();
			system.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
			system.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			system.UseTestDouble<FakeScenarioRepository>().For<IScenarioRepository>();
			system.UseTestDouble<FakeActivityRepository>().For<IActivityRepository>();
			system.UseTestDouble<FakePlanningPeriodRepository>().For<IPlanningPeriodRepository>();
			system.UseTestDouble<FakeDayOffTemplateRepository>().For<IDayOffTemplateRepository>();
			system.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			system.UseTestDouble<FakeRepositoryFactory>().For<IRepositoryFactory>();

			/*
			                LoadActivity(repositoryFactory.CreateActivityRepository(unitOfWork));
                LoadAbsences(repositoryFactory.CreateAbsenceRepository(unitOfWork));
                LoadDayOffs(repositoryFactory.CreateDayOffRepository(unitOfWork));
                LoadShiftCategory(repositoryFactory.CreateShiftCategoryRepository(unitOfWork));
                LoadContracts(repositoryFactory.CreateContractRepository(unitOfWork));
                LoadContractSchedules(repositoryFactory.CreateContractScheduleRepository(unitOfWork));
                loadScheduleTags(repositoryFactory.CreateScheduleTagRepository(unitOfWork));
	            loadWorkflowControlSets(repositoryFactory.CreateWorkflowControlSetRepository(unitOfWork));
	*/

			system.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();
		}
	}
}