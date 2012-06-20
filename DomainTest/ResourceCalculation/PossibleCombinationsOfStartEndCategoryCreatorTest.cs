using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class PossibleCombinationsOfStartEndCategoryCreatorTest
	{
		private MockRepository _mocks;
		private ISchedulingOptions _schedulingOptions;
		private PossibleCombinationsOfStartEndCategoryCreator _target;
		private ShiftCategory _category;

		private readonly TimeSpan _start1 = TimeSpan.FromHours(8);
		readonly TimeSpan _start2 = TimeSpan.FromHours(7);

		private readonly TimeSpan _end1 = TimeSpan.FromHours(15);
		private readonly TimeSpan _end2 = TimeSpan.FromHours(16);
		readonly TimeSpan _end3 = TimeSpan.FromHours(17);

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulingOptions = new SchedulingOptions {UseGroupScheduling = true, UseGroupSchedulingCommonStart = true};
			_target = new PossibleCombinationsOfStartEndCategoryCreator();
			_category = new ShiftCategory("catt");
		}


		[Test]
		public void ShouldFindAllStartAndEndTimesAndCategory()
		{
			_schedulingOptions = new SchedulingOptions { UseGroupScheduling = true, UseGroupSchedulingCommonEnd = true, UseGroupSchedulingCommonStart = true, UseGroupSchedulingCommonCategory = true};
			
			var poss1 = new PossibleStartEndCategory {StartTime = _start1, EndTime = _end1, ShiftCategory = _category};
			var poss2 = new PossibleStartEndCategory {StartTime = _start1, EndTime = _end1, ShiftCategory = _category};
			var poss3 = new PossibleStartEndCategory {StartTime = _start1, EndTime = _end2, ShiftCategory = _category};
			var poss4 = new PossibleStartEndCategory {StartTime = _start2, EndTime = _end3, ShiftCategory = _category};
			var poss5 = new PossibleStartEndCategory {StartTime = _start2, EndTime = _end3, ShiftCategory = _category};
			var temp = new WorkTimeMinMax
			           	{PossibleStartEndCategories = new List<IPossibleStartEndCategory> {poss1, poss2, poss3, poss4, poss5}};
			_mocks.ReplayAll();
			var result = _target.FindCombinations(temp,_schedulingOptions);
			Assert.That(result.Count, Is.EqualTo(3));
			_mocks.VerifyAll();
		}


	}

	

	
    


}