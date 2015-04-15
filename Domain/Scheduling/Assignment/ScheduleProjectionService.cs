using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ScheduleProjectionService : IProjectionService
	{
		private static readonly Lazy<IActivity> _fakeActivity = new Lazy<IActivity>(() => new Activity("Fake activity")
			{
				InWorkTime = true,
				InPaidTime = true,
				InContractTime = true
			});

		private static readonly Lazy<IActivity> _fakeActivityNotInContractTime =
			new Lazy<IActivity>(() => new Activity("Fake not in contract activity")
				{
					InWorkTime = false,
					InPaidTime = false,
					InContractTime = false
				});

		private static readonly TimeSpan fakeLayerStart = TimeSpan.FromHours(8);
		private static readonly VisualLayerFactory _visualLayerFactory = new VisualLayerFactory();

		/// <summary>
		/// I deeply regret that scheduleDay was injected to ctor instead of passed to CreateProjection...
		/// You probably want to use <see cref="IProjectionProvider"/> instead!
		/// </summary>
		/// <param name="scheduleDay"></param>
		/// <param name="projectionMerger"></param>
		public ScheduleProjectionService(IScheduleDay scheduleDay, IProjectionMerger projectionMerger)
		{
			ScheduleDay = scheduleDay;
			ProjectionMerger = projectionMerger;
		}

		public IProjectionMerger ProjectionMerger { get; private set; }
		public IScheduleDay ScheduleDay { get; private set; }

		public IActivity FakeActivity
		{
			get { return _fakeActivity.Value; }
		}

		public IActivity FakeActivityNotInContractTime
		{
			get { return _fakeActivityNotInContractTime.Value; }
		}

		public IVisualLayerCollection CreateProjection()
		{
			var workingColl = new List<IVisualLayer>();
			var assesProjection = createAssignmentLayers();

			var projectionService = projectionServiceWithAssignmentLayers(assesProjection);
			addFakeLayers(assesProjection);
			addMeetingToProjectionService(projectionService, assesProjection);
			addAbsenceToProjectionService(projectionService, assesProjection);

			workingColl.AddRange(((VisualLayerCollection)projectionService.CreateProjection()).UnMergedCollection);
			removeUnusedFakeActivities(workingColl);
			return new VisualLayerCollection(ScheduleDay.Person, workingColl, ProjectionMerger);
		}

		private void addFakeLayers(ICollection<IVisualLayer> projection)
		{
			//rk - perf reason to read this early - gives a chance to jump out early
			//in fakeLayerMightBeAdded
			var personAbsenceOnScheduleDay = ScheduleDay.PersonAbsenceCollection(true);

			if (fakeLayerMightBeAdded(projection, personAbsenceOnScheduleDay))
			{
				var scheduleDate = ScheduleDay.DateOnlyAsPeriod.DateOnly;
				var person = ScheduleDay.Person;
				var personPeriod = person.Period(scheduleDate);
				var periodStartDate = person.SchedulePeriodStartDate(scheduleDate);
				long workLengthTicks = 0;
				var shouldWork = false;
				if (personPeriod != null && periodStartDate.HasValue)
				{
                    if (personPeriod.PersonContract.Contract.WorkTimeSource == WorkTimeSource.FromContract)
                        workLengthTicks = (long)(person.AverageWorkTimeOfDay(scheduleDate).Ticks*personPeriod.PersonContract.PartTimePercentage.Percentage.Value);
                    else
                        workLengthTicks = person.AverageWorkTimeOfDay(scheduleDate).Ticks;
					shouldWork = personPeriod.PersonContract.ContractSchedule.IsWorkday(periodStartDate.Value, scheduleDate) &&
						!ScheduleDay.HasDayOff();
				}
				var fakeLayer = createFakeLayer(workLengthTicks, ScheduleDay.DateOnlyAsPeriod, shouldWork);
				if (personAbsenceOnScheduleDay.Any(abs => abs.Period.Contains(fakeLayer.Period)))
					projection.Add(fakeLayer);
			}
		}

		private static bool fakeLayerMightBeAdded(IEnumerable<IVisualLayer> projection, IEnumerable<IPersonAbsence> personAbsences)
		{
			return projection.IsEmpty() &&
					!personAbsences.IsEmpty();
		}

		private void removeUnusedFakeActivities(IList<IVisualLayer> fakeLayers)
		{
			for (var i = fakeLayers.Count - 1; i >= 0; i--)
			{
				var layer = fakeLayers[i];
				if (layer.Payload.Equals(FakeActivity) || layer.Payload.Equals(FakeActivityNotInContractTime))
					fakeLayers.RemoveAt(i);
			}
		}


		private IList<IVisualLayer> createAssignmentLayers()
		{
			var assesProjection = new List<IVisualLayer>();
			var assignment = ScheduleDay.PersonAssignment();
			if (assignment != null)
			{
				var assLayers = ((VisualLayerCollection)assignment.ProjectionService().CreateProjection()).UnMergedCollection;
				assesProjection.AddRange(assLayers);
			}
			return assesProjection;
		}

		private IVisualLayer createFakeLayer(long length, IDateOnlyAsDateTimePeriod dateAndPeriod, bool shouldWork)
		{
			var act = shouldWork ? FakeActivity : FakeActivityNotInContractTime;

			var start = dateAndPeriod.Period().StartDateTime.Add(fakeLayerStart);
			var end = start.AddTicks(length);
			return _visualLayerFactory.CreateShiftSetupLayer(act, new DateTimePeriod(start, end), ScheduleDay.Person);
		}

		private void addMeetingToProjectionService(VisualLayerProjectionService projectionService, IEnumerable<IVisualLayer> assignmentProjection)
		{
			foreach (var meeting in ScheduleDay.PersonMeetingCollection(true))
			{
				if (meeting.Person.Equals(ScheduleDay.Person))
				{
					createMeetingProjection(meeting, assignmentProjection).ForEach(projectionService.Add);
				}
			}
		}

		private static IEnumerable<IVisualLayer> createMeetingProjection(IPersonMeeting personMeeting, IEnumerable<IVisualLayer> assignmentProjection)
		{
			var retList = new List<IVisualLayer>();
			var layerFactory = _visualLayerFactory;
			foreach (var visualLayer in assignmentProjection)
			{
				var intersectPeriod = visualLayer.Period.Intersection(personMeeting.Period);
				if (intersectPeriod.HasValue)
				{
					retList.Add(layerFactory.CreateMeetingSetupLayer(new MeetingPayload(personMeeting.BelongsToMeeting),
																	visualLayer,
																	intersectPeriod.Value));
				}
			}
			return retList;
		}

		private void addAbsenceToProjectionService(VisualLayerProjectionService projectionService, IEnumerable<IVisualLayer> assignmentProjection)
		{
			foreach (var personAbsence in ScheduleDay.PersonAbsenceCollection(true))
			{
				var layerFactory = _visualLayerFactory; 
				foreach (var visualLayer in assignmentProjection)
				{
					var intersectPeriod = visualLayer.Period.Intersection(personAbsence.Period);
					if (intersectPeriod.HasValue)
					{
						projectionService.Add(layerFactory.CreateAbsenceSetupLayer(personAbsence.Layer.Payload, visualLayer, intersectPeriod.Value));
					}
				}
			}
		}

		private VisualLayerProjectionService projectionServiceWithAssignmentLayers(IEnumerable<IVisualLayer> assignmentLayers)
		{
			var svc = new VisualLayerProjectionService(ScheduleDay.Person);
			assignmentLayers.ForEach(svc.Add);
			return svc;
		}
	}
}
