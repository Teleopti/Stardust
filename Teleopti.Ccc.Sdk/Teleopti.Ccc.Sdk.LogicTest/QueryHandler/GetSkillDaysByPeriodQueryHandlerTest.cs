﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetSkillDaysByPeriodQueryHandlerTest
	{
		private GetSkillDaysByPeriodQueryHandler target;
		private IAssembler<ISkillStaffPeriod, SkillDataDto> assembler;
		private IPersonRepository personRepository;
		private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;
		private ICurrentScenario currentScenario;
		private ISkillRepository skillRepository;
		private IResourceCalculationPrerequisitesLoader resourceCalculationPrerequisitesLoader;
		private IDateTimePeriodAssembler dateTimePeriodAssembler;
		private IResourceOptimizationHelper resourceOptimizationHelper;
		private ILoadSchedulingStateHolderForResourceCalculation loadSchedulingStateHolderForResourceCalculation;
		private ISchedulingResultStateHolder schedulingResultStateHolder;
		private IServiceLevelCalculator serviceLevelCalculator;

		[SetUp]
		public void Setup()
		{
			assembler = MockRepository.GenerateMock<IAssembler<ISkillStaffPeriod, SkillDataDto>>();
			dateTimePeriodAssembler = MockRepository.GenerateMock<IDateTimePeriodAssembler>();
			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			currentScenario = MockRepository.GenerateMock<ICurrentScenario>();
			currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			resourceCalculationPrerequisitesLoader = MockRepository.GenerateMock<IResourceCalculationPrerequisitesLoader>();
			resourceOptimizationHelper = MockRepository.GenerateMock<IResourceOptimizationHelper>();
			loadSchedulingStateHolderForResourceCalculation =
				MockRepository.GenerateMock<ILoadSchedulingStateHolderForResourceCalculation>();
			schedulingResultStateHolder = MockRepository.GenerateMock<ISchedulingResultStateHolder>();
			serviceLevelCalculator = MockRepository.GenerateMock<IServiceLevelCalculator>();
			target = new GetSkillDaysByPeriodQueryHandler(dateTimePeriodAssembler, assembler, currentScenario, personRepository,
			                                              skillRepository, resourceCalculationPrerequisitesLoader,
			                                              currentUnitOfWorkFactory, resourceOptimizationHelper,
			                                              loadSchedulingStateHolderForResourceCalculation,
			                                              schedulingResultStateHolder, serviceLevelCalculator);
		}

		[Test]
		public void ShouldReturnSkillDaysForGivenPeriod()
		{
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var skill = SkillFactory.CreateSkill("skill");
			var skills = new List<ISkill> { skill };
			var staffPeriodHolder = MockRepository.GenerateMock<ISkillStaffPeriodHolder>();
			var skillStaffPeriods = new ISkillStaffPeriod[] { };
			var period = new DateOnlyPeriod(2014, 3, 31, 2014, 4, 3);

			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			personRepository.Stub(x => x.FindPeopleInOrganization(period, true)).Return(new List<IPerson>{new Person()});
			schedulingResultStateHolder.Stub(x => x.VisibleSkills).Return(skills);
			schedulingResultStateHolder.Stub(x => x.SkillStaffPeriodHolder).Return(staffPeriodHolder);
			staffPeriodHolder.Stub(x => x.SkillStaffPeriodList(skills, new DateOnlyPeriod(2014,4,1,2014,4,1).ToDateTimePeriod(TimeZoneInfo.Utc))).Return(skillStaffPeriods);
			staffPeriodHolder.Stub(x => x.SkillStaffPeriodList(skills, new DateOnlyPeriod(2014,4,2,2014,4,2).ToDateTimePeriod(TimeZoneInfo.Utc))).Return(skillStaffPeriods);
			assembler.Stub(x => x.DomainEntitiesToDtos(skillStaffPeriods)).Return(new List<SkillDataDto>());

			target.Handle(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto {StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 2)},
				TimeZoneId = TimeZoneInfo.Utc.Id
			}).Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldInitializeAllSkills()
		{
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var skill = SkillFactory.CreateSkill("skill");
			var skills = new List<ISkill> { skill };
			var staffPeriodHolder = MockRepository.GenerateMock<ISkillStaffPeriodHolder>();
			var skillStaffPeriods = new ISkillStaffPeriod[] { };
			var period = new DateOnlyPeriod(2014, 3, 31, 2014, 4, 3);

			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			personRepository.Stub(x => x.FindPeopleInOrganization(period, true)).Return(new List<IPerson> { new Person() });
			schedulingResultStateHolder.Stub(x => x.VisibleSkills).Return(skills);
			schedulingResultStateHolder.Stub(x => x.SkillStaffPeriodHolder).Return(staffPeriodHolder);
			staffPeriodHolder.Stub(x => x.SkillStaffPeriodList(skills, new DateOnlyPeriod(2014, 4, 1, 2014, 4, 1).ToDateTimePeriod(TimeZoneInfo.Utc))).Return(skillStaffPeriods);
			staffPeriodHolder.Stub(x => x.SkillStaffPeriodList(skills, new DateOnlyPeriod(2014, 4, 2, 2014, 4, 2).ToDateTimePeriod(TimeZoneInfo.Utc))).Return(skillStaffPeriods);
			assembler.Stub(x => x.DomainEntitiesToDtos(skillStaffPeriods)).Return(new List<SkillDataDto>());

			target.Handle(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 2) },
				TimeZoneId = TimeZoneInfo.Utc.Id
			});

			skillRepository.AssertWasCalled(x => x.LoadAll());
		}

		[Test]
		public void ShouldNotIncludeMultisiteSkills()
		{
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			var multisiteSkill = SkillFactory.CreateMultisiteSkill("multi");
			var skills = new [] { skill, multisiteSkill };
			var skillStaffPeriod = MockRepository.GenerateMock<ISkillStaffPeriod>();
			var skillStaffPeriods = new[] { skillStaffPeriod };
			var period = new DateOnlyPeriod(2014, 3, 31, 2014, 4, 2);
			var skillDay = MockRepository.GenerateMock<ISkillDay>();

			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			personRepository.Stub(x => x.FindPeopleInOrganization(period, true)).Return(new List<IPerson> { new Person() });
			assembler.Stub(x => x.DomainEntitiesToDtos(skillStaffPeriods)).Return(new List<SkillDataDto>());
			skillDay.Stub(x => x.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(skillStaffPeriods));
			skillStaffPeriod.Stub(x => x.Period)
			                .Return(new DateTimePeriod(2014, 4, 1, 2014, 4, 1).ChangeEndTime(TimeSpan.FromMinutes(15)));

			schedulingResultStateHolder = new SchedulingResultStateHolder();
			schedulingResultStateHolder.AddSkills(skills);
			schedulingResultStateHolder.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>
				{
					{skill, new List<ISkillDay> { skillDay}},
					{multisiteSkill, new List<ISkillDay> { skillDay}}
				};

			target = new GetSkillDaysByPeriodQueryHandler(dateTimePeriodAssembler, assembler, currentScenario, personRepository,
											  skillRepository, resourceCalculationPrerequisitesLoader,
											  currentUnitOfWorkFactory, resourceOptimizationHelper,
											  loadSchedulingStateHolderForResourceCalculation,
											  schedulingResultStateHolder, serviceLevelCalculator);

			target.Handle(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 1) },
				TimeZoneId = TimeZoneInfo.Utc.Id
			}).Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldIncludeSubSkillForMultisiteSkills()
		{
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var multisiteSkill = SkillFactory.CreateMultisiteSkill("multi");
			var childskill = SkillFactory.CreateChildSkill("S1", multisiteSkill);
			var skills = new ISkill[] { multisiteSkill, childskill };
			var skillStaffPeriod = MockRepository.GenerateMock<ISkillStaffPeriod>();
			var skillStaffPeriods = new[] { skillStaffPeriod };
			var period = new DateOnlyPeriod(2014, 3, 31, 2014, 4, 2);
			var skillDay = MockRepository.GenerateMock<ISkillDay>();

			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			personRepository.Stub(x => x.FindPeopleInOrganization(period, true)).Return(new List<IPerson> { new Person() });
			assembler.Stub(x => x.DomainEntitiesToDtos(skillStaffPeriods)).Return(new List<SkillDataDto>());
			skillDay.Stub(x => x.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(skillStaffPeriods));
			skillStaffPeriod.Stub(x => x.Period)
							.Return(new DateTimePeriod(2014, 4, 1, 2014, 4, 1).ChangeEndTime(TimeSpan.FromMinutes(15)));

			schedulingResultStateHolder = new SchedulingResultStateHolder();
			schedulingResultStateHolder.AddSkills(skills);
			schedulingResultStateHolder.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>
				{
					{childskill, new List<ISkillDay> { skillDay}},
					{multisiteSkill, new List<ISkillDay> { skillDay}}
				};

			target = new GetSkillDaysByPeriodQueryHandler(dateTimePeriodAssembler, assembler, currentScenario, personRepository,
											  skillRepository, resourceCalculationPrerequisitesLoader,
											  currentUnitOfWorkFactory, resourceOptimizationHelper,
											  loadSchedulingStateHolderForResourceCalculation,
											  schedulingResultStateHolder, serviceLevelCalculator);

			target.Handle(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 1) },
				TimeZoneId = TimeZoneInfo.Utc.Id
			}).Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotAllowLoadingMoreThan31Days()
		{
			Assert.Throws<FaultException>(()=>target.Handle(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto {StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 5, 2)},
				TimeZoneId = TimeZoneInfo.Utc.Id
			}));
		}
	}
}