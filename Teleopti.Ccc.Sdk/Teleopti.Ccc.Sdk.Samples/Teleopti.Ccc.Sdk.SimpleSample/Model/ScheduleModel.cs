using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Sdk.SimpleSample.Model
{
    public class ScheduleModel
    {
        public ScheduleModel()
        {
            ScheduleDetails = new List<ScheduleDayDetailModel>();
        }
        public string PersonName { get; set; }
        public Guid PersonId { get; set; }
        public string ShiftCategoryName { get; set; }
        public string DayOffName { get; set; }
        public string DayOffPayrollCode { get; set; }
        public DateTime BelongsToDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan ContractTime { get; set; }
        public bool IsFullDayAbsence { get; set; }
        public IList<ScheduleDayDetailModel> ScheduleDetails { get; private set; }
    }
}