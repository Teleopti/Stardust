using System.Collections.Generic;
using System.Data;

namespace Teleopti.Ccc.DBConverter
{
    public class AbsenceConverterHelper :CommonHelper
    {
        public AbsenceConverterHelper(string connectionString) : base(connectionString)
        {
        }

        public IList<DataRow> ReadConfidentialAbsence()
        {
            return ReadData("select abs_id, private_desc from absences");
        }
    }
}
