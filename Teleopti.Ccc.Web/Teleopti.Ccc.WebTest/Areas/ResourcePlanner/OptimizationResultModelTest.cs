﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	[TestFixture]
	public class OptimizationResultModelTest
	{
		private OptimizationResultModel _target;
		private ISkill _skill1;
		private ISkill _skill2;
		private ISkillDay _skillDay1;
		private ISkillDay _skillDay2;
		private Dictionary<ISkill, IList<ISkillDay>> _skillDays;
		private DateOnlyPeriod _period;

		[SetUp]
		public void Setup()
		{
			_target = new OptimizationResultModel();
			_skill1 = SkillFactory.CreateSkill("Skill1",new SkillTypePhone(new Description(), ForecastSource.InboundTelephony), 15);
			_skill2 = SkillFactory.CreateSkill("Skill2");
			_skillDay1 = SkillDayFactory.CreateSkillDay(_skill1, new DateOnly(2015, 9, 4));
			_skillDay1.SkillDayCalculator = new SkillDayCalculator(_skill1, new List<ISkillDay> { _skillDay1 }, new DateOnlyPeriod(new DateOnly(2015, 9, 4), new DateOnly(2015, 9, 4)));
			_skillDay2 = SkillDayFactory.CreateSkillDay(_skill1, new DateOnly(2015, 9, 5));
			_skillDay2.SkillDayCalculator = new SkillDayCalculator(_skill1, new List<ISkillDay> { _skillDay2 }, new DateOnlyPeriod(new DateOnly(2015, 9, 5), new DateOnly(2015, 9, 5)));
			_skillDays = new Dictionary<ISkill, IList<ISkillDay>>();
			_period = new DateOnlyPeriod(new DateOnly(2015, 9, 4), new DateOnly(2015, 9, 5));
		}

		[Test]
		public void ShouldMapSkillNames()
		{
			_skillDays.Add(_skill1, new List<ISkillDay>());
			_skillDays.Add(_skill2, new List<ISkillDay>());
			_target.Map(_skillDays, _period);
			Assert.AreEqual(_skill1.Name, _target.SkillResultList.ToList()[0].SkillName);
			Assert.AreEqual(_skill2.Name, _target.SkillResultList.ToList()[1].SkillName);
		}

		[Test]
		public void ShouldMapSkillDaysDate()
		{
			_skillDays.Add(_skill1, new List<ISkillDay>{_skillDay1, _skillDay2});
			_target.Map(_skillDays, _period);
			Assert.AreEqual(_skillDay1.CurrentDate, _target.SkillResultList.ToList()[0].SkillDetails.ToList()[0].Date);
			Assert.AreEqual(_skillDay2.CurrentDate, _target.SkillResultList.ToList()[0].SkillDetails.ToList()[1].Date);
		}

		[Test]
		public void ShouldMapRelativeDifference()
		{
			var skillDay = MockRepository.GenerateMock<ISkillDay>();
			var skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill1, new DateTime(2015,9,4,12,0,0,DateTimeKind.Utc), 0, 0);
			skillStaffPeriod.SetCalculatedResource65(5);
			typeof(SkillStaff).GetField("_forecastedIncomingDemand", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(skillStaffPeriod.Payload, 10);
			_skillDays.Add(_skill1, new List<ISkillDay> { skillDay });
			
			skillDay.Stub(x => x.SkillStaffPeriodCollection)
				.Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> { skillStaffPeriod }));
			skillDay.Stub(x => x.CurrentDate).Return(_period.StartDate);

			_target.Map(_skillDays, _period);
			Assert.AreEqual(-0.5, _target.SkillResultList.ToList()[0].SkillDetails.ToList()[0].RelativeDifference);
		}

		[Test]
		public void ShouldMapColorId0IfWithinThresholds()
		{
			var skillDay = MockRepository.GenerateMock<ISkillDay>();
			var skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill1, new DateTime(2015, 9, 4, 12, 0, 0, DateTimeKind.Utc), 0, 0);
			skillStaffPeriod.SetCalculatedResource65(9.9);
			typeof(SkillStaff).GetField("_forecastedIncomingDemand", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(skillStaffPeriod.Payload, 10);
			_skillDays.Add(_skill1, new List<ISkillDay> { skillDay });

			skillDay.Stub(x => x.SkillStaffPeriodCollection)
				.Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> { skillStaffPeriod }));
			skillDay.Stub(x => x.CurrentDate).Return(_period.StartDate);
			skillDay.Stub(x => x.OpenForWork).Return(new OpenForWork(true, true));

			_target.Map(_skillDays, _period);
			Assert.AreEqual(0, _target.SkillResultList.ToList()[0].SkillDetails.ToList()[0].ColorId);
		}

		[Test]
		public void ShouldMapColorId1IfUnderstaffed()
		{
			var skillDay = MockRepository.GenerateMock<ISkillDay>();
			var skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill1, new DateTime(2015, 9, 4, 12, 0, 0, DateTimeKind.Utc), 0, 0);
			skillStaffPeriod.SetCalculatedResource65(8.9);
			typeof(SkillStaff).GetField("_forecastedIncomingDemand", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(skillStaffPeriod.Payload, 10);
			_skillDays.Add(_skill1, new List<ISkillDay> { skillDay });

			skillDay.Stub(x => x.SkillStaffPeriodCollection)
				.Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> { skillStaffPeriod }));
			skillDay.Stub(x => x.CurrentDate).Return(_period.StartDate);
			skillDay.Stub(x => x.OpenForWork).Return(new OpenForWork(true, true));

			_target.Map(_skillDays, _period);
			Assert.AreEqual(1, _target.SkillResultList.ToList()[0].SkillDetails.ToList()[0].ColorId);
		}

		[Test]
		public void ShouldMapColorId2IfCriticalUnderstaff()
		{
			var skillDay = MockRepository.GenerateMock<ISkillDay>();
			var skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill1, new DateTime(2015, 9, 4, 12, 0, 0, DateTimeKind.Utc), 0, 0);
			skillStaffPeriod.SetCalculatedResource65(5);
			typeof(SkillStaff).GetField("_forecastedIncomingDemand", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(skillStaffPeriod.Payload, 10);
			_skillDays.Add(_skill1, new List<ISkillDay> { skillDay });

			skillDay.Stub(x => x.SkillStaffPeriodCollection)
				.Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> { skillStaffPeriod }));
			skillDay.Stub(x => x.CurrentDate).Return(_period.StartDate);
			skillDay.Stub(x => x.OpenForWork).Return(new OpenForWork(true, true));

			_target.Map(_skillDays, _period);
			Assert.AreEqual(2, _target.SkillResultList.ToList()[0].SkillDetails.ToList()[0].ColorId);
		}

		[Test]
		public void ShouldMapColorId3IfOverStaffed()
		{
			var skillDay = MockRepository.GenerateMock<ISkillDay>();
			var skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill1, new DateTime(2015, 9, 4, 12, 0, 0, DateTimeKind.Utc), 0, 0);
			skillStaffPeriod.SetCalculatedResource65(12);
			typeof(SkillStaff).GetField("_forecastedIncomingDemand", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(skillStaffPeriod.Payload, 10);
			_skillDays.Add(_skill1, new List<ISkillDay> { skillDay });

			skillDay.Stub(x => x.SkillStaffPeriodCollection)
				.Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> { skillStaffPeriod }));
			skillDay.Stub(x => x.CurrentDate).Return(_period.StartDate);
			skillDay.Stub(x => x.OpenForWork).Return(new OpenForWork(true, true));

			_target.Map(_skillDays, _period);
			Assert.AreEqual(3, _target.SkillResultList.ToList()[0].SkillDetails.ToList()[0].ColorId);
		}

		[Test]
		public void ShouldNotIncludeMaxSeatSkill()
		{
			_skill1.SkillType.ForecastSource = ForecastSource.MaxSeatSkill;
			_skillDays.Add(_skill1, new List<ISkillDay>());
			_target.Map(_skillDays, _period);
			Assert.AreEqual(0, _target.SkillResultList.Count());
		}

		[Test]
		public void ShouldNotIncludeOutboundCampaignSkill()
		{
			_skill1.SkillType.ForecastSource = ForecastSource.OutboundTelephony;
			_skillDays.Add(_skill1, new List<ISkillDay>());
			_target.Map(_skillDays, _period);
			Assert.AreEqual(0, _target.SkillResultList.Count());
		}

		[Test]
		public void ShouldMapColorId4IfSkillDayIsClosed()
		{
			var skillDay = MockRepository.GenerateMock<ISkillDay>();
			var skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill1, new DateTime(2015, 9, 4, 12, 0, 0, DateTimeKind.Utc), 0, 0);
			skillStaffPeriod.SetCalculatedResource65(12);
			typeof(SkillStaff).GetField("_forecastedIncomingDemand", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(skillStaffPeriod.Payload, 10);
			_skillDays.Add(_skill1, new List<ISkillDay> { skillDay });

			skillDay.Stub(x => x.SkillStaffPeriodCollection)
				.Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> { skillStaffPeriod }));
			skillDay.Stub(x => x.CurrentDate).Return(_period.StartDate);
			skillDay.Stub(x => x.OpenForWork).Return(new OpenForWork(false, false));
			_target.Map(_skillDays, _period);
			Assert.AreEqual(4, _target.SkillResultList.ToList()[0].SkillDetails.ToList()[0].ColorId);
		}
	}
}