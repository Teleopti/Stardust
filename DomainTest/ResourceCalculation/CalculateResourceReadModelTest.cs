﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[DomainTest]
	public class CalculateResourceReadModelTest : ISetup
	{
		public UpdateStaffingLevelReadModel Target;
		public FakeExtractSkillStaffDataForResourceCalculation ExtractSkillStaffDataForResourceCalculation;
		public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;
		public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<UpdateStaffingLevelReadModel>().For<IUpdateStaffingLevelReadModel>();
			system.UseTestDouble<FakeExtractSkillStaffDataForResourceCalculation>().For<IExtractSkillStaffDataForResourceCalculation>();
			system.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
		}

		[Test]
		public void ShouldPerformResourceCalculation()
		{
			ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriodExt = new SkillSkillStaffPeriodExtendedDictionary();
			ISkill skill = SkillFactory.CreateSkillWithId("skill1");

			ISkillStaffPeriodDictionary skillStaffPeriodDic = new SkillStaffPeriodDictionary(skill);
			var dateTime = new DateTime(2016, 10, 03, 11, 0, 0, DateTimeKind.Utc);
			skillStaffPeriodDic.Add(new DateTimePeriod(dateTime,dateTime.AddMinutes(15)), getSkillStaffPeriod(new DateTimePeriod(dateTime, dateTime.AddMinutes(15))) );
			skillStaffPeriodDic.Add(new DateTimePeriod(dateTime.AddMinutes(15), dateTime.AddMinutes(30)), getSkillStaffPeriod(new DateTimePeriod(dateTime.AddMinutes(15), dateTime.AddMinutes(30))));
			skillStaffPeriodDic.Add(new DateTimePeriod(dateTime.AddMinutes(30), dateTime.AddMinutes(60)), getSkillStaffPeriod(new DateTimePeriod(dateTime.AddMinutes(30), dateTime.AddMinutes(60))));

			skillStaffPeriodExt.Add(skill,skillStaffPeriodDic);
			var fakeholder = new FakeSkillStaffPeriodHolder();
			fakeholder.SetDictionary(skillStaffPeriodExt);
			var  fakeResourceCalculationData = new FakeResourceCalculationData();
			fakeResourceCalculationData.SetSkills(new List<ISkill> {skill});
			fakeResourceCalculationData.SetSkillStaffPeriodHolder(fakeholder);
			ExtractSkillStaffDataForResourceCalculation.FakeResourceCalculationData = fakeResourceCalculationData;
			Target.Update(new DateTimePeriod(dateTime,dateTime.AddDays(1)));

			var staffing = ScheduleForecastSkillReadModelRepository.GetBySkill(skill.Id.GetValueOrDefault(), dateTime, dateTime.AddMinutes(90)).ToList(); 
			staffing.Count.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldCallPurge()
		{
			ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriodExt = new SkillSkillStaffPeriodExtendedDictionary();
			ISkill skill = SkillFactory.CreateSkillWithId("skill1");

			ISkillStaffPeriodDictionary skillStaffPeriodDic = new SkillStaffPeriodDictionary(skill);
			var dateTime = new DateTime(2016, 10, 03, 11, 0, 0, DateTimeKind.Utc);
			skillStaffPeriodDic.Add(new DateTimePeriod(dateTime, dateTime.AddMinutes(15)), getSkillStaffPeriod(new DateTimePeriod(dateTime, dateTime.AddMinutes(15))));

			skillStaffPeriodExt.Add(skill, skillStaffPeriodDic);
			var fakeholder = new FakeSkillStaffPeriodHolder();
			fakeholder.SetDictionary(skillStaffPeriodExt);
			var fakeResourceCalculationData = new FakeResourceCalculationData();
			fakeResourceCalculationData.SetSkills(new List<ISkill> { skill });
			fakeResourceCalculationData.SetSkillStaffPeriodHolder(fakeholder);
			ExtractSkillStaffDataForResourceCalculation.FakeResourceCalculationData = fakeResourceCalculationData;
			Target.Update(new DateTimePeriod(dateTime, dateTime.AddDays(1)));
			ScheduleForecastSkillReadModelRepository.PurgeWasCalled.Should().Be.True();
		}

		[Test]
		public void ShouldPurgeChangesOlderThanTimeWhenDataWasLoaded()
		{
			ScheduleForecastSkillReadModelRepository.UtcNow = new DateTime(2016, 10, 03, 11, 15, 0, DateTimeKind.Utc);
			Now.Is(new DateTime(2016, 10, 03, 11, 15, 0, DateTimeKind.Utc));
			var dateTime = new DateTime(2016, 10, 03, 11, 0, 0, DateTimeKind.Utc);
			ScheduleForecastSkillReadModelRepository.PersistChange(new StaffingIntervalChange() { StartDateTime = dateTime, EndDateTime = dateTime.AddMinutes(15) });
			ScheduleForecastSkillReadModelRepository.PersistChange(new StaffingIntervalChange() { StartDateTime = dateTime.AddMinutes(15), EndDateTime = dateTime.AddMinutes(30) });
			ScheduleForecastSkillReadModelRepository.UtcNow = new DateTime(2016, 10, 03, 11, 30, 0, DateTimeKind.Utc);

			Now.Is(new DateTime(2016, 10, 03, 11, 30, 0, DateTimeKind.Utc));
			ScheduleForecastSkillReadModelRepository.PersistChange(new StaffingIntervalChange() { StartDateTime = dateTime.AddMinutes(30), EndDateTime = dateTime.AddMinutes(60) });


			ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriodExt = new SkillSkillStaffPeriodExtendedDictionary();
			ISkill skill = SkillFactory.CreateSkillWithId("skill1");

			ISkillStaffPeriodDictionary skillStaffPeriodDic = new SkillStaffPeriodDictionary(skill);
			
			skillStaffPeriodDic.Add(new DateTimePeriod(dateTime, dateTime.AddMinutes(15)), getSkillStaffPeriod(new DateTimePeriod(dateTime, dateTime.AddMinutes(15))));
			skillStaffPeriodDic.Add(new DateTimePeriod(dateTime.AddMinutes(15), dateTime.AddMinutes(30)), getSkillStaffPeriod(new DateTimePeriod(dateTime.AddMinutes(15), dateTime.AddMinutes(30))));
			skillStaffPeriodDic.Add(new DateTimePeriod(dateTime.AddMinutes(30), dateTime.AddMinutes(60)), getSkillStaffPeriod(new DateTimePeriod(dateTime.AddMinutes(30), dateTime.AddMinutes(60))));

			skillStaffPeriodExt.Add(skill, skillStaffPeriodDic);
			var fakeholder = new FakeSkillStaffPeriodHolder();
			fakeholder.SetDictionary(skillStaffPeriodExt);
			var fakeResourceCalculationData = new FakeResourceCalculationData();
			fakeResourceCalculationData.SetSkills(new List<ISkill> { skill });
			fakeResourceCalculationData.SetSkillStaffPeriodHolder(fakeholder);
			ExtractSkillStaffDataForResourceCalculation.FakeResourceCalculationData = fakeResourceCalculationData;

			Target.Update(new DateTimePeriod(dateTime, dateTime.AddDays(1)));

			var changes =  ScheduleForecastSkillReadModelRepository.GetReadModelChanges(new DateTimePeriod(dateTime,dateTime.AddMinutes(90)));
			changes.ToList().Count.Should().Be.EqualTo(1);
			changes.FirstOrDefault().StartDateTime.Should().Be.EqualTo(dateTime.AddMinutes(30));
			changes.FirstOrDefault().EndDateTime.Should().Be.EqualTo(dateTime.AddMinutes(60));
		}


		private ISkillStaffPeriod getSkillStaffPeriod(DateTimePeriod period)
		{
			var skillStaffperiod = new SkillStaffPeriod(period, new Task(), new ServiceAgreement(), new StaffingCalculatorServiceFacade());
			return skillStaffperiod;
		}

	}
}
