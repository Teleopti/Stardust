﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Notification
{
	[TestFixture]
	public class ScheduleDayReadModelCreatorTest
	{
		private MockRepository _mocks;
		private ScheduleDayReadModelCreator _target;
		private IPerson _person;
		private IProjectionService _projService;
		private IVisualLayerCollection _proj;
		private DateTimePeriod? _period;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_person = PersonFactory.CreatePersonWithBasicPermissionInfo("hej", "hej");
			_person.SetId(Guid.NewGuid());
			_projService = _mocks.StrictMock<IProjectionService>();
			_proj = _mocks.StrictMock<IVisualLayerCollection>();
			_target = new ScheduleDayReadModelCreator();
			_period = new DateTimePeriod(2012,8,20,2012,8,27);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldReturnReadModelFromMainShift()
		{
			var sched = _mocks.StrictMock<IScheduleDay>();
			var cat = ShiftCategoryFactory.CreateShiftCategory("Name", "Red");
			var ass = _mocks.StrictMock<IPersonAssignment>();
			var shift = _mocks.StrictMock<IMainShift>();
			Expect.Call(sched.Person).Return(_person);
			Expect.Call(sched.ProjectionService()).Return(_projService);
			Expect.Call(_projService.CreateProjection()).Return(_proj);

			Expect.Call(_proj.ContractTime()).Return(TimeSpan.FromHours(8));
			Expect.Call(_proj.WorkTime()).Return(TimeSpan.FromHours(9));
			Expect.Call(sched.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2012, 8, 29),
			                                                                        _person.PermissionInformation.DefaultTimeZone
			                                                                        	()));
			Expect.Call(_proj.Period()).Return(_period).Repeat.Any();
			
			Expect.Call(sched.SignificantPart()).Return(SchedulePartView.MainShift);
			Expect.Call(sched.AssignmentHighZOrder()).Return(ass);
			Expect.Call(ass.MainShift).Return(shift);
			Expect.Call(shift.ShiftCategory).Return(cat);
			_mocks.ReplayAll();
			var model = _target.TurnScheduleToModel(sched);
			Assert.That(model.StartDateTime, Is.Not.EqualTo(new DateTime()));
			Assert.That(model.Workday, Is.True);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnReadModelFromOvertime()
		{
			var sched = _mocks.StrictMock<IScheduleDay>();
			Expect.Call(sched.Person).Return(_person);
			Expect.Call(sched.ProjectionService()).Return(_projService);
			Expect.Call(_projService.CreateProjection()).Return(_proj);

			Expect.Call(_proj.ContractTime()).Return(TimeSpan.FromHours(8));
			Expect.Call(_proj.WorkTime()).Return(TimeSpan.FromHours(9));
			Expect.Call(sched.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2012, 8, 29),
																					_person.PermissionInformation.DefaultTimeZone
																						()));
			Expect.Call(_proj.Period()).Return(_period).Repeat.Any();

			Expect.Call(sched.SignificantPart()).Return(SchedulePartView.Overtime);
			
			_mocks.ReplayAll();
			var model = _target.TurnScheduleToModel(sched);
			Assert.That(model.StartDateTime, Is.Not.EqualTo(new DateTime()));
			Assert.That(model.Workday, Is.True);

			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldReturnReadModelFromFullDayAbsence()
		{
			var sched = _mocks.StrictMock<IScheduleDay>();
			var layer = _mocks.StrictMock<IAbsenceLayer>();
			var personAbsence = _mocks.StrictMock<IPersonAbsence>();
			var pl = AbsenceFactory.CreateAbsence("borta");

			var absCol = new ReadOnlyCollection<IPersonAbsence>(new List<IPersonAbsence> { personAbsence });
			Expect.Call(sched.Person).Return(_person);
			Expect.Call(sched.ProjectionService()).Return(_projService);
			Expect.Call(_projService.CreateProjection()).Return(_proj);

			Expect.Call(_proj.ContractTime()).Return(TimeSpan.FromHours(8));
			Expect.Call(_proj.WorkTime()).Return(TimeSpan.FromHours(9));
			Expect.Call(sched.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2012, 8, 29),
																					_person.PermissionInformation.DefaultTimeZone
																						()));
			Expect.Call(_proj.Period()).Return(_period).Repeat.Any();

			Expect.Call(sched.SignificantPart()).Return(SchedulePartView.FullDayAbsence);
			Expect.Call(sched.PersonAbsenceCollection()).Return(absCol);
			Expect.Call(personAbsence.Layer).Return(layer);
			Expect.Call(layer.Payload).Return(pl);
			_mocks.ReplayAll();
			var model = _target.TurnScheduleToModel(sched);
			Assert.That(model.StartDateTime, Is.Not.EqualTo(new DateTime()));
			Assert.That(model.Workday, Is.False);

			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldReturnReadModelFromDayOff()
		{
			var sched = _mocks.StrictMock<IScheduleDay>();
			var dagOff = DayOffFactory.CreateDayOffDayOff("dagav");
			var personDayOff = _mocks.StrictMock<IPersonDayOff>();

			var dayOffCol = new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { personDayOff });
			Expect.Call(sched.Person).Return(_person);
			Expect.Call(sched.ProjectionService()).Return(_projService);
			Expect.Call(_projService.CreateProjection()).Return(_proj);

			Expect.Call(_proj.ContractTime()).Return(TimeSpan.FromHours(8));
			Expect.Call(_proj.WorkTime()).Return(TimeSpan.FromHours(9));
			Expect.Call(sched.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2012, 8, 29),
																					_person.PermissionInformation.DefaultTimeZone
																						()));
			Expect.Call(_proj.Period()).Return(_period).Repeat.Any();

			Expect.Call(sched.SignificantPart()).Return(SchedulePartView.DayOff);
			Expect.Call(sched.PersonDayOffCollection()).Return(dayOffCol);
			Expect.Call(personDayOff.DayOff).Return(dagOff);
			_mocks.ReplayAll();
			var model = _target.TurnScheduleToModel(sched);
			Assert.That(model.StartDateTime, Is.Not.EqualTo(new DateTime()));
			Assert.That(model.Workday, Is.False);

			_mocks.VerifyAll();
		}
	}

	
}