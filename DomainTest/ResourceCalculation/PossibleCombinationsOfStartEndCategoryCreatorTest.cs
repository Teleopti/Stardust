using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
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
		private IShiftProjectionCache _cashe1;
		private IShiftProjectionCache _cashe2;
		private IShiftProjectionCache _cashe3;
		private IShiftProjectionCache _cashe4;

		readonly DateTime _start1 = new DateTime(2012,6,8,8,0,0);
		readonly DateTime _start2 = new DateTime(2012,6,8,7,0,0);
		readonly DateTime _start3 = new DateTime(2012,6,8,8,0,0);
		readonly DateTime _start4 = new DateTime(2012,6,8,10,0,0);
		readonly DateTime _start5 = new DateTime(2012,6,8,10,0,0);

		readonly DateTime _end1 = new DateTime(2012, 6, 8, 15, 0, 0);
		readonly DateTime _end2 = new DateTime(2012, 6, 8, 16, 0, 0);
		readonly DateTime _end3 = new DateTime(2012, 6, 8, 17, 0, 0);
		readonly DateTime _end4 = new DateTime(2012, 6, 8, 16, 0, 0);
		readonly DateTime _end5 = new DateTime(2012, 6, 8, 16, 0, 0);

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulingOptions = new SchedulingOptions {UseGroupScheduling = true, UseGroupSchedulingCommonStart = true};
			_target = new PossibleCombinationsOfStartEndCategoryCreator( _schedulingOptions);
			_category = new ShiftCategory("hej");
		}

		[Test]
		public void ShouldFindAllStartTimes()
		{
			var cashes = getCashes();
			Expect.Call(_cashe1.MainShiftStartDateTime).Return(_start1);
			Expect.Call(_cashe2.MainShiftStartDateTime).Return(_start2);
			Expect.Call(_cashe3.MainShiftStartDateTime).Return(_start3);
			Expect.Call(_cashe4.MainShiftStartDateTime).Return(_start4);
			_mocks.ReplayAll();
			var result = _target.FindCombinations(cashes);
			Assert.That(result.Count, Is.EqualTo(3));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldFindAllEndTimes()
		{
			_schedulingOptions = new SchedulingOptions { UseGroupScheduling = true, UseGroupSchedulingCommonEnd = true };
			_target = new PossibleCombinationsOfStartEndCategoryCreator(_schedulingOptions);
			var cashes = getCashes();
			Expect.Call(_cashe1.MainShiftEndDateTime).Return(_end1);
			Expect.Call(_cashe2.MainShiftEndDateTime).Return(_end2);
			Expect.Call(_cashe3.MainShiftEndDateTime).Return(_end3);
			Expect.Call(_cashe4.MainShiftEndDateTime).Return(_end4);
			_mocks.ReplayAll();
			var result = _target.FindCombinations(cashes);
			Assert.That(result.Count, Is.EqualTo(3));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldFindAllStartAndEndTimes()
		{
			_schedulingOptions = new SchedulingOptions { UseGroupScheduling = true, UseGroupSchedulingCommonEnd = true, UseGroupSchedulingCommonStart = true};
			_target = new PossibleCombinationsOfStartEndCategoryCreator(_schedulingOptions);
			var cashes = getCashes();
			var cashe5 = _mocks.DynamicMock<IShiftProjectionCache>();
			cashes.Add(cashe5);
			Expect.Call(_cashe1.MainShiftStartDateTime).Return(_start1);
			Expect.Call(_cashe2.MainShiftStartDateTime).Return(_start2);
			Expect.Call(_cashe3.MainShiftStartDateTime).Return(_start3);
			Expect.Call(_cashe4.MainShiftStartDateTime).Return(_start4);
			Expect.Call(cashe5.MainShiftStartDateTime).Return(_start5);

			Expect.Call(_cashe1.MainShiftEndDateTime).Return(_end1);
			Expect.Call(_cashe2.MainShiftEndDateTime).Return(_end2);
			Expect.Call(_cashe3.MainShiftEndDateTime).Return(_end3);
			Expect.Call(_cashe4.MainShiftEndDateTime).Return(_end4);
			Expect.Call(cashe5.MainShiftEndDateTime).Return(_end5);

			_mocks.ReplayAll();
			var result = _target.FindCombinations(cashes);
			Assert.That(result.Count, Is.EqualTo(4));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldFindAllStartAndEndTimesAndCategory()
		{
			_schedulingOptions = new SchedulingOptions { UseGroupScheduling = true, UseGroupSchedulingCommonEnd = true, UseGroupSchedulingCommonStart = true, UseGroupSchedulingCommonCategory = true};
			_target = new PossibleCombinationsOfStartEndCategoryCreator(_schedulingOptions);
			var cashes = getCashes();
			var cashe5 = _mocks.DynamicMock<IShiftProjectionCache>();
			var mainShift = _mocks.DynamicMock<IMainShift>();
			var mainShiftOther = _mocks.StrictMock<IMainShift>();
			var category = _mocks.StrictMock<IShiftCategory>();
			cashes.Add(cashe5);
			Expect.Call(_cashe1.TheMainShift).Return(mainShift);
			Expect.Call(_cashe2.TheMainShift).Return(mainShift);
			Expect.Call(_cashe3.TheMainShift).Return(mainShift);
			Expect.Call(_cashe4.TheMainShift).Return(mainShift);
			Expect.Call(mainShift.ShiftCategory).Return(_category);

			Expect.Call(cashe5.TheMainShift).Return(mainShiftOther);
			Expect.Call(mainShiftOther.ShiftCategory).Return(category);

			Expect.Call(_cashe1.MainShiftStartDateTime).Return(_start1);
			Expect.Call(_cashe2.MainShiftStartDateTime).Return(_start2);
			Expect.Call(_cashe3.MainShiftStartDateTime).Return(_start3);
			Expect.Call(_cashe4.MainShiftStartDateTime).Return(_start4);
			Expect.Call(cashe5.MainShiftStartDateTime).Return(_start5);

			Expect.Call(_cashe1.MainShiftEndDateTime).Return(_end1);
			Expect.Call(_cashe2.MainShiftEndDateTime).Return(_end2);
			Expect.Call(_cashe3.MainShiftEndDateTime).Return(_end3);
			Expect.Call(_cashe4.MainShiftEndDateTime).Return(_end4);
			Expect.Call(cashe5.MainShiftEndDateTime).Return(_end5);

			_mocks.ReplayAll();
			var result = _target.FindCombinations(cashes);
			Assert.That(result.Count, Is.EqualTo(5));
			_mocks.VerifyAll();
		}

		private IList<IShiftProjectionCache> getCashes()
		{
			_cashe1 = _mocks.DynamicMock<IShiftProjectionCache>();
			_cashe2 = _mocks.DynamicMock<IShiftProjectionCache>();
			_cashe3 = _mocks.DynamicMock<IShiftProjectionCache>();
			_cashe4 = _mocks.DynamicMock<IShiftProjectionCache>();
			return new List<IShiftProjectionCache>{_cashe1, _cashe2, _cashe3, _cashe4};
		}
	}

	public class PossibleCombinationsOfStartEndCategoryCreator
	{
		private readonly ISchedulingOptions _schedulingOptions;

		public PossibleCombinationsOfStartEndCategoryCreator(ISchedulingOptions schedulingOptions)
		{
			_schedulingOptions = schedulingOptions;
		}

		public HashSet<PossibleStartEndCategory> FindCombinations(IList<IShiftProjectionCache> shiftProjectionCaches)
		{
			var ret = new HashSet<PossibleStartEndCategory>();

			foreach (var shiftProjectionCach in shiftProjectionCaches)
			{
				var possible = new PossibleStartEndCategory();
				if(_schedulingOptions.UseGroupSchedulingCommonStart)
					possible.StartTime = shiftProjectionCach.MainShiftStartDateTime;
				if(_schedulingOptions.UseGroupSchedulingCommonEnd)
					possible.EndTime = shiftProjectionCach.MainShiftEndDateTime;
				if(_schedulingOptions.UseGroupSchedulingCommonCategory)
					possible.ShiftCategory = shiftProjectionCach.TheMainShift.ShiftCategory;
				ret.Add(possible);
			}
			return ret;
		}
	}

	public class PossibleStartEndCategory: IEquatable<PossibleStartEndCategory>
	{
		private int? _hashCode;
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public IShiftCategory ShiftCategory { get; set; }
		// holds the best value of this combination
		public double Shiftvalue { get; set; }

		public bool Equals(PossibleStartEndCategory other)
		{
			return GetHashCode().Equals(other.GetHashCode());
		}

		public override int GetHashCode()
		{
			if (!_hashCode.HasValue)
			{
				var catHash = 0;
				if (ShiftCategory != null)
					catHash = ShiftCategory.GetHashCode();
				_hashCode = StartTime.GetHashCode() ^ EndTime.GetHashCode() ^ catHash;
			}
			
			return _hashCode.Value;
		}
		
	}
}