using System;
using System.Data;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.Transformer.ScheduleThreading;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Analytics.Etl.TransformerTest.ScheduleThreading
{
	[TestFixture]
	public class ContractDataRowFactoryTest
	{
		[Test]
		public void ShouldCreateNewRow()
		{
			var guid = Guid.NewGuid();
			var dataTable = new DataTable();
			dataTable.Columns.Add("date");
			dataTable.Columns.Add("person_code");
			dataTable.Columns.Add("interval_id");
			dataTable.Columns.Add("scheduled_contract_time_m");
			dataTable.Columns.Add("business_unit_code");
			dataTable.Columns.Add("business_unit_name");
			dataTable.Columns.Add("datasource_id");
			dataTable.Columns.Add("insert_date");
			dataTable.Columns.Add("update_date");
			var dateTime = new DateTime(2013, 05, 04);
			var person = new Person();
			person.SetId(guid);
			var intervalBase = new IntervalBase(dateTime, 5);
			const int contractTime = 180;
			var businessUnit = new BusinessUnit("Test");
			businessUnit.SetId(guid);

			var result = ContractDataRowFactory.CreateContractDataRow(dataTable, dateTime, person, intervalBase, contractTime,
			                                                          businessUnit, dateTime);

			var dateTimeString = dateTime.ToString();
			result["date"].Should().Be.EqualTo(dateTimeString);
			result["person_code"].Should().Be.EqualTo(person.Id.ToString());
			result["interval_id"].Should().Be.EqualTo(intervalBase.Id.ToString());
			result["scheduled_contract_time_m"].Should().Be.EqualTo(contractTime.ToString());

			result["business_unit_code"].Should().Be.EqualTo(businessUnit.Id.ToString());
			result["business_unit_name"].Should().Be.EqualTo(businessUnit.Name);
			result["datasource_id"].Should().Be.EqualTo(1.ToString());
			result["insert_date"].Should().Be.EqualTo(dateTimeString);
			result["update_date"].Should().Be.EqualTo(dateTimeString);
		}
	}
}
