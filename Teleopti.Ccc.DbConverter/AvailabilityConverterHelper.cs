using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.DBConverter
{
    public class AvailabilityConverterHelper : CommonHelper
    {
        public AvailabilityConverterHelper(string connectionString)
            : base(connectionString)
        { }

        public IList<DataRow> LoadAllAvailability()
        {
            return ReadData("SELECT core_id, periods, date_from, emp_id FROM t_core_times");
        }

        public IList<DataRow> LoadAllAvailabilityDays()
        {
            return ReadData("SELECT core_id, core_day, time_from, time_to, available FROM t_core_days");
        }

        public IList<DataRow> LoadAllEmployeeAvailability()
        {
            return ReadData("SELECT emp_id, core_id, date_from FROM t_core_times");
        }
    }
}
