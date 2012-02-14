using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer
{
    public class DayOffTransformer
    {
        //private DataTable _dataTable;

        //public DayOffTransformer()
        //{
        //    _dataTable = DayOffInfrastructure.CreateEmptyDataTable();
        //}


        //public DataTable Transform(IList<IDayOffTemplate> dayOffList, DateTime insertDateTime)
        //{
        //    foreach (IDayOffTemplate dayOff in dayOffList)
        //    {
        //        DataRow row = _dataTable.NewRow();

        //        row["day_off_code"] = dayOff.Id;
        //        row["day_off_name"] = dayOff.Description.Name;
        //        row["display_color"] = dayOff.DisplayColor.ToArgb();
        //        row["business_unit_code"] = ((DayOffTemplate)dayOff).BusinessUnit.Id;
        //        row["business_unit_name"] = ((DayOffTemplate)dayOff).BusinessUnit.Name;
        //        row["datasource_id"] = 1;
        //        row["insert_date"] = insertDateTime;
        //        row["update_date"] = insertDateTime;
        //        row["datasource_update_date"] = RaptorTransformerHelper.GetUpdatedDate(dayOff);

        //        _dataTable.Rows.Add(row);
        //    }

        //    return _dataTable;
        //}
    }
}
