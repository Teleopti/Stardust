using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ScheduleProjectionService : IProjectionService
	{
		private IActivity _fakeActivity;
		private IActivity _fakeActivityNotInContractTime;
		private readonly TimeSpan fakeLayerStart = new TimeSpan(8, 0, 0);


		//rk - I deeply regret that scheduleDay was injected to ctor instead of passed to CreateProjection...
		//Remove scheduleDay param from here!
		public ScheduleProjectionService(IScheduleDay scheduleDay, IProjectionMerger projectionMerger)
		{
			ScheduleDay = scheduleDay;
			ProjectionMerger = projectionMerger;
		}

		public IProjectionMerger ProjectionMerger { get; private set; }
		public IScheduleDay ScheduleDay { get; private set; }

		public IActivity FakeActivity
		{
			get
			{
				if (_fakeActivity == null)
				{
					_fakeActivity = new Activity("Fake activity")
					{
						InWorkTime = true,
						InPaidTime = true,
						InContractTime = true
					};
				}
				return _fakeActivity;
			}
		}

		public IActivity FakeActivityNotInContractTime
		{
			get
			{
				if (_fakeActivityNotInContractTime == null)
				{
					_fakeActivityNotInContractTime = new Activity("Fake not in contract activity")
																	{
																		InWorkTime = false,
																		InPaidTime = false,
																		InContractTime = false
																	};
				}
				return _fakeActivityNotInContractTime;
			}
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
				var personPeriod = ScheduleDay.Person.Period(scheduleDate);
				if (personPeriod != null)
				{
					var workLengthTicks = personPeriod.PersonContract.AverageWorkTimePerDay.Ticks;
					var shouldWork = personPeriod.PersonContract.ContractSchedule.IsWorkday(personPeriod.StartDate, scheduleDate);
					var fakeLayer = createFakeLayer(workLengthTicks, ScheduleDay.DateOnlyAsPeriod, shouldWork);
					if (personAbsenceOnScheduleDay.Any(abs => abs.Period.Contains(fakeLayer.Period)))
						projection.Add(fakeLayer);
				}
			}
		}

		private bool fakeLayerMightBeAdded(IEnumerable<IVisualLayer> projection, IEnumerable<IPersonAbsence> personAbsences)
		{
			return projection.IsEmpty() &&
					!personAbsences.IsEmpty() &&
					ScheduleDay.PersonDayOffCollection().IsEmpty();
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
			foreach (var assignment in ScheduleDay.PersonAssignmentCollection())
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
			return new VisualLayerFactory().CreateShiftSetupLayer(act, new DateTimePeriod(start, end));
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
			var layerFactory = personMeeting.CreateVisualLayerFactory();
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
				var layerFactory = personAbsence.CreateVisualLayerFactory();
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
