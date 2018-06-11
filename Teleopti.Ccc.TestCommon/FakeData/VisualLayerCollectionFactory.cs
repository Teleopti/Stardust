using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public static class VisualLayerCollectionFactory
	{
		private static readonly IVisualLayerFactory factory = new VisualLayerFactory();

		public static IVisualLayerCollection CreateForWorkShift(IPerson person, TimeSpan start, TimeSpan end)
		{
			var coll = createVisualLayerCollection(person, start, end);
			return new VisualLayerCollection(person, coll, new ProjectionPayloadMerger());
		}

		public static IVisualLayerCollection CreateForAbsence(IPerson person, TimeSpan start, TimeSpan end)
		{
			var coll = createVisualLayerCollectionWithAbsence(person, start, end);
			return new VisualLayerCollection(person, coll, new ProjectionPayloadMerger());
		}

		private static IEnumerable<IVisualLayer> createVisualLayerCollection(IPerson person, TimeSpan start, TimeSpan end)
		{
			return new List<IVisualLayer>
			{
				factory.CreateShiftSetupLayer(new Activity("for test"),
					new DateTimePeriod(WorkShift.BaseDate.Add(start), WorkShift.BaseDate.Add(end)), person)
			};
		}

		private static IEnumerable<IVisualLayer> createVisualLayerCollectionWithAbsence(IPerson person, TimeSpan start,
			TimeSpan end)
		{
			var originalVisualLayer = factory.CreateShiftSetupLayer(new Activity("for test"),
				new DateTimePeriod(WorkShift.BaseDate.Add(start), WorkShift.BaseDate.Add(end)), person);
			var holidayAbsence = AbsenceFactory.CreateAbsence("TestHoliday");
			var absenceVisualLayer = factory.CreateAbsenceSetupLayer(holidayAbsence, originalVisualLayer,
				originalVisualLayer.Period, Guid.NewGuid());

			return new List<IVisualLayer> {absenceVisualLayer};
		}
	}
}
