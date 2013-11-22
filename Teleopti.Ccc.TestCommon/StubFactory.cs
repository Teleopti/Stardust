﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class StubFactory
	{
		public IScheduleDay ScheduleDayStub()
		{
			return ScheduleDayStub(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
		}

		public IScheduleDay ScheduleDayStub(DateTimePeriod totalPeriod)
		{
			return ScheduleDayStub(totalPeriod.StartDateTime, new Person(), SchedulePartView.MainShift, null, null, null, null, totalPeriod);
		}

		public IScheduleDay ScheduleDayStub(DateTime date)
		{
			return ScheduleDayStub(date, SchedulePartView.None, null, null, null);
		}

		public IScheduleDay ScheduleDayStub(DateTime date, IPerson person)
		{
			return ScheduleDayStub(date, person, SchedulePartView.None, null, null, null);
		}

		public IScheduleDay ScheduleDayStub(DateTime date, SchedulePartView significantPartToDisplay, IPersonAssignment personAssignment)
		{
			return ScheduleDayStub(date, significantPartToDisplay, personAssignment, null, null);
		}

		public IScheduleDay ScheduleDayStub(DateTime date, IPerson person, SchedulePartView significantPartToDisplay, IPersonAssignment personAssignment)
		{
			return ScheduleDayStub(date, person, significantPartToDisplay, personAssignment, null, null);
		}

		public IScheduleDay ScheduleDayStub(DateTime date, SchedulePartView significantPartToDisplay, IPersonAbsence personAbsence)
		{
			return ScheduleDayStub(date, significantPartToDisplay, new[] {personAbsence});
		}

		public IScheduleDay ScheduleDayStub(DateTime date, IPerson person, SchedulePartView significantPartToDisplay, IPersonAbsence personAbsence)
		{
			return ScheduleDayStub(date, person, significantPartToDisplay, null, new[] { personAbsence }, null);
		}


		public IScheduleDay ScheduleDayStub(DateTime date, SchedulePartView significantPartToDisplay, IEnumerable<IPersonAbsence> personAbsences)
		{
			return ScheduleDayStub(date, significantPartToDisplay, null, personAbsences, null);
		}

		public IScheduleDay ScheduleDayStub(DateTime date, SchedulePartView significantPartToDisplay, IPublicNote publicNote)
		{
			return ScheduleDayStub(date, significantPartToDisplay, null, null, publicNote);
		}

		public IScheduleDay ScheduleDayStub(DateTime date, SchedulePartView significantPartToDisplay, IPersonAssignment personAssignment, IEnumerable<IPersonAbsence> personAbsences, IPublicNote publicNote)
		{
			return ScheduleDayStub(date, new Person(), significantPartToDisplay, personAssignment, personAbsences, publicNote);
		}

		public IScheduleDay ScheduleDayStub(DateTime date, IPerson person, SchedulePartView significantPartToDisplay, IPersonAssignment personAssignment, IEnumerable<IPersonAbsence> personAbsences, IPublicNote publicNote)
		{
			return ScheduleDayStub(date, person, significantPartToDisplay, personAssignment, personAbsences,
								   publicNote, null, null);
		}

		public IScheduleDay ScheduleDayStub(DateTime date, IPerson person, SchedulePartView significantPartToDisplay, IPersonAssignment personAssignment, IEnumerable<IPersonAbsence> personAbsences, IPublicNote publicNote, IEnumerable<IPersonMeeting> meetings, DateTimePeriod? totalPeriod)
		{
			var timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			                       
			var dateOnlyAsPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(date), timeZone);
			var scheduleDay = MockRepository.GenerateStub<IScheduleDay>();
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(dateOnlyAsPeriod);
			scheduleDay.Stub(x => x.SignificantPartForDisplay()).Return(significantPartToDisplay);
			scheduleDay.Stub(x => x.SignificantPart()).Return(significantPartToDisplay);
			scheduleDay.Stub(x => x.TimeZone).Return(timeZone);
			scheduleDay.Stub(x => x.TotalPeriod()).Return(totalPeriod);
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
			if (personAssignment != null)
			{
				scheduleDay.Stub(x => scheduleDay.PersonAssignment()).Return(personAssignment);
				if (personAssignment.DayOff() != null)
				{
					scheduleDay.Stub(x => x.HasDayOff()).Return(true);
				}
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
			var absenceLayer = MockRepository.GenerateMock<IAbsenceLayer>();
			absenceLayer.Expect(x => x.Payload).Return(absence);
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

		public IPersonAssignment PersonAssignmentStub(DateTimePeriod dateTimePeriod) { return PersonAssignmentStub(dateTimePeriod, new ShiftCategory("sdf")); }

		public IPersonAssignment PersonAssignmentStub(DateTimePeriod dateTimePeriod, IShiftCategory shiftCategory)
		{
			var personAssignment = MockRepository.GenerateStub<IPersonAssignment>();
			personAssignment.Stub(x => x.ShiftCategory).Return(shiftCategory);
			personAssignment.Stub(x => x.Period).Return(dateTimePeriod);
			return personAssignment;
		}


		public IPersonAssignment PersonAssignmentPersonalShiftStub()
		{
			var personAssignment = MockRepository.GenerateStub<IPersonAssignment>();
			personAssignment.Stub(x => x.PersonalActivities()).Return(Enumerable.Empty<IPersonalShiftLayer>());
			return personAssignment;
		}


		public IShiftCategory ShiftCategoryStub(Color color)
		{
			var id = Guid.NewGuid();
			var shiftCategory = MockRepository.GenerateStub<IShiftCategory>();
			shiftCategory.Stub(x => x.Id).Return(id);
			shiftCategory.DisplayColor = color;
			return shiftCategory;
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

		public IVisualLayerCollection ProjectionStub(DateTimePeriod period)
		{
			return
				ProjectionStub(new List<IVisualLayer>
					{
						new VisualLayer(new Activity("for test"), period, new Activity("also for test"), new Person())
					});
		}

		public IVisualLayer VisualLayerStub(Color displayColor)
		{
			var visualLayer = MockRepository.GenerateMock<IVisualLayer>();
			visualLayer.Stub(x => x.DisplayColor()).Return(displayColor);
			visualLayer.Stub(x => x.Period).Return(new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
			return visualLayer;
		}

		public IVisualLayer VisualLayerStub(string activtyName)
		{
			var visualLayer = MockRepository.GenerateMock<IVisualLayer>();
			visualLayer.Stub(x => x.DisplayDescription()).Return(new Description(activtyName));
			visualLayer.Stub(x => x.Period).Return(new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
			return visualLayer;
		}

		public IVisualLayer VisualLayerStub(DateTimePeriod period)
		{
			var visualLayer = MockRepository.GenerateMock<IVisualLayer>();
			visualLayer.Stub(x => x.Period).Return(period);
			return visualLayer;
		}

		public IVisualLayer VisualLayerStub(DateTimePeriod period, IPerson person)
		{
			var visualLayer = MockRepository.GenerateMock<IVisualLayer>();
			visualLayer.Stub(x => x.Period).Return(period);
			visualLayer.Stub(x => x.Person).Return(person);
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