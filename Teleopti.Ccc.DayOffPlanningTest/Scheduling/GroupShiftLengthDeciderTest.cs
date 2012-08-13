using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest.Scheduling
{
	[TestFixture]
	public class GroupShiftLengthDeciderTest
	{
		private MockRepository _mocks;
		private IShiftLengthDecider _shiftLengthDecider;
		private IScheduleMatrixListCreator _scheduleMatrixListCreator;
		private IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private GroupShiftLengthDecider _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_shiftLengthDecider = _mocks.StrictMock<IShiftLengthDecider>();
			_scheduleMatrixListCreator = _mocks.DynamicMock<IScheduleMatrixListCreator>();
			_workShiftMinMaxCalculator = _mocks.DynamicMock<IWorkShiftMinMaxCalculator>();
			_target = new GroupShiftLengthDecider(_shiftLengthDecider, _scheduleMatrixListCreator, _workShiftMinMaxCalculator);
		}

		[Test]
		public void ShouldGetShiftLengthFromAllPersons()
		{
			var person1 = _mocks.DynamicMock<IPerson>();
			var person2 = _mocks.DynamicMock<IPerson>();
			var persons = new List<IPerson> {person1, person2};
			var options = new SchedulingOptions();
			var dictionary = _mocks.DynamicMock<IScheduleDictionary>();
			var dateOnly = new DateOnly(2012,6,29);
			var range = _mocks.DynamicMock<IScheduleRange>();
			var cache1 = _mocks.DynamicMock<IShiftProjectionCache>();
			var cache2 = _mocks.DynamicMock<IShiftProjectionCache>();
			var dummyList = new List<IShiftProjectionCache>();

			var matrixPro = _mocks.DynamicMock<IScheduleMatrixPro>();
			Expect.Call(dictionary[person1]).Return(range);
			Expect.Call(dictionary[person2]).Return(range);				

			Expect.Call(_scheduleMatrixListCreator.CreateMatrixListFromScheduleParts(new List<IScheduleDay>())).Return(
				new List<IScheduleMatrixPro> { matrixPro }).IgnoreArguments().Repeat.Twice();

			Expect.Call(_shiftLengthDecider.
							FilterList(dummyList, _workShiftMinMaxCalculator, matrixPro, options)).
				Return(new List<IShiftProjectionCache> {cache1});

			Expect.Call(_shiftLengthDecider.
							FilterList(dummyList, _workShiftMinMaxCalculator, matrixPro, options)).
				Return(new List<IShiftProjectionCache> { cache2 });

			_mocks.ReplayAll();
			var ret = _target.FilterList(dummyList, persons, options, dictionary, dateOnly);
			Assert.That(ret.Count, Is.EqualTo(2));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldSkipIfNoMatrix()
		{
			var person1 = _mocks.DynamicMock<IPerson>();
			var persons = new List<IPerson> { person1 };
			var options = new SchedulingOptions();
			var dictionary = _mocks.DynamicMock<IScheduleDictionary>();
			var dateOnly = new DateOnly(2012, 6, 29);
			var range = _mocks.DynamicMock<IScheduleRange>();
			var dummyList = new List<IShiftProjectionCache>();

			Expect.Call(dictionary[person1]).Return(range);

			Expect.Call(_scheduleMatrixListCreator.CreateMatrixListFromScheduleParts(new List<IScheduleDay>())).Return(
				new List<IScheduleMatrixPro>()).IgnoreArguments();

			_mocks.ReplayAll();
			var ret = _target.FilterList(dummyList, persons, options, dictionary, dateOnly);
			Assert.That(ret.Count, Is.EqualTo(0));
			_mocks.VerifyAll();
		}
	}

}