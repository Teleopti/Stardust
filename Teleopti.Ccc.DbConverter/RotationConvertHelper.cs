using System.Collections.Generic;
using System.Data;

namespace Teleopti.Ccc.DBConverter
{
    public class RotationConvertHelper :CommonHelper
    {
       
        public RotationConvertHelper(string connectionString)
            : base(connectionString)
        {}

        public IList<DataRow> LoadAllRotations()
        {
            return ReadData("SELECT rotation_id, rotation_name, weeks FROM t_rotations");
        }

        public IList<DataRow> LoadAllRotationDays()
        {
            return ReadData("SELECT r.rotation_id, r.rotation_day, r.start_interval, r.end_interval, r.shift_cat_id, r.abs_id, a.abs_short_desc_nonuni as abs_short_desc FROM t_rotation_days r left outer join absences a on r.abs_id = a.abs_id");
        }

        public IList<DataRow> LoadAllEmployeeRotations()
        {
            return ReadData("SELECT emp_id, date_from, start, rotation_id FROM t_rotation_employees");
        }
    }
}
