using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public static class VisualLayerCollectionFactory
	{
		private static IVisualLayerFactory factory = new VisualLayerFactory();

		public static IVisualLayerCollection CreateForWorkShift(IPerson person, TimeSpan start, TimeSpan end)
		{
			IList<IVisualLayer> coll = createVisualLayerCollection(person, start, end);
			IVisualLayerCollection retColl = new VisualLayerCollection(person, coll, new ProjectionPayloadMerger());
			return retColl;
		}

		public static IVisualLayerCollection CreateForWorkShift(IPerson person, TimeSpan start, TimeSpan end, IActivity activity)
		{
			IList<IVisualLayer> coll = createVisualLayerCollection(person, start, end, activity);
			IVisualLayerCollection retColl = new VisualLayerCollection(person, coll, new ProjectionPayloadMerger());
			return retColl;
		}

		public static IVisualLayerCollection CreateForWorkShift(IPerson person, TimeSpan start, TimeSpan end, TimePeriod lunch)
		{
			VisualLayerProjectionService projSvc = new VisualLayerProjectionService(person);
			IActivity lunchAct = new Activity("lunch");
			lunchAct.InContractTime = false;
			lunchAct.SetId(Guid.NewGuid());
			IActivity testAct = new Activity("for test");
			testAct.SetId(Guid.NewGuid());
			projSvc.Add(factory.CreateShiftSetupLayer(testAct, new DateTimePeriod(WorkShift.BaseDate.Add(start),
				WorkShift.BaseDate.Add(end)),person));
			projSvc.Add(factory.CreateShiftSetupLayer(lunchAct, new DateTimePeriod(
						WorkShift.BaseDate.Add(lunch.StartTime),
						WorkShift.BaseDate.Add(lunch.EndTime)),person));
			return projSvc.CreateProjection();
		}

		public static IVisualLayerCollection CreateForAbsence(IPerson person, TimeSpan start, TimeSpan end)
		{
			IList<IVisualLayer> coll = createVisualLayerCollectionWithAbsence(person, start, end);
			IVisualLayerCollection retColl = new VisualLayerCollection(person, coll, new ProjectionPayloadMerger());
			return retColl;
		}

		private static IList<IVisualLayer> createVisualLayerCollection(IPerson person,TimeSpan start, TimeSpan end)
		{
			IList<IVisualLayer> coll = new List<IVisualLayer>();
			coll.Add(factory.CreateShiftSetupLayer(new Activity("for test"), new DateTimePeriod(WorkShift.BaseDate.Add(start), WorkShift.BaseDate.Add(end)),person));
			return coll;
		}

		private static IList<IVisualLayer> createVisualLayerCollection(IPerson person, TimeSpan start, TimeSpan end, IActivity activity)
		{
			IList<IVisualLayer> coll = new List<IVisualLayer>();
			coll.Add(factory.CreateShiftSetupLayer(activity, new DateTimePeriod(WorkShift.BaseDate.Add(start), WorkShift.BaseDate.Add(end)), person));
			return coll;
		}

		private static IList<IVisualLayer> createVisualLayerCollectionWithAbsence(IPerson person, TimeSpan start, TimeSpan end)
		{
			IList<IVisualLayer> visualLayers = new List<IVisualLayer>();
			var originalVisualLayer = factory.CreateShiftSetupLayer(new Activity("for test"),
				new DateTimePeriod(WorkShift.BaseDate.Add(start), WorkShift.BaseDate.Add(end)), person);
			var holidayAbsence = AbsenceFactory.CreateAbsence("TestHoliday");
			var absenceVisualLayer = factory.CreateAbsenceSetupLayer(holidayAbsence, originalVisualLayer,
				originalVisualLayer.Period, Guid.NewGuid());
			visualLayers.Add(absenceVisualLayer);
			return visualLayers;
		}
	}
}
