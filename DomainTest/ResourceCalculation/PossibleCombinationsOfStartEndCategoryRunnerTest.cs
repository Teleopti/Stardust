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
	public class PossibleCombinationsOfStartEndCategoryRunnerTest
	{
		private MockRepository _mocks;
		private List<IPossibleStartEndCategory> _options;
		private PossibleCombinationsOfStartEndCategoryRunner _target;
		private IBestGroupValueExtractorThreadFactory _bestGroupValueExtractorThreadFactory;

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
				Return(new ShiftCategoryPeriodValueExtractorThreadForTest()).
				IgnoreArguments();
			Expect.Call(_bestGroupValueExtractorThreadFactory.
							GetNewBestGroupValueExtractorThread(new List<IShiftProjectionCache>(),
																						  dateOnly, person,
																						  schedulingOptions, false, null, null, null, persons, effectiveRestriction)).
				Return(new ShiftCategoryPeriodValueExtractorThreadForTest()).
				IgnoreArguments();
			Expect.Call(_bestGroupValueExtractorThreadFactory.
							GetNewBestGroupValueExtractorThread(new List<IShiftProjectionCache>(),
																						  dateOnly, person,
																						  schedulingOptions, false, null, null, null, persons, effectiveRestriction)).
				Return(new ShiftCategoryPeriodValueExtractorThreadForTest()).
				IgnoreArguments();
			Expect.Call(_bestGroupValueExtractorThreadFactory.
							GetNewBestGroupValueExtractorThread(new List<IShiftProjectionCache>(),
																						  dateOnly, person,
																						  schedulingOptions, false, null, null, null, persons, effectiveRestriction)).
				Return(new ShiftCategoryPeriodValueExtractorThreadForTest()).
				IgnoreArguments();
			Expect.Call(_bestGroupValueExtractorThreadFactory.
							GetNewBestGroupValueExtractorThread(new List<IShiftProjectionCache>(),
																						  dateOnly, person,
																						  schedulingOptions, false, null, null, null, persons, effectiveRestriction)).
				Return(new ShiftCategoryPeriodValueExtractorThreadForTest()).
				IgnoreArguments();
			_mocks.ReplayAll();
			_target.RunTheList(_options, new List<IShiftProjectionCache>(), dateOnly, person, schedulingOptions, false, null, null, null, persons, effectiveRestriction);
			foreach (var possibleStartEndCategory in _options)
			{
				Assert.That(possibleStartEndCategory.ShiftValue, Is.EqualTo(100));
			}

			_mocks.VerifyAll();
		}
	}

	public class ShiftCategoryPeriodValueExtractorThreadForTest : IShiftCategoryPeriodValueExtractorThread
	{
		private readonly ManualResetEvent _manualResetEvent;

		public ShiftCategoryPeriodValueExtractorThreadForTest()
		{
			_manualResetEvent = new ManualResetEvent(false);
		}

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

			Console.WriteLine("Resetting it manual");
		}

		public ManualResetEvent ManualResetEvent
		{
			get { return _manualResetEvent; }
		}
	}
}