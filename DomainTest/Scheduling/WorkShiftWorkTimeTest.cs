using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling
{
	//moved from WorkShiftRuleSetTest

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ShiftWork"), TestFixture]
	public class WorkShiftWorkTimeTest
	{
		private IWorkShiftWorkTime target;
		private IRuleSetProjectionService projectionService;

		[SetUp]
		public void Setup()
		{
			projectionService = MockRepository.GenerateMock<IRuleSetProjectionService>();
			target = new WorkShiftWorkTime(projectionService);
		}

		[Test]
		public void VerifyMinMaxWorkTimeAddingCorrect()
		{
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(ActivityFactory.CreateActivity("sample"),
			                                                                  new TimePeriodWithSegment(10, 0, 12, 0, 60),
			                                                                  new TimePeriodWithSegment(11, 0, 13, 0, 60),
			                                                                  ShiftCategoryFactory.CreateShiftCategory("sample")));
			var restriction = MockRepository.GenerateMock<IEffectiveRestriction>();
			var info1 = new WorkShiftProjection
			{
				ContractTime = TimeSpan.FromHours(7),
				TimePeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(17))
			};
			var info2 = new WorkShiftProjection
			{
				ContractTime = TimeSpan.FromHours(9),
				TimePeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(16))
			};

			projectionService.Expect(proj => proj.ProjectionCollection(ruleSet,null)).Return(new[] {info1, info2}).IgnoreArguments();
			restriction.Expect(r => r.Match(info1)).Return(true);
			restriction.Expect(r => r.Match(info2)).Return(true);
			restriction.Expect(r => r.MayMatchWithShifts()).Return(true);

			var result = target.CalculateMinMax(ruleSet, restriction);
			Assert.AreEqual(TimeSpan.FromHours(7), result.StartTimeLimitation.StartTime);
			Assert.AreEqual(TimeSpan.FromHours(8), result.StartTimeLimitation.EndTime);
			Assert.AreEqual(TimeSpan.FromHours(16), result.EndTimeLimitation.StartTime);
			Assert.AreEqual(TimeSpan.FromHours(17), result.EndTimeLimitation.EndTime);
			Assert.AreEqual(TimeSpan.FromHours(7), result.WorkTimeLimitation.StartTime);
			Assert.AreEqual(TimeSpan.FromHours(9), result.WorkTimeLimitation.EndTime);
		}

		[Test]
		public void VerifyMinMaxWorkTimeDisregardsShiftOutsideRestrictions()
		{
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(ActivityFactory.CreateActivity("sample"),
																									new TimePeriodWithSegment(10, 0, 12, 0, 60),
																									new TimePeriodWithSegment(11, 0, 13, 0, 60),
																									ShiftCategoryFactory.CreateShiftCategory("sample")));
			var restriction = MockRepository.GenerateMock<IEffectiveRestriction>();

			var info1 = new WorkShiftProjection
			{
				ContractTime = TimeSpan.FromHours(7),
				TimePeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(17))
			};
			var info2 = new WorkShiftProjection();

			projectionService.Expect(proj => proj.ProjectionCollection(ruleSet,null)).Return(new[] { info1, info2 }).IgnoreArguments();
			restriction.Expect(r => r.Match(info1)).Return(true);
			restriction.Expect(r => r.Match(info2)).Return(false);
			restriction.Expect(r => r.MayMatchWithShifts()).Return(true);

			var result = target.CalculateMinMax(ruleSet, restriction);
			Assert.AreEqual(TimeSpan.FromHours(8), result.StartTimeLimitation.StartTime);
			Assert.AreEqual(TimeSpan.FromHours(8), result.StartTimeLimitation.EndTime);
			Assert.AreEqual(TimeSpan.FromHours(17), result.EndTimeLimitation.StartTime);
			Assert.AreEqual(TimeSpan.FromHours(17), result.EndTimeLimitation.EndTime);
			Assert.AreEqual(TimeSpan.FromHours(7), result.WorkTimeLimitation.StartTime);
			Assert.AreEqual(TimeSpan.FromHours(7), result.WorkTimeLimitation.EndTime);
		}

		[Test]
		public void NoAvailableRestrictionShouldReturnNull()
		{
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(ActivityFactory.CreateActivity("sample"),
																						new TimePeriodWithSegment(10, 0, 12, 0, 60),
																						new TimePeriodWithSegment(11, 0, 13, 0, 60),
																						ShiftCategoryFactory.CreateShiftCategory("sample")));
			var restriction = MockRepository.GenerateMock<IEffectiveRestriction>();
			restriction.Expect(r => r.NotAvailable).Return(true);
			target.CalculateMinMax(ruleSet, restriction).Should().Be.Null();
		}
	}
}