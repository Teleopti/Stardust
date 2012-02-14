using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer
{
    public class ScheduleDayOffCountTransformer : IEtlTransformer<IScheduleDay>
    {
        private readonly int _intervalsPerDay;
        private DataTable _table;

        private ScheduleDayOffCountTransformer() { }

        public ScheduleDayOffCountTransformer(int intervalsPerDay)
            : this()
        {
            _intervalsPerDay = intervalsPerDay;
        }

        public void Transform(IEnumerable<IScheduleDay> rootList, DataTable table)
        {
            _table = table;

            foreach (DataRow dataRow in CreateDataRows(rootList, _table, _intervalsPerDay))
            {
                _table.Rows.Add(dataRow);
            }
        }

        public static IList<DataRow> CreateDataRows(IEnumerable<IScheduleDay> schedulePartCollection, DataTable dataTable, int intervalsPerDay)
        {
            IList<DataRow> dataRowCollection = new List<DataRow>();

            foreach (IScheduleDay schedulePart in schedulePartCollection)
            {
                if (doDayOffExistInSchedulePart(schedulePart))
                {
                    dataRowCollection.Add(CreateDataRow(schedulePart, dataTable, intervalsPerDay));
                }
            }

            return dataRowCollection;
        }

        public static DataRow CreateDataRow(IScheduleDay schedulePart, DataTable dataTable, int intervalsPerDay)
        {
            if (dataTable == null)
                return null;
            DataRow dataRow = dataTable.NewRow();
            if (schedulePart == null)
                return dataRow;

			IPersonDayOff personDayOff = extractDayOff(schedulePart);

            dataRow["date"] = personDayOff.DayOff.Anchor.Date;
            dataRow["start_interval_id"] = new IntervalBase(personDayOff.DayOff.Anchor, intervalsPerDay).Id;
            dataRow["person_code"] = schedulePart.Person.Id;
            dataRow["scenario_code"] = schedulePart.Scenario.Id;
            dataRow["starttime"] = personDayOff.DayOff.Anchor;
            dataRow["day_off_code"] = DBNull.Value;
            //dataRow["day_off_name"] = "CountDayOff"; //Get from domain
            dataRow["day_off_name"] = personDayOff.DayOff.Description.Name; //Get from domain
            dataRow["day_count"] = 1;
            dataRow["business_unit_code"] = personDayOff.BusinessUnit.Id;
            //dataRow["datasource_id"] = 1;
			dataRow["datasource_update_date"] = RaptorTransformerHelper.GetUpdatedDate(personDayOff);

            return dataRow;
        }

        private static bool doDayOffExistInSchedulePart(IScheduleDay schedulePart)
        {

        	SchedulePartView significant = schedulePart.SignificantPart();
            if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff)
            {
                return true;
            }

            return false;
        }

		private static IPersonDayOff extractDayOff(IScheduleDay scheduleDay)
		{
			if(scheduleDay.SignificantPart() == SchedulePartView.ContractDayOff)
			{
				IDayOffTemplate template = new DayOffTemplate(new Description("ContractDayOff", "CD"));
				template.Anchor = TimeSpan.FromHours(12);
				IPersonDayOff dayOff = new PersonDayOff(scheduleDay.Person, scheduleDay.Scenario, template,
				                                        scheduleDay.DateOnlyAsPeriod.DateOnly);
				RaptorTransformerHelper.SetUpdatedOn(dayOff, new DateTime(2059, 12, 31));
				return dayOff;
			}

			return scheduleDay.PersonDayOffCollection()[0];
		}
    }
}
