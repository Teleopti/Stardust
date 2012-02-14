using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest
{
    [TestFixture, Ignore("Ignore ETL fetching of day offs for now.")]
    public class DayOffTransformerTest
    {
        //private DayOffTransformer _target;
        //private IList<IDayOffTemplate> _dayOffList;
        //private DataTable _dataTable;
        //private DateTime _insertDateTime;

        //[SetUp]
        //public void Setup()
        //{
        //    _dayOffList = new List<IDayOffTemplate>();
        //    IDayOffTemplate dayOff1 = new DayOffTemplate(new Description("Day Off 1", "DO1"));
        //    dayOff1.SetId(Guid.NewGuid());
        //    dayOff1.DisplayColor = Color.Gray;
        //    RaptorTransformerHelper.SetCreatedOn(dayOff1, DateTime.Now);
        //    _dayOffList.Add(dayOff1);
        //    _insertDateTime = DateTime.Now;

        //    _target = new DayOffTransformer();
        //    _dataTable = _target.Transform(_dayOffList, _insertDateTime);
        //}

        //[Test]
        //public void VerifyDayOffData()
        //{
        //    IDayOffTemplate dayOff = _dayOffList[0];
        //    DataRow row = _dataTable.Rows[0];

        //    Assert.AreEqual(1, _dataTable.Rows.Count);
        //    Assert.AreEqual(dayOff.Id, row["day_off_code"]);
        //    Assert.AreEqual(dayOff.Description.Name, row["day_off_name"]);
        //    Assert.AreEqual(dayOff.DisplayColor.ToArgb(), row["display_color"]);
        //    Assert.AreEqual(((DayOffTemplate)dayOff).BusinessUnit.Id, row["business_unit_code"]);
        //    Assert.AreEqual(((DayOffTemplate)dayOff).BusinessUnit.Name, row["business_unit_name"]);
        //    Assert.AreEqual(1, row["datasource_id"]);
        //    Assert.AreEqual(_insertDateTime, row["insert_date"]);
        //    Assert.AreEqual(_insertDateTime, row["update_date"]);
        //    Assert.AreEqual(RaptorTransformerHelper.GetUpdatedDate(dayOff), row["datasource_update_date"]);
        //}
    }
}
