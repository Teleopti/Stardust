using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.TestCommon.FakeData
{
	public static class VisualLayerCollectionFactory
	{
		private static readonly IVisualLayerFactory factory = new VisualLayerFactory();

		public static IVisualLayerCollection CreateForWorkShift(TimeSpan start, TimeSpan end)
		{
			var coll = createVisualLayerCollection(start, end);
			return new VisualLayerCollection(coll, new ProjectionPayloadMerger());
		}

		public static IVisualLayerCollection CreateForAbsence(TimeSpan start, TimeSpan end)
		{
			var coll = createVisualLayerCollectionWithAbsence(start, end);
			return new VisualLayerCollection(coll, new ProjectionPayloadMerger());
		}

		private static IEnumerable<IVisualLayer> createVisualLayerCollection(TimeSpan start, TimeSpan end)
		{
			return new List<IVisualLayer>
			{
				factory.CreateShiftSetupLayer(new Activity("for test"),
					new DateTimePeriod(WorkShift.BaseDate.Add(start), WorkShift.BaseDate.Add(end)))
			};
		}

		private static IEnumerable<IVisualLayer> createVisualLayerCollectionWithAbsence(TimeSpan start,
			TimeSpan end)
		{
			var originalVisualLayer = factory.CreateShiftSetupLayer(new Activity("for test"),
				new DateTimePeriod(WorkShift.BaseDate.Add(start), WorkShift.BaseDate.Add(end)));
			var holidayAbsence = AbsenceFactory.CreateAbsence("TestHoliday");
			var absenceVisualLayer = factory.CreateAbsenceSetupLayer(holidayAbsence, originalVisualLayer,
				originalVisualLayer.Period);

			return new List<IVisualLayer> {absenceVisualLayer};
		}
	}
}
