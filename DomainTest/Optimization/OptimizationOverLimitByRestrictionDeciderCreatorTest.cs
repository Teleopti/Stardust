using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class OptimizationOverLimitByRestrictionDeciderCreatorTest
	{
		private MockRepository _mocks;
		private IScheduleMatrixPro _matrixPro;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_matrixPro = _mocks.DynamicMock<IScheduleMatrixPro>();
		}

		[Test]
		public void ShouldGetSchedulePartAndReturnTheObject()
		{
			var dateOnly = new DateOnly(2013,2,4);
			var scheduleDay = _mocks.DynamicMock<IScheduleDayPro>();
			var schedulePart = _mocks.DynamicMock<IScheduleDay>();
			var optimizationPreferences = new OptimizationPreferences();
			Expect.Call(_matrixPro.GetScheduleDayByKey(dateOnly)).Return(scheduleDay);
			Expect.Call(scheduleDay.DaySchedulePart()).Return(schedulePart);
			_mocks.ReplayAll();
			var creator = new OptimizationOverLimitByRestrictionDeciderCreator();
			Assert.That(creator.GetDecider(dateOnly, _matrixPro,optimizationPreferences),Is.Not.Null);
			_mocks.VerifyAll();
		}
	}

	
}