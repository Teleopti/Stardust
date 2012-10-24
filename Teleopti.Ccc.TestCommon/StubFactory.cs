﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class StubFactory
	{
		public IBusinessUnit BusinessUnitStub(string name)
		{
			var id = Guid.NewGuid();
			var businessUnit = MockRepository.GenerateStub<IBusinessUnit>();
			businessUnit.Stub(x => x.Id).Return(id);
			businessUnit.Name = name;
			return businessUnit;
		}

		public IPerson PersonStub()
		{
			var id = Guid.NewGuid();
			var person = MockRepository.GenerateStub<IPerson>();
			person.Stub(x => x.Id).Return(id);
			return person;
		}

		public IDataSource DataSourceStub(string name)
		{
			var dataSource = MockRepository.GenerateStub<IDataSource>();
			dataSource.Stub(x => x.DataSourceName).Return(name);
			return dataSource;
		}

		public IScheduleDay ScheduleDayStub()
		{
			return ScheduleDayStub(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
		}

		public IScheduleDay ScheduleDayStub(DateTime date)
		{
			return ScheduleDayStub(date, SchedulePartView.None, null, null, null, null);
		}

		public IScheduleDay ScheduleDayStub(DateTime date, IPerson person)
		{
			return ScheduleDayStub(date, person, SchedulePartView.None, null, null, null, null);
		}

		public IScheduleDay ScheduleDayStub(DateTime date, SchedulePartView significantPartToDisplay, IPersonDayOff personDayOff)
		{
			return ScheduleDayStub(date, significantPartToDisplay, personDayOff, null, null, null);
		}

		public IScheduleDay ScheduleDayStub(DateTime date, IPerson person, SchedulePartView significantPartToDisplay, IPersonDayOff personDayOff)
		{
			return ScheduleDayStub(date, person, significantPartToDisplay, personDayOff, null, null, null);
		}

		public IScheduleDay ScheduleDayStub(DateTime date, SchedulePartView significantPartToDisplay, IPersonAssignment personAssignment)
		{
			return ScheduleDayStub(date, significantPartToDisplay, null, personAssignment, null, null);
		}

		public IScheduleDay ScheduleDayStub(DateTime date, IPerson person, SchedulePartView significantPartToDisplay, IPersonAssignment personAssignment)
		{
			return ScheduleDayStub(date, person, significantPartToDisplay, null, personAssignment, null, null);
		}

		public IScheduleDay ScheduleDayStub(DateTime date, SchedulePartView significantPartToDisplay, IPersonAbsence personAbsence)
		{
			return ScheduleDayStub(date, significantPartToDisplay, new[] {personAbsence});
		}

		public IScheduleDay ScheduleDayStub(DateTime date, IPerson person, SchedulePartView significantPartToDisplay, IPersonAbsence personAbsence)
		{
			return ScheduleDayStub(date, person, significantPartToDisplay, null, null, new[] { personAbsence }, null);
		}

		public IScheduleDay ScheduleDayStub(DateTime date, IPerson person, SchedulePartView significantPartToDisplay, IPersonMeeting personMeeting)
		{
			return ScheduleDayStub(date, person, significantPartToDisplay, null, null, null, null, new[] {personMeeting});
		}

		public IScheduleDay ScheduleDayStub(DateTime date, SchedulePartView significantPartToDisplay, IEnumerable<IPersonAbsence> personAbsences)
		{
			return ScheduleDayStub(date, significantPartToDisplay, null, null, personAbsences, null);
		}

		public IScheduleDay ScheduleDayStub(DateTime date, SchedulePartView significantPartToDisplay, IPublicNote publicNote)
		{
			return ScheduleDayStub(date, significantPartToDisplay, null, null, null, publicNote);
		}

		public IScheduleDay ScheduleDayStub(DateTime date, SchedulePartView significantPartToDisplay, IPersonDayOff personDayOff, IPersonAssignment personAssignment, IEnumerable<IPersonAbsence> personAbsences, IPublicNote publicNote)
		{
			return ScheduleDayStub(date, null, significantPartToDisplay, personDayOff, personAssignment, personAbsences, publicNote);
		}

		public IScheduleDay ScheduleDayStub(DateTime date, IPerson person, SchedulePartView significantPartToDisplay, IPersonDayOff personDayOff, IPersonAssignment personAssignment, IEnumerable<IPersonAbsence> personAbsences, IPublicNote publicNote)
		{
			return ScheduleDayStub(date, person, significantPartToDisplay, personDayOff, personAssignment, personAbsences,
			                       publicNote, null);
		}

		public IScheduleDay ScheduleDayStub(DateTime date, IPerson person, SchedulePartView significantPartToDisplay, IPersonDayOff personDayOff, IPersonAssignment personAssignment, IEnumerable<IPersonAbsence> personAbsences, IPublicNote publicNote, IEnumerable<IPersonMeeting> meetings)
		{
			var timeZone = CccTimeZoneInfoFactory.StockholmTimeZoneInfo();
			var dateOnlyAsPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(date), timeZone);
			var scheduleDay = MockRepository.GenerateStub<IScheduleDay>();
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(dateOnlyAsPeriod);
			scheduleDay.Stub(x => x.SignificantPartForDisplay()).Return(significantPartToDisplay);
			scheduleDay.Stub(x => x.SignificantPart()).Return(significantPartToDisplay);
			scheduleDay.Stub(x => x.TimeZone).Return(timeZone);
			//scheduleDay.Stub(x => x.RestrictionCollection()).Return(new IRestrictionBase[] {});
			if (person != null)
				scheduleDay.Stub(x => x.Person).Return(person);
			if (publicNote != null)
			{
				var publicNotes = new ReadOnlyCollection<IPublicNote>(new List<IPublicNote>(new[] {publicNote}));
				scheduleDay.Stub(x => x.PublicNoteCollection()).Return(publicNotes);
			}
			else
			{
				var publicNotes = new ReadOnlyCollection<IPublicNote>(new List<IPublicNote>());
				scheduleDay.Stub(x => x.PublicNoteCollection()).Return(publicNotes);
			}
			if (personDayOff != null)
			{
				var personDayOffs = new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> {personDayOff});
				scheduleDay.Stub(x => x.PersonDayOffCollection()).Return(personDayOffs);
			}
			if (personAssignment != null)
			{
				scheduleDay.Stub(x => scheduleDay.AssignmentHighZOrder()).Return(personAssignment);
			}
			if (personAbsences != null)
			{
				var personAbsencesCollection = new ReadOnlyCollection<IPersonAbsence>(new List<IPersonAbsence>(personAbsences));
				scheduleDay.Stub(x => x.PersonAbsenceCollection()).Return(personAbsencesCollection);
			}
			if (meetings != null)
			{
				var personMeetingCollection = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>(meetings));
				scheduleDay.Stub(x => x.PersonMeetingCollection()).Return(personMeetingCollection);
			}
			return scheduleDay;
		}

		public IPersonAbsence PersonAbsenceStub() { return PersonAbsenceStub(new DateTimePeriod()); }

		public IPersonAbsence PersonAbsenceStub(string name) { return PersonAbsenceStub(new DateTimePeriod(), name); }

		public IPersonAbsence PersonAbsenceStub(DateTimePeriod period) { return PersonAbsenceStub(period, AbsenceLayerStub()); }

		public IPersonAbsence PersonAbsenceStub(DateTimePeriod period, string name) { return PersonAbsenceStub(period, AbsenceLayerStub(name)); }

		public IPersonAbsence PersonAbsenceStub(DateTimePeriod period, IAbsenceLayer absenceLayer)
		{
			var id = Guid.NewGuid();
			var personAbsence = MockRepository.GenerateStub<IPersonAbsence>();
			personAbsence.Stub(x => x.Id).Return(id);
			personAbsence.Stub(x => x.Layer).Return(absenceLayer);
			personAbsence.Stub(x => x.Period).Return(period);
			return personAbsence;
		}

		public IAbsenceLayer AbsenceLayerStub() { return AbsenceLayerStub(AbsenceStub()); }

		public IAbsenceLayer AbsenceLayerStub(string name) { return AbsenceLayerStub(AbsenceStub(name)); }

		public IAbsenceLayer AbsenceLayerStub(IAbsence absence)
		{
			var absenceLayer = MockRepository.GenerateStub<IAbsenceLayer>();
			absenceLayer.Payload = absence;
			return absenceLayer;
		}

		public IAbsence AbsenceStub() { return AbsenceStub(Color.Green); }

		public IAbsence AbsenceStub(string name)
		{
			return AbsenceStub(Color.Green, name);
		}

		public IAbsence AbsenceStub(Color color)
		{
			return AbsenceStub(color, Guid.NewGuid().ToString());
		}

		public IAbsence AbsenceStub(Color color, string name)
		{
			var absence = MockRepository.GenerateStub<IAbsence>();
			absence.Description = new Description(name);
			absence.DisplayColor = color;
			absence.Stub(x => x.ConfidentialDisplayColor(Arg<IPerson>.Is.Anything, Arg<DateOnly>.Is.Anything)).Return(Color.Gray);
			return absence;
		}

		public IPersonAssignment PersonAssignmentStub(DateTimePeriod dateTimePeriod) { return PersonAssignmentStub(dateTimePeriod, MainShiftStub()); }

		public IPersonAssignment PersonAssignmentStub(DateTimePeriod dateTimePeriod, IMainShift mainShift)
		{
			var personAssignment = MockRepository.GenerateStub<IPersonAssignment>();
			personAssignment.Stub(x => x.MainShift).Return(mainShift);
			personAssignment.Stub(x => x.Period).Return(dateTimePeriod);
			return personAssignment;
		}

		public IPersonAssignment PersonAssignmentPersonalShiftStub()
		{
			var personAssignment = MockRepository.GenerateStub<IPersonAssignment>();
			var personalShift = MockRepository.GenerateStub<IPersonalShift>();
			personAssignment.Stub(x => x.PersonalShiftCollection).Return(
				new ReadOnlyCollection<IPersonalShift>(new[] {personalShift}));
			return personAssignment;
		}

		public IPersonAssignment PersonAssignmentPersonalShiftStub(IPersonalShiftActivityLayer activity)
		{
			var personAssignment = MockRepository.GenerateStub<IPersonAssignment>();
			var personalShift = MockRepository.GenerateStub<IPersonalShift>();
			var layers = new LayerCollection<IActivity> {activity};
			personalShift.Stub(x => x.LayerCollection).Return(layers);
			personAssignment.Stub(x => x.Person).Return(new Person());
			personAssignment.Stub(x => x.PersonalShiftCollection).Return(
				new ReadOnlyCollection<IPersonalShift>(new[] { personalShift }));
			return personAssignment;
		}

		public IPersonAssignment PersonAssignmentOvertimeStub()
		{
			var personAssignment = MockRepository.GenerateStub<IPersonAssignment>();
			var overtimeShift = MockRepository.GenerateStub<IOvertimeShift>();
			personAssignment.Stub(x => x.OvertimeShiftCollection).Return(
				new ReadOnlyCollection<IOvertimeShift>(new[] { overtimeShift }));
			return personAssignment;
		}

		public IMainShift MainShiftStub() { return MainShiftStub(ShiftCategoryStub()); }

		public IMainShift MainShiftStub(IShiftCategory shiftCategory)
		{
			var mainShift = MockRepository.GenerateStub<IMainShift>();
			mainShift.ShiftCategory = shiftCategory;
			return mainShift;
		}

		public IShiftCategory ShiftCategoryStub() { return ShiftCategoryStub(Color.Black); }

		public IShiftCategory ShiftCategoryStub(Color color)
		{
			var id = Guid.NewGuid();
			var shiftCategory = MockRepository.GenerateStub<IShiftCategory>();
			shiftCategory.Stub(x => x.Id).Return(id);
			shiftCategory.DisplayColor = color;
			return shiftCategory;
		}

		public IPersonDayOff PersonDayOffStub() { return PersonDayOffStub(new DateTimePeriod()); }

		public IPersonDayOff PersonDayOffStub(DateTimePeriod period) { return PersonDayOffStub(period, null); }

		public IPersonDayOff PersonDayOffStub(DateTimePeriod period, DayOff? dayOff)
		{
			var personDayOff = MockRepository.GenerateStub<IPersonDayOff>();
			personDayOff.Stub(x => x.Period).Return(period);
			if (dayOff != null)
				personDayOff.Stub(x => x.DayOff).Return(dayOff.Value);
			return personDayOff;
		}

		public IVisualLayerCollection ProjectionStub() { return ProjectionStub(new IVisualLayer[] { }); }

		public IVisualLayerCollection ProjectionStub(IEnumerable<IVisualLayer> visualLayerCollection)
		{
			//var projection = MockRepository.GenerateStub<IVisualLayerCollection>();
			//projection.Stub(x => x.ContractTime()).Return(TimeSpan.FromHours(8));
			//return projection;

			// extremely ugly, but I find no better way of stubbing this hybrid collection class
			var visualLayers = new List<IVisualLayer>(visualLayerCollection);
			var projectionMerger = MockRepository.GenerateMock<IProjectionMerger>();
			projectionMerger.Stub(x => x.MergedCollection(visualLayers, null)).IgnoreArguments().Return(visualLayers);
			var projection = new VisualLayerCollection(null, visualLayers, projectionMerger);
			return projection;
		}

		public IVisualLayer VisualLayerStub() { return VisualLayerStub(Color.Transparent); }

		public IVisualLayer VisualLayerStub(Color displayColor)
		{
			var visualLayer = MockRepository.GenerateMock<IVisualLayer>();
			visualLayer.Stub(x => x.DisplayColor()).Return(displayColor);
			visualLayer.Stub(x => x.Period).PropertyBehavior();
			visualLayer.Period = new DateTimePeriod();
			return visualLayer;
		}

		public IVisualLayer VisualLayerStub(string activtyName)
		{
			var visualLayer = MockRepository.GenerateMock<IVisualLayer>();
			visualLayer.Stub(x => x.DisplayDescription()).Return(new Description(activtyName));
			return visualLayer;
		}

		public IVisualLayer VisualLayerStub(DateTimePeriod period)
		{
			var visualLayer = MockRepository.GenerateMock<IVisualLayer>();
			visualLayer.Stub(x => x.Period).Return(period);
			return visualLayer;
		}

		public IPublicNote PublicNoteStub()
		{
			var publicNote = MockRepository.GenerateStub<IPublicNote>();
			publicNote.Stub(x => x.GetScheduleNote(new NoFormatting())).Return("a note");
			return publicNote;
		}
	}
}