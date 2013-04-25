using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
	[TestFixture]
	public class ScheduleMatrixOriginalStateContainerFactoryTest
	{
		private MockRepository _mocks;
		private IScheduleMatrixOriginalStateContainerFactory _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new ScheduleMatrixOriginalStateContainerFactory();
		}

		[Test]
		public void ShouldReturnScheduleMatrixOriginalStateContainer()
		{
			var scheduleDayEquator = _mocks.StrictMock<IScheduleDayEquator>();
			var scheduleMatrix = _mocks.StrictMock<IScheduleMatrixPro>();
			using (_mocks.Record())
			{
				Expect.Call(scheduleMatrix.EffectivePeriodDays)
				      .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>()));
			}
			using (_mocks.Playback())
			{
				var result = _target.CreateScheduleMatrixOriginalStateContainer(scheduleMatrix, scheduleDayEquator);
				Assert.That(result, Is.TypeOf(typeof(ScheduleMatrixOriginalStateContainer)));
			}
		}
	}
}
