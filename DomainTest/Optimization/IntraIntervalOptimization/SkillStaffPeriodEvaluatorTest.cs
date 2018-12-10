using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization.IntraIntervalOptimization
{
	[TestFixture]
	public class SkillStaffPeriodEvaluatorTest
	{
		private SkillStaffPeriodEvaluator _target;
		private DateTimePeriod _period1;
		private DateTimePeriod _period2;
		private double _limit;
			
		[SetUp]
		public void SetUp()
		{
			_target = new SkillStaffPeriodEvaluator();
			_period1 = new DateTimePeriod(2014, 1, 1, 8, 2014, 1, 1, 10);
			_period2 = new DateTimePeriod(2014, 1, 1, 15, 2014, 1, 1, 16);
			_limit = 0.8;
		}

		[Test]
		public void ShouldReturnTrueWhenPeriodIsBetter()
		{
			var skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period1, new Task(), new ServiceAgreement());
			var skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period2, new Task(), new ServiceAgreement());
			var skillStaffPeriod3 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period1, new Task(), new ServiceAgreement());
			var skillStaffPeriod4 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period2, new Task(), new ServiceAgreement());

			skillStaffPeriod1.IntraIntervalValue = 0.5;
			skillStaffPeriod2.IntraIntervalValue = 0.5;

			skillStaffPeriod3.IntraIntervalValue = 0.6;
			skillStaffPeriod4.IntraIntervalValue = 0.6;

			var listBefore = new List<ISkillStaffPeriod>{skillStaffPeriod1, skillStaffPeriod2};
			var listAfter = new List<ISkillStaffPeriod>{skillStaffPeriod3, skillStaffPeriod4};

			var result = _target.ResultIsBetter(listBefore, listAfter, _limit);
			Assert.IsTrue(result);
		}

		[Test]
		public void ShouldReturnTrueWhenPeriodIsWorse()
		{
			var skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period1, new Task(), new ServiceAgreement());
			var skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period2, new Task(), new ServiceAgreement());
			var skillStaffPeriod3 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period1, new Task(), new ServiceAgreement());
			var skillStaffPeriod4 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period2, new Task(), new ServiceAgreement());

			skillStaffPeriod1.IntraIntervalValue = 0.5;
			skillStaffPeriod2.IntraIntervalValue = 0.5;

			skillStaffPeriod3.IntraIntervalValue = 0.5;
			skillStaffPeriod4.IntraIntervalValue = 0.4;

			var listBefore = new List<ISkillStaffPeriod> { skillStaffPeriod1, skillStaffPeriod2 };
			var listAfter = new List<ISkillStaffPeriod> { skillStaffPeriod3, skillStaffPeriod4 };

			var result = _target.ResultIsWorse(listBefore, listAfter, _limit);
			Assert.IsTrue(result);
		}	
	}
}
