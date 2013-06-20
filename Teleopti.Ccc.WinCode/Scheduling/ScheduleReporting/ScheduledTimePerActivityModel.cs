using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting
{
    public class ScheduledTimePerActivityModel : IReportData
    {
        public string ActivityName { get; set; }
        public double ScheduledTime { get; set; }
        public DateTime ScheduledDate { get; set; }

        public static IList<IReportData> GetReportDataFromScheduleDictionary(IScheduleDictionary scheduleDictionary, IList<IPerson> persons, IList<DateOnly> dates, IList<IPayload> payloads)
        {
            var list = new List<IReportData>();
            foreach (IPerson person in persons)
            {
                var personSchedule = scheduleDictionary[person];
                foreach (var dateOnly in dates)
                {
                    var schedulePart = personSchedule.ScheduledDay(dateOnly, true);
                    list.AddRange(GetReportDataFromSchedulePart(schedulePart, payloads));
                }
            }
			return list.OrderBy(o1 => o1.ActivityName).ThenBy(o2 => o2.ScheduledDate).ToList();
        }

        public static IList<IReportData> GetReportDataFromScheduleParts(IList<IScheduleDay> scheduleParts)
        {
            var list = new List<IReportData>();
            foreach (var schedulePart in scheduleParts)
            {
                list.AddRange(GetReportDataFromSchedulePart(schedulePart, null));
            }
			return list.OrderBy(o1 => o1.ActivityName).ThenBy(o2 => o2.ScheduledDate).ToList();
        }

        public static IList<IReportData> GetReportDataFromSchedulePart(IScheduleDay schedulePart, IList<IPayload> payloads)
        {
            var list = new List<IReportData>();

			//IVirtualSchedulePeriod virtualSchedulePeriod = schedulePart.Person.VirtualSchedulePeriod(schedulePart.DateOnlyAsPeriod.DateOnly);
			//if (!virtualSchedulePeriod.IsValid)
			//	return list;

            var visualLayerCollection = schedulePart.ProjectionService().CreateProjection();
            foreach (IVisualLayer layer in visualLayerCollection)
            {
                // if we have a list of activities we look in that to see that the layer has one off those
                if (payloads != null && !payloads.Contains(layer.Payload.UnderlyingPayload))
                    continue;
                var activity = layer.Payload as IActivity;
				
				var meetingActivity = layer.Payload as IMeetingPayload;

				if (activity != null || meetingActivity != null)
                {
                    DateOnly dateOnly = schedulePart.DateOnlyAsPeriod.DateOnly;
                    list.Add(new ScheduledTimePerActivityModel { ActivityName = layer.DisplayDescription().Name, ScheduledTime = layer.Period.ElapsedTime().TotalMinutes, ScheduledDate = dateOnly }); 
                }
            }
            return list;

        }

		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		//public static IList<IReportData> GetDummyData()
		//{
		//	var list = new List<IReportData>();
		//	list.Add(new ScheduledTimePerActivityModel{ActivityName = "Aktivitet 1", ScheduledTime = 2000,ScheduledDate = new DateTime(2010,2,2)});
		//	list.Add(new ScheduledTimePerActivityModel { ActivityName = "Aktivitet 2", ScheduledTime = 3000, ScheduledDate = new DateTime(2010, 2, 2) });
		//	list.Add(new ScheduledTimePerActivityModel { ActivityName = "Aktivitet 3", ScheduledTime = 3500, ScheduledDate = new DateTime(2010, 2, 2) });

		//	list.Add(new ScheduledTimePerActivityModel { ActivityName = "Aktivitet 1", ScheduledTime = 1000, ScheduledDate = new DateTime(2010, 2, 3) });
		//	list.Add(new ScheduledTimePerActivityModel { ActivityName = "Aktivitet 2", ScheduledTime = 2500, ScheduledDate = new DateTime(2010, 2, 3) });
		//	list.Add(new ScheduledTimePerActivityModel { ActivityName = "Aktivitet 4", ScheduledTime = 3500, ScheduledDate = new DateTime(2010, 2, 3) });

		//	list.Add(new ScheduledTimePerActivityModel { ActivityName = "Aktivitet 1", ScheduledTime = 500, ScheduledDate = new DateTime(2010, 2, 3) });
		//	list.Add(new ScheduledTimePerActivityModel { ActivityName = "Aktivitet 2", ScheduledTime = 500, ScheduledDate = new DateTime(2010, 2, 3) });
		//	list.Add(new ScheduledTimePerActivityModel { ActivityName = "Aktivitet 4", ScheduledTime = 500, ScheduledDate = new DateTime(2010, 2, 3) });

		//	return list;
		//}
    }
}
