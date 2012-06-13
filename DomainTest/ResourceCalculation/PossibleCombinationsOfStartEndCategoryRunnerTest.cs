using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class PossibleCombinationsOfStartEndCategoryRunnerTest
	{
		private MockRepository _mocks;
		private List<PossibleStartEndCategory> _options;
		private PossibleCombinationsOfStartEndCategoryRunner _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			var option1 = new PossibleStartEndCategory();
			var option2 = new PossibleStartEndCategory();
			var option3 = new PossibleStartEndCategory();
			var option4 = new PossibleStartEndCategory();
			var option5 = new PossibleStartEndCategory();
			_options = new List<PossibleStartEndCategory> {option1, option2, option3, option4, option5};
			_target = new PossibleCombinationsOfStartEndCategoryRunner();
		}

		[Test]
		public void ShouldSetValueInAllObjects()
		{
			_target.RunTheList(_options);
			foreach (var possibleStartEndCategory in _options)
			{
				Assert.That(possibleStartEndCategory.ShiftValue , Is.EqualTo(100));
			}
		}
	}

	public class PossibleCombinationsOfStartEndCategoryRunner
	{
		public void RunTheList(IList<PossibleStartEndCategory> possibleStartEndCategories )
		{
			var arrayLimit = possibleStartEndCategories.Count;
			var doneEvents = new ManualResetEvent[arrayLimit];
			var dummyArray = new Dummy[arrayLimit];
			for (var i = 0; i < arrayLimit; i++)
			{
				doneEvents[i] = new ManualResetEvent(false);
				var d = new Dummy(doneEvents[i] );
				dummyArray[i] = d;
				ThreadPool.QueueUserWorkItem(d.Calc, possibleStartEndCategories[i]);

			}
			WaitHandle.WaitAll(doneEvents);
		}

	}

	public class Dummy
	{
		private readonly ManualResetEvent _doneEvent;

		public Dummy(ManualResetEvent doneEvent)
		{
			_doneEvent = doneEvent;
		}

		public void Calc(object possibleStartEndCategory)
		{
			var possible = possibleStartEndCategory as PossibleStartEndCategory;
			if (possible == null)
			{
				_doneEvent.Set();
				return;
			}
            possible.ShiftValue = 100;
			_doneEvent.Set();
		}
	}
}