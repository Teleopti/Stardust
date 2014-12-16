using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class Person : IAnalyticsDataSetup, IPersonData
	{
		private readonly Guid _personCode;
		private readonly IDatasourceData _datasource;
		private readonly DateTime _validFrom;
		private readonly DateTime _validTo;
		private readonly int _validFromId;
		private readonly int _validToId;
		private readonly int _businessUnitId;
		private readonly Guid _businessUnitCode;
		private readonly bool _toBeDeleted;
		private readonly int _timeZoneId;
		private readonly Guid? _personPeriodCode;
		private readonly string _firstName;
		private readonly string _lastName;

		public int PersonId { get; set; }
		public IEnumerable<DataRow> Rows { get; set; }

		public Person(IPerson person, IDatasourceData datasource, int personId, DateTime validFrom, DateTime validTo,
			int validFromId,
			int validToId, int businessUnitId, Guid businessUnitCode, bool toBeDeleted, int timeZoneId)
			: this(
				person, datasource, personId, validFrom, validTo, validFromId, validToId, businessUnitId, businessUnitCode,
				toBeDeleted, timeZoneId, null)
		{ }

		public Person(IPerson person, IDatasourceData datasource, int personId, DateTime validFrom, DateTime validTo, int validFromId,
							int validToId, int businessUnitId, Guid businessUnitCode, bool toBeDeleted, int timeZoneId, Guid? personPeriodCode)
			: this(
				personId, person.Id.Value, personPeriodCode, person.Name.FirstName, person.Name.LastName,
				validFrom, validTo, validFromId, validToId, businessUnitId, businessUnitCode, datasource,
				toBeDeleted, timeZoneId)
		{

		}

		public Person(
			int personId, Guid personCode, Guid? personPeriodCode, string firstName, string lastName, DateTime validFrom, DateTime validTo, int validFromId,
			int validToId, int businessUnitId, Guid businessUnitCode, IDatasourceData datasource, bool toBeDeleted, int timeZoneId)
		{
			PersonId = personId;
			_personCode = personCode;
			_datasource = datasource;
			_validFrom = validFrom;
			_validTo = validTo;
			_validFromId = validFromId;
			_validToId = validToId;
			_businessUnitId = businessUnitId;
			_businessUnitCode = businessUnitCode;
			_toBeDeleted = toBeDeleted;
			_timeZoneId = timeZoneId;
			_personPeriodCode = personPeriodCode;
			_firstName = firstName;
			_lastName = lastName;

		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_person.CreateTable())
			{
				table.AddPerson(PersonId, _personCode, _firstName, _lastName, _validFrom, _validTo,
												_validFromId, 0, _validToId, 0, _businessUnitId, _businessUnitCode, "",
												_datasource.RaptorDefaultDatasourceId, _toBeDeleted, _timeZoneId, _personPeriodCode);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}
	}
}