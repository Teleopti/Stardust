using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class PasteAsOvertimeTest
	{
		private PasteAsOvertime _target;
		private IScheduleDay _source;
		private IScheduleDay _destination;
		private DateOnly _dateOnly;
		private IActivity _activity1;
		private IActivity _activity2;
		private IActivity _activity0;
		private DateTimePeriod _period1;
		private DateTimePeriod _period2;
		private DateTimePeriod _period0;
		private DateTime _start1;
		private DateTime _start2;
		private DateTime _start3;
		private DateTime _end1;
		private DateTime _end2;
		private DateTime _end3;
		private IShiftCategory _shiftCategory;
		private IMultiplicatorDefinitionSet _multiplicatorDefinitionSet;
		private IDisposable auth;


		[SetUp]
		public void Setup()
		{
			auth = CurrentAuthorization.ThreadlyUse(new FullPermission());
			_multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			_activity1 = new Activity("activity1");
			_activity2 = new Activity("activity2");
			_activity0 = new Activity("activity3");
			_dateOnly = new DateOnly(2013, 1 ,1);
			_start1 = new DateTime(2013,1,1,8,0,0,DateTimeKind.Utc);
			_start2 = new DateTime(2013,1,1,10,0,0,DateTimeKind.Utc);
			_start3 = new DateTime(2013, 1, 1, 15, 0, 0, DateTimeKind.Utc);
			_end1 = new DateTime(2013,1,1,17,0,0,DateTimeKind.Utc);
			_end2 = new DateTime(2013,1,1,12,0,0,DateTimeKind.Utc);
			_end3 = new DateTime(2013, 1, 1, 19, 0, 0, DateTimeKind.Utc);
			_period1 = new DateTimePeriod(_start1, _end1);
			_period2 = new DateTimePeriod(_start2, _end2);
			_period0 = new DateTimePeriod(_start3, _end3);
			_shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");
			_source = ScheduleDayFactory.Create(_dateOnly);
			_source.CreateAndAddActivity(_activity1, _period1, _shiftCategory);
			_source.CreateAndAddActivity(_activity2,_period2,_shiftCategory);
			_destination = ScheduleDayFactory.Create(_dateOnly);

			_target = new PasteAsOvertime(_source, _destination, _multiplicatorDefinitionSet);
		}

		[TearDown]
		public void Teardown()
		{
			auth?.Dispose();
		}

		[Test]
		public void ShouldPasteOnEmptyDay()
		{
			_target.Paste();
			var shiftLayers = _destination.PersonAssignment().ShiftLayers as IList<ShiftLayer>;

			Assert.IsNotNull(shiftLayers);
			Assert.AreEqual(2, shiftLayers.Count);
			Assert.AreEqual(_period1, shiftLayers[0].Period);
			Assert.AreEqual(_period2, shiftLayers[1].Period);
			Assert.AreEqual(_activity1, shiftLayers[0].Payload);
			Assert.AreEqual(_activity2, shiftLayers[1].Payload);
			Assert.AreEqual(_multiplicatorDefinitionSet, ((OvertimeShiftLayer)shiftLayers[0]).DefinitionSet);
			Assert.AreEqual(_multiplicatorDefinitionSet, ((OvertimeShiftLayer)shiftLayers[1]).DefinitionSet);
		}

		[Test]
		public void ShoulPasteWhenDayOffOnDestination()
		{
			_destination.PersonAssignment(true).SetDayOff(DayOffFactory.CreateDayOff());
			_target.Paste();
			var shiftLayers = _destination.PersonAssignment().ShiftLayers as IList<ShiftLayer>;

			Assert.IsNotNull(shiftLayers);
			Assert.AreEqual(2, shiftLayers.Count);
			Assert.AreEqual(_period1, shiftLayers[0].Period);
			Assert.AreEqual(_period2, shiftLayers[1].Period);
			Assert.AreEqual(_activity1, shiftLayers[0].Payload);
			Assert.AreEqual(_activity2, shiftLayers[1].Payload);
			Assert.AreEqual(_multiplicatorDefinitionSet, ((OvertimeShiftLayer)shiftLayers[0]).DefinitionSet);
			Assert.AreEqual(_multiplicatorDefinitionSet, ((OvertimeShiftLayer)shiftLayers[1]).DefinitionSet);
			Assert.IsTrue(_destination.HasDayOff());
		}

		[Test]
		public void ShoulPasteWhenShiftOnDestination()
		{
			_destination.CreateAndAddActivity(_activity0,_period0,_shiftCategory);
			_target.Paste();
			var shiftLayers = _destination.PersonAssignment().ShiftLayers as IList<ShiftLayer>;

			Assert.IsNotNull(shiftLayers);
			Assert.AreEqual(3, shiftLayers.Count);
			Assert.AreEqual(_period0, shiftLayers[0].Period);
			Assert.AreEqual(_period1, shiftLayers[1].Period);
			Assert.AreEqual(_period2, shiftLayers[2].Period);
			Assert.AreEqual(_activity0, shiftLayers[0].Payload);
			Assert.AreEqual(_activity1, shiftLayers[1].Payload);
			Assert.AreEqual(_activity2, shiftLayers[2].Payload);
			Assert.AreEqual(typeof(MainShiftLayer), shiftLayers[0].GetType());
			Assert.AreEqual(_multiplicatorDefinitionSet, ((OvertimeShiftLayer)shiftLayers[1]).DefinitionSet);
			Assert.AreEqual(_multiplicatorDefinitionSet, ((OvertimeShiftLayer)shiftLayers[2]).DefinitionSet);
		}

		[Test]
		public void ShoulPasteWhenAbsenceOnDestination()
		{
			IAbsenceLayer absenceLayer = new AbsenceLayer(AbsenceFactory.CreateAbsence("absence"), _period1);
			_destination.CreateAndAddAbsence(absenceLayer);
			_target.Paste();
			var shiftLayers = _destination.PersonAssignment().ShiftLayers as IList<ShiftLayer>;

			Assert.IsNotNull(shiftLayers);
			Assert.AreEqual(2, shiftLayers.Count);
			Assert.AreEqual(_period1, shiftLayers[0].Period);
			Assert.AreEqual(_period2, shiftLayers[1].Period);
			Assert.AreEqual(_activity1, shiftLayers[0].Payload);
			Assert.AreEqual(_activity2, shiftLayers[1].Payload);
			Assert.AreEqual(_multiplicatorDefinitionSet, ((OvertimeShiftLayer)shiftLayers[0]).DefinitionSet);
			Assert.AreEqual(_multiplicatorDefinitionSet, ((OvertimeShiftLayer)shiftLayers[1]).DefinitionSet);
			Assert.AreEqual(1, _destination.PersonAbsenceCollection().Length);
		}

		[Test]
		public void ShouldNotPasteOnNullSource()
		{
			_target = new PasteAsOvertime(null, _destination, _multiplicatorDefinitionSet);
			_target.Paste();
			Assert.IsNull(_destination.PersonAssignment());		
		}

		[Test]
		public void ShouldNotPasteOnNullDestination()
		{
			_target = new PasteAsOvertime(_source, null, _multiplicatorDefinitionSet);
			_target.Paste();
			Assert.IsNull(_destination.PersonAssignment());	

		}

		[Test]
		public void ShouldNotPasteOnNullDefinitionSet()
		{
			_target = new PasteAsOvertime(_source, _destination, null);
			_target.Paste();
			Assert.IsNull(_destination.PersonAssignment());	
		}
	}
}
