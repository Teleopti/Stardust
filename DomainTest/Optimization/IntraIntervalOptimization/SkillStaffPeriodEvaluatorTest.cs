using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.IntraIntervalOptimization
{
	[TestFixture]
	public class SkillStaffPeriodEvaluatorTest
	{
		private SkillStaffPeriodEvaluator _target;
		private DateTimePeriod _period1;
		private DateTimePeriod _period2;
			
		[SetUp]
		public void SetUp()
		{
			_target = new SkillStaffPeriodEvaluator();
			_period1 = new DateTimePeriod(2014, 1, 1, 8, 2014, 1, 1, 10);
			_period2 = new DateTimePeriod(2014, 1, 1, 15, 2014, 1, 1, 16);
		}

		[Test]
		public void ShouldReturnTrueWhenPeriodIsBetter()
		{
			var skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period1, new Task(), new ServiceAgreement());
			var skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period2, new Task(), new ServiceAgreement());
			var skillStaffPeriod3 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period1, new Task(), new ServiceAgreement());
			var skillStaffPeriod4 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period2, new Task(), new ServiceAgreement());

			skillStaffPeriod1.HasIntraIntervalIssue = true;
			skillStaffPeriod2.HasIntraIntervalIssue = true;
			skillStaffPeriod3.HasIntraIntervalIssue = true;
			skillStaffPeriod4.HasIntraIntervalIssue = true;

			skillStaffPeriod1.IntraIntervalValue = 0.5;
			skillStaffPeriod2.IntraIntervalValue = 0.5;
			skillStaffPeriod3.IntraIntervalValue = 0.9;
			skillStaffPeriod4.IntraIntervalValue = 0.5;

			var listBefore = new List<ISkillStaffPeriod>{skillStaffPeriod1, skillStaffPeriod2};
			var listAfter = new List<ISkillStaffPeriod>{skillStaffPeriod3, skillStaffPeriod4};

			var result = _target.ResultIsBetter(listBefore, listAfter);
			Assert.IsTrue(result);
		}

		[Test]
		public void ShouldReturnTrueWhenPeriodIsWorseAndCheckingForWorse()
		{
			var skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period1, new Task(), new ServiceAgreement());
			var skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period2, new Task(), new ServiceAgreement());
			var skillStaffPeriod3 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period1, new Task(), new ServiceAgreement());
			var skillStaffPeriod4 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period2, new Task(), new ServiceAgreement());

			skillStaffPeriod1.HasIntraIntervalIssue = true;
			skillStaffPeriod2.HasIntraIntervalIssue = true;
			skillStaffPeriod3.HasIntraIntervalIssue = true;
			skillStaffPeriod4.HasIntraIntervalIssue = true;

			skillStaffPeriod1.IntraIntervalValue = 0.5;
			skillStaffPeriod2.IntraIntervalValue = 0.9;
			skillStaffPeriod3.IntraIntervalValue = 0.9;
			skillStaffPeriod4.IntraIntervalValue = 0.5;

			var listBefore = new List<ISkillStaffPeriod> { skillStaffPeriod1, skillStaffPeriod2 };
			var listAfter = new List<ISkillStaffPeriod> { skillStaffPeriod3, skillStaffPeriod4 };

			var result = _target.ResultIsWorse(listBefore, listAfter);
			Assert.IsTrue(result);	
		}

		[Test]
		public void ShouldReturnFalseWhenPeriodIsWorseAndCheckingForBetter()
		{
			var skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period1, new Task(), new ServiceAgreement());
			var skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period2, new Task(), new ServiceAgreement());
			var skillStaffPeriod3 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period1, new Task(), new ServiceAgreement());
			var skillStaffPeriod4 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period2, new Task(), new ServiceAgreement());

			skillStaffPeriod1.HasIntraIntervalIssue = true;
			skillStaffPeriod2.HasIntraIntervalIssue = true;
			skillStaffPeriod3.HasIntraIntervalIssue = true;
			skillStaffPeriod4.HasIntraIntervalIssue = true;

			skillStaffPeriod1.IntraIntervalValue = 0.5;
			skillStaffPeriod2.IntraIntervalValue = 0.9;
			skillStaffPeriod3.IntraIntervalValue = 0.9;
			skillStaffPeriod4.IntraIntervalValue = 0.5;

			var listBefore = new List<ISkillStaffPeriod> { skillStaffPeriod1, skillStaffPeriod2 };
			var listAfter = new List<ISkillStaffPeriod> { skillStaffPeriod3, skillStaffPeriod4 };

			var result = _target.ResultIsBetter(listBefore, listAfter);
			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnFalseWhenPeriodIsEven()
		{
			var skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period1, new Task(), new ServiceAgreement());
			var skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period2, new Task(), new ServiceAgreement());
			var skillStaffPeriod3 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period1, new Task(), new ServiceAgreement());
			var skillStaffPeriod4 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period2, new Task(), new ServiceAgreement());

			skillStaffPeriod1.HasIntraIntervalIssue = true;
			skillStaffPeriod2.HasIntraIntervalIssue = true;
			skillStaffPeriod3.HasIntraIntervalIssue = true;
			skillStaffPeriod4.HasIntraIntervalIssue = true;

			skillStaffPeriod1.IntraIntervalValue = 0.5;
			skillStaffPeriod2.IntraIntervalValue = 0.9;
			skillStaffPeriod3.IntraIntervalValue = 0.5;
			skillStaffPeriod4.IntraIntervalValue = 0.9;

			var listBefore = new List<ISkillStaffPeriod> { skillStaffPeriod1, skillStaffPeriod2 };
			var listAfter = new List<ISkillStaffPeriod> { skillStaffPeriod3, skillStaffPeriod4 };

			var result = _target.ResultIsBetter(listBefore, listAfter);
			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnFalseWhenMoreNewPeriods()
		{
			var skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period1, new Task(), new ServiceAgreement());
			var skillStaffPeriod3 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period1, new Task(), new ServiceAgreement());
			var skillStaffPeriod4 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period2, new Task(), new ServiceAgreement());

			skillStaffPeriod1.HasIntraIntervalIssue = true;
			skillStaffPeriod3.HasIntraIntervalIssue = true;
			skillStaffPeriod4.HasIntraIntervalIssue = true;

			skillStaffPeriod1.IntraIntervalValue = 0.5;
			skillStaffPeriod3.IntraIntervalValue = 0.9;
			skillStaffPeriod4.IntraIntervalValue = 0.9;

			var listBefore = new List<ISkillStaffPeriod> { skillStaffPeriod1 };
			var listAfter = new List<ISkillStaffPeriod> { skillStaffPeriod3, skillStaffPeriod4 };

			var result = _target.ResultIsBetter(listBefore, listAfter);
			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnBetterIfNumberOfIssuesIsLessAndAnyExistingIssueIsNotWorse()
		{
			var period1 = new DateTimePeriod(2014, 1, 1, 8, 2014, 1, 1, 9);
			var skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(period1, new Task(), new ServiceAgreement());
			skillStaffPeriod1.HasIntraIntervalIssue = true;
			skillStaffPeriod1.IntraIntervalValue = 0.773;
			var period2 = new DateTimePeriod(2014, 1, 1, 20, 2014, 1, 1, 21);
			var skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(period2, new Task(), new ServiceAgreement());
			skillStaffPeriod2.HasIntraIntervalIssue = true;
			skillStaffPeriod2.IntraIntervalValue = 0.727;
			var listBefore = new List<ISkillStaffPeriod> {skillStaffPeriod1, skillStaffPeriod2};
			var listAfter = new List<ISkillStaffPeriod> {skillStaffPeriod2 };
			Assert.IsTrue(_target.ResultIsBetter(listBefore, listAfter));
		}	
	}
}
