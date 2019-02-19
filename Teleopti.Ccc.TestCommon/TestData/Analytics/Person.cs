using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class Person : IAnalyticsDataSetup, IPersonData
	{
		private readonly int _validToDateIdMaxDate;
		private readonly Guid? _personCode;
		private readonly int _datasourceId;
		private readonly DateTime _validFrom;
		private readonly DateTime _validTo;
		private readonly int _validFromId;
		private readonly int _validToId;
		private readonly int _businessUnitId;
		private readonly Guid? _businessUnitCode;
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
				toBeDeleted, timeZoneId, person.PersonPeriodCollection.FirstOrDefault()?.Id)
		{ }

		public Person(IPerson person, IDatasourceData datasource, int personId, DateTime validFrom, DateTime validTo, int validFromId,
							int validToId, int businessUnitId, Guid businessUnitCode, bool toBeDeleted, int timeZoneId, Guid? personPeriodCode)
			: this(
				personId, person.Id.Value, personPeriodCode, person.Name.FirstName, person.Name.LastName,
				validFrom, validTo, validFromId, validToId, businessUnitId, businessUnitCode, datasource,
				toBeDeleted, timeZoneId)
		{

		}

		public Person(IPerson person, IDatasourceData datasource, int personId, DateTime validFrom, DateTime validTo, int validFromId,
			int validToId, int businessUnitId, Guid businessUnitCode, bool toBeDeleted, int timeZoneId, Guid? personPeriodCode, int validToDateIdMaxDate
		) : this(person, datasource, personId, validFrom, validTo, validFromId, validToId, businessUnitId, businessUnitCode, toBeDeleted, timeZoneId, personPeriodCode)
		{
			_validToDateIdMaxDate = validToDateIdMaxDate;
		}


		public static int FindPersonIdByPersonCode(Guid person_code)
		{
			var personTable = new Person();
			using (var connection = new SqlConnection(InfraTestConfigReader.AnalyticsConnectionString()))
			{
				var culture = Thread.CurrentThread.CurrentCulture;
				connection.Open();
				personTable.List(connection, culture, CultureInfo.GetCultureInfo("sv-SE"));
			}
			var personRow = personTable.Rows.FirstOrDefault(p =>
			{
				var item = p["person_code"];
				if (item == DBNull.Value) return false;

				return (Guid) item == person_code;
			});

			return personRow != null ? (int) personRow["person_id"] : -1;
		}

		private Person()
		{
		}

		public static Person NotDefinedPerson(IDatasourceData datasource)
		{
			return new Person(-1, null, null, "Not Defined", "Not Defined", new DateTime(1900, 1, 1),
				new DateTime(2059, 12, 31), -1, -2, -1, null, datasource, false, -1);
		}
		
		public Person(IPerson person, int datasourceId, int personId, DateTime validFrom, DateTime validTo,
			int validFromId,
			int validToId, int businessUnitId, Guid businessUnitCode, bool toBeDeleted, int timeZoneId)
			: this(
				personId, person.Id.Value, person.PersonPeriodCollection.FirstOrDefault()?.Id, person.Name.FirstName, person.Name.LastName,
				validFrom, validTo, validFromId, validToId, businessUnitId, businessUnitCode, datasourceId,
				toBeDeleted, timeZoneId)
		{ }

		public Person(
			int personId, Guid? personCode, Guid? personPeriodCode, string firstName, string lastName, DateTime validFrom, DateTime validTo, int validFromId,
			int validToId, int businessUnitId, Guid? businessUnitCode, IDatasourceData datasource, bool toBeDeleted, int timeZoneId): 
			this(personId, personCode, personPeriodCode, firstName, lastName, validFrom, validTo, validFromId, validToId, businessUnitId, businessUnitCode, datasource.RaptorDefaultDatasourceId, toBeDeleted, timeZoneId)
		{
		}
		
		public Person(
			int personId, Guid? personCode, Guid? personPeriodCode, string firstName, string lastName, DateTime validFrom, DateTime validTo, int validFromId,
			int validToId, int businessUnitId, Guid? businessUnitCode, int datasourceId, bool toBeDeleted, int timeZoneId)
		{
			PersonId = personId;
			_personCode = personCode;
			_datasourceId = datasourceId;
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
												_datasourceId, _toBeDeleted, _timeZoneId, _personPeriodCode, _validToDateIdMaxDate);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}

		public void List(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_person.CreateTable())
			{
				using (var adapter = new SqlDataAdapter($"select * from {table.TableName}", connection))
				{
					adapter.Fill(table);
				}
				Rows = table.AsEnumerable();
			}
		}
	}
}