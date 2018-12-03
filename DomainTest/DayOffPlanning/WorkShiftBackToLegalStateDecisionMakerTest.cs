using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
	[TestFixture]
	public class WorkShiftBackToLegalStateDecisionMakerTest
	{
		private MockRepository _mock;
		private IWorkShiftBackToLegalStateDecisionMaker _target;
		private IRelativeDailyDifferencesByAllSkillsExtractor _scheduleResultDataExtractor;
		private WorkShiftLegalStateDayIndexCalculator _workShiftLegalStateDayIndexCalculator;
		private DateOnlyPeriod _dateOnlyPeriod;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_scheduleResultDataExtractor = _mock.StrictMock<IRelativeDailyDifferencesByAllSkillsExtractor>();
			_workShiftLegalStateDayIndexCalculator = new WorkShiftLegalStateDayIndexCalculator();
			_target = new WorkShiftBackToLegalStateDecisionMaker(_scheduleResultDataExtractor, _workShiftLegalStateDayIndexCalculator);
			_dateOnlyPeriod = new DateOnlyPeriod();
		}

		[Test]
		public void ShouldSuccess()
		{
			ILockableBitArray array = new LockableBitArray(7, false, false, null);
			using (_mock.Record())
			{
				Expect.Call(_scheduleResultDataExtractor.Values(_dateOnlyPeriod)).Return(new List<double?> {0, 1, 0, 0, 1, 0, 1});
			}

			int? result;

			using (_mock.Playback())
			{
				result = _target.Execute(array, false, _dateOnlyPeriod);
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
				Expect.Call(_scheduleResultDataExtractor.Values(_dateOnlyPeriod)).Return(new List<double?> { 0, 1, 0, 0, 1, 0, 1 });
			}

			int? result;

			using (_mock.Playback())
			{
				result = _target.Execute(array, true, _dateOnlyPeriod);
			}

			Assert.IsTrue(result.HasValue);
			Assert.AreEqual(0, result.Value);
		}
	}
}