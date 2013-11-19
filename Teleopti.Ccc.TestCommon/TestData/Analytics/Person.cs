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
        private readonly IPerson _person;
		private readonly IDatasourceData _datasource;
	    private readonly DateTime _validFrom;
	    private readonly DateTime _validTo;
	    private readonly int _validFromId;
	    private readonly int _validToId;
	    private readonly int _businessUnitId;
	    private readonly Guid _businessUnitCode;
	    private readonly bool _toBeDeleted;
		private readonly int _validToDateIdMax;
		private readonly int _validToIntervalIdMax;
		private readonly DateTime _validFromLocal;
		private readonly DateTime _validToLocal;
		private readonly int _validFromIdLocal;
		private readonly int _validToIdLocal;

		public int PersonId { get; set; }
		public IEnumerable<DataRow> Rows { get; set; }

        public Person(IPerson person, IDatasourceData datasource, int personId, DateTime validFrom, DateTime validTo, int validFromId,
							int validToId, int businessUnitId, Guid businessUnitCode, bool toBeDeleted)
        {
            _person = person;
			_datasource = datasource;
            _validFrom = validFrom;
            _validTo = validTo;
            _validFromId = validFromId;
            _validToId = validToId;
            _businessUnitId = businessUnitId;
            _businessUnitCode = businessUnitCode;
            _toBeDeleted = toBeDeleted;
			_validToDateIdMax = validToId;
			_validToIntervalIdMax = 0;
			_validFromLocal = validFrom;
			_validToLocal = validTo;
			_validFromIdLocal = validFromId;
			_validToIdLocal = validToId;
            PersonId = personId;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_person.CreateTable())
			{
			    table.AddPerson(PersonId, _person.Id.Value, _person.Name.FirstName, _person.Name.LastName, _validFrom, _validTo,
			                    _validFromId, 0, _validToId, 0, _businessUnitId, _businessUnitCode, "",
			                    _datasource.RaptorDefaultDatasourceId, _toBeDeleted);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}

	}
}