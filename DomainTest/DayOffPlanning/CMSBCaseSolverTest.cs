using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Secrets.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "CMSB"), TestFixture]
	public class CMSBCaseSolverTest
	{
		private IDayOffBackToLegalStateSolver _target;
		private ILockableBitArray _bitArray;
		private IDaysOffPreferences _daysOffPreferences;
		private IDayOffDecisionMaker _cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker;
		private IDayOffBackToLegalStateFunctions _functions;
		private MockRepository _mocks;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker = _mocks.StrictMock<IDayOffDecisionMaker>();
			_daysOffPreferences = new DaysOffPreferences();
			_bitArray = new LockableBitArray(7, false, false, null);
			_functions = new DayOffBackToLegalStateFunctions(_bitArray);
			_daysOffPreferences.UseConsecutiveDaysOff = true;
			_target = new CMSBCaseSolver(_bitArray, _functions, _daysOffPreferences, _cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker);
		}

		[Test]
		public void ShouldReportToManyCorrect()
		{
			var result = _target.ResolvableState();
			Assert.AreEqual(MinMaxNumberOfResult.ToMany, result);

			_daysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(0, 9);
			_target = new CMSBCaseSolver(_bitArray, _functions, _daysOffPreferences, _cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker);

			result = _target.ResolvableState();
			Assert.AreEqual(MinMaxNumberOfResult.Ok, result);
		}

		[Test]
		public void ShouldNotReportOkOntoFew()
		{
			var result = _target.SetToFewBackToLegalState();
			Assert.AreNotEqual(MinMaxNumberOfResult.Ok, result);
		}

		[Test]
		public void ShouldReportSolverName()
		{
			Assert.AreEqual("CMSBCaseSolver", _target.ResolverDescriptionKey);
		}

		[Test]
		public void ShouldKickInIfNeeded()
		{
			IList<double?> values = new List<double?>();
			for (int i = 0; i < _bitArray.Count; i++)
			{
				values.Add(0);
			}
			using (_mocks.Record())
			{
				Expect.Call(_cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker.Execute(_bitArray, values)).Return(true);
			}

			bool result;

			using (_mocks.Playback())
			{
				result = _target.SetToManyBackToLegalState();
			}

			Assert.AreEqual(true, result);
		}

		[Test]
		public void ShouldReturnFalseIfOk()
		{
			_daysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(0, 9);
			_target = new CMSBCaseSolver(_bitArray, _functions, _daysOffPreferences, _cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker);
			var result = _target.SetToManyBackToLegalState();
			Assert.IsFalse(result);
		}
	}
}