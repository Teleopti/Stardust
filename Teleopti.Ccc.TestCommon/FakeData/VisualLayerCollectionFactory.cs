using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.FakeData
{
	public static class VisualLayerCollectionFactory
	{
		private static IVisualLayerFactory factory = new VisualLayerFactory();

		public static IVisualLayerCollection CreateForWorkShift(IPerson person, TimeSpan start, TimeSpan end)
		{
			IList<IVisualLayer> coll = createVisualLayerCollection(start, end);
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
				WorkShift.BaseDate.Add(end))));
			projSvc.Add(factory.CreateShiftSetupLayer(lunchAct, new DateTimePeriod(
						WorkShift.BaseDate.Add(lunch.StartTime),
						WorkShift.BaseDate.Add(lunch.EndTime))));
			return projSvc.CreateProjection();
		}

		public static IVisualLayerCollection CreateForAbsence(IPerson person, TimeSpan start, TimeSpan end)
		{
			IList<IVisualLayer> coll = createVisualLayerCollectionWithAbsence(start, end);
			IVisualLayerCollection retColl = new VisualLayerCollection(person, coll, new ProjectionPayloadMerger());
			return retColl;
		}

		private static IList<IVisualLayer> createVisualLayerCollection(TimeSpan start, TimeSpan end)
		{
			IList<IVisualLayer> coll = new List<IVisualLayer>();
			coll.Add(factory.CreateShiftSetupLayer(new Activity("for test"), new DateTimePeriod(WorkShift.BaseDate.Add(start), WorkShift.BaseDate.Add(end))));
			return coll;
		}

		private static IList<IVisualLayer> createVisualLayerCollectionWithAbsence(TimeSpan start, TimeSpan end)
		{
			IList<IVisualLayer> visualLayers = new List<IVisualLayer>();
			IVisualLayer originalVisualLayer = factory.CreateShiftSetupLayer(new Activity("for test"), new DateTimePeriod(WorkShift.BaseDate.Add(start), WorkShift.BaseDate.Add(end)));
			IAbsence holidayAbsence = AbsenceFactory.CreateAbsence("TestHoliday");
			IVisualLayer absenceVisualLayer = factory.CreateAbsenceSetupLayer(holidayAbsence, originalVisualLayer, originalVisualLayer.Period);
			visualLayers.Add(absenceVisualLayer);
			return visualLayers;
		}
	}
}
