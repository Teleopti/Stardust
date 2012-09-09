using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
	[TestFixture]
	public class WorkShiftBackToLegalStateDecisionMakerTest
	{
		private MockRepository _mock;
		private IWorkShiftBackToLegalStateDecisionMaker _target;
		private IScheduleResultDataExtractor _scheduleResultDataExtractor;
		private IWorkShiftLegalStateDayIndexCalculator _workShiftLegalStateDayIndexCalculator;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_scheduleResultDataExtractor = _mock.StrictMock<IScheduleResultDataExtractor>();
			_workShiftLegalStateDayIndexCalculator = new WorkShiftLegalStateDayIndexCalculator();
			_target = new WorkShiftBackToLegalStateDecisionMaker(_scheduleResultDataExtractor, _workShiftLegalStateDayIndexCalculator);
		}

		[Test]
		public void ShouldSuccess()
		{
			ILockableBitArray array = new LockableBitArray(7, false, false, null);
			using (_mock.Record())
			{
				Expect.Call(_scheduleResultDataExtractor.Values()).Return(new List<double?> {0, 1, 0, 0, 1, 0, 1});
			}

			int? result;

			using (_mock.Playback())
			{
				result = _target.Execute(array, false);
			}

			Assert.IsTrue(result.HasValue);
			Assert.AreEqual(1, result.Value);
		}

		[Test]
		public void ShouldSuccessWithRaise()
		{
			ILockableBitArray array = new LockableBitArray(7, false, false, null);
			using (_mock.Record())
			{
				Expect.Call(_scheduleResultDataExtractor.Values()).Return(new List<double?> { 0, 1, 0, 0, 1, 0, 1 });
			}

			int? result;

			using (_mock.Playback())
			{
				result = _target.Execute(array, true);
			}

			Assert.IsTrue(result.HasValue);
			Assert.AreEqual(0, result.Value);
		}
	}
}