using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class PossibleCombinationsOfStartEndCategoryRunnerTest :IDisposable
	{
		private MockRepository _mocks;
		private List<IPossibleStartEndCategory> _options;
		private PossibleCombinationsOfStartEndCategoryRunner _target;
		private IBestGroupValueExtractorThreadFactory _bestGroupValueExtractorThreadFactory;
		private ShiftCategoryPeriodValueExtractorThreadForTest _thread1;
		private ShiftCategoryPeriodValueExtractorThreadForTest _thread2;
		private ShiftCategoryPeriodValueExtractorThreadForTest _thread3;
		private ShiftCategoryPeriodValueExtractorThreadForTest _thread4;
		private ShiftCategoryPeriodValueExtractorThreadForTest _thread5;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			var option1 = new PossibleStartEndCategory();
			var option2 = new PossibleStartEndCategory();
			var option3 = new PossibleStartEndCategory();
			var option4 = new PossibleStartEndCategory();
			var option5 = new PossibleStartEndCategory();
			_options = new List<IPossibleStartEndCategory> {option1, option2, option3, option4, option5};
			_bestGroupValueExtractorThreadFactory = _mocks.StrictMock<IBestGroupValueExtractorThreadFactory>();
			_target = new PossibleCombinationsOfStartEndCategoryRunner(_bestGroupValueExtractorThreadFactory);
	    	_thread1 = new ShiftCategoryPeriodValueExtractorThreadForTest();
	    	_thread2 = new ShiftCategoryPeriodValueExtractorThreadForTest();
			_thread3 = new ShiftCategoryPeriodValueExtractorThreadForTest();
			_thread4 = new ShiftCategoryPeriodValueExtractorThreadForTest();
			_thread5 = new ShiftCategoryPeriodValueExtractorThreadForTest();

		}

		[Test]
		public void ShouldSetValueInAllObjects()
		{
			var dateOnly = new DateOnly(2012,6,14);
			var person = _mocks.DynamicMock<IGroupPerson>();
			var schedulingOptions = new SchedulingOptions { UseGroupScheduling = true, UseGroupSchedulingCommonStart = true };
			var persons = new List<IPerson>();
			var effectiveRestriction = _mocks.DynamicMock<IEffectiveRestriction>();
			
			Expect.Call(_bestGroupValueExtractorThreadFactory.
							GetNewBestGroupValueExtractorThread(new List<IShiftProjectionCache>(),
				                                                                          dateOnly, person,
																						  schedulingOptions, false, null, null, null, persons, effectiveRestriction)).
				Return(_thread1).
				IgnoreArguments();
			Expect.Call(_bestGroupValueExtractorThreadFactory.
							GetNewBestGroupValueExtractorThread(new List<IShiftProjectionCache>(),
																						  dateOnly, person,
																						  schedulingOptions, false, null, null, null, persons, effectiveRestriction)).
				Return(_thread2).
				IgnoreArguments();
			Expect.Call(_bestGroupValueExtractorThreadFactory.
							GetNewBestGroupValueExtractorThread(new List<IShiftProjectionCache>(),
																						  dateOnly, person,
																						  schedulingOptions, false, null, null, null, persons, effectiveRestriction)).
				Return(_thread3).
				IgnoreArguments();
			Expect.Call(_bestGroupValueExtractorThreadFactory.
							GetNewBestGroupValueExtractorThread(new List<IShiftProjectionCache>(),
																						  dateOnly, person,
																						  schedulingOptions, false, null, null, null, persons, effectiveRestriction)).
				Return(_thread4).
				IgnoreArguments();
			Expect.Call(_bestGroupValueExtractorThreadFactory.
							GetNewBestGroupValueExtractorThread(new List<IShiftProjectionCache>(),
																						  dateOnly, person,
																						  schedulingOptions, false, null, null, null, persons, effectiveRestriction)).
				Return(_thread5).
				IgnoreArguments();
			_mocks.ReplayAll();
			_target.RunTheList(_options, new List<IShiftProjectionCache>(), dateOnly, person, schedulingOptions, false, null, null, null, persons, effectiveRestriction);
			foreach (var possibleStartEndCategory in _options)
			{
				Assert.That(possibleStartEndCategory.ShiftValue, Is.EqualTo(100));
			}

			_mocks.VerifyAll();
			
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_thread1.Dispose();
				_thread2.Dispose();
				_thread3.Dispose();
				_thread4.Dispose();
				_thread5.Dispose();
			}
		}
	}

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class ShiftCategoryPeriodValueExtractorThreadForTest : IShiftCategoryPeriodValueExtractorThread ,IDisposable
	{
		private ManualResetEvent _manualResetEvent;

		public ShiftCategoryPeriodValueExtractorThreadForTest()
		{
			_manualResetEvent = new ManualResetEvent(false);
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)")]
        public void ExtractShiftCategoryPeriodValue(object possibleStartEndCategory)
		{
			var possible = possibleStartEndCategory as PossibleStartEndCategory;
			if (possible == null)
			{
				_manualResetEvent.Set();
				return;
			}

			possible.ShiftValue = 100;
			_manualResetEvent.Set();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_manualResetEvent != null)
					_manualResetEvent.Close();
				_manualResetEvent = null;
			}
		}
	}
}