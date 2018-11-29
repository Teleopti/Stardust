using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.FakeData;

using PersonFactory = Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData.PersonFactory;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
	[TestFixture]
	public class PersonTransformerTest
	{
		private IList<IPerson> _personCollection;
		private DataTable _table;
		private DataTable _acdTable;
		private int _intervalsPerDay;
		private ICommonNameDescriptionSetting _commonNameDescriptionSetting;
		
		[SetUp]
		public void Setup()
		{
			string aliasFormat = string.Format(CultureInfo.InvariantCulture, "{0} - {1} {2}",
														  CommonNameDescriptionSetting.EmployeeNumber,
														  CommonNameDescriptionSetting.FirstName,
														  CommonNameDescriptionSetting.LastName);
			_commonNameDescriptionSetting = new CommonNameDescriptionSetting(aliasFormat);
			_intervalsPerDay = 96;
			_personCollection = PersonFactory.CreatePersonGraphCollection();
			_table = new DataTable();
			_table.Locale = Thread.CurrentThread.CurrentCulture;
			_acdTable = new DataTable();
			_acdTable.Locale = Thread.CurrentThread.CurrentCulture;
			PersonInfrastructure.AddColumnsToDataTable(_table);
			AcdLogOnPersonInfrastructure.AddColumnsToDataTable(_acdTable);
			PersonTransformer.Transform(_personCollection, _intervalsPerDay, new DateOnly(2000, 1, 1), _table, _acdTable,
				_commonNameDescriptionSetting, new List<LogonInfo>());
		}

		[Test]
		public void VerifyAggregateRoot()
		{
			//BusinessUnit
			IList<IPersonPeriod> wrapper = new List<IPersonPeriod>(_personCollection[0].PersonPeriodCollection);
			IBusinessUnit bu = wrapper[1].Team.BusinessUnitExplicit;
			Assert.AreEqual(bu.Id, _table.Rows[2]["business_unit_code"]);
			Assert.AreEqual(bu.Description.Name, _table.Rows[2]["business_unit_name"]);
			//UpdatedOn
			Assert.AreEqual(RaptorTransformerHelper.GetUpdatedDate(_personCollection[0]),
								 _table.Rows[0]["datasource_update_date"]);
		}

		[Test]
		public void VerifyIsAgentAndUser()
		{
			Assert.AreEqual(false, _table.Rows[2]["is_agent"]);
			Assert.AreEqual(false, _table.Rows[2]["is_user"]);
		}

		[Test]
		public void VerifyOrganization()
		{
			// Team
			Assert.AreEqual(_personCollection[0].PersonPeriodCollection.First().Team.Id, _table.Rows[0]["team_code"]);
			Assert.AreEqual(_personCollection[0].PersonPeriodCollection.First().Team.Description.Name,
								 _table.Rows[0]["team_name"]);
			//Site
			Assert.AreEqual(_personCollection[0].PersonPeriodCollection.First().Team.Site.Id, _table.Rows[0]["site_code"]);
			Assert.AreEqual(_personCollection[2].PersonPeriodCollection.First().Team.Site.Description.Name,
								 _table.Rows[2]["site_name"]);
		}

		[Test]
		public void VerifyTeamGetScorecard()
		{
			// Team
			Assert.AreEqual(_personCollection[0].PersonPeriodCollection.First().Team.Scorecard.Id, _table.Rows[0]["scorecard_code"]);
			Assert.AreEqual(DBNull.Value, _table.Rows[2]["scorecard_code"]);
		}

		[Test]
		public void VerifyNoScorecardForDeletedTeam()
		{
			// Team
			var _updatedOnDateTime = DateTime.Now;

			var personPeriod = _personCollection[0].PersonPeriodCollection.First();
			personPeriod.Team = TeamFactory.CreateSimpleTeam();
			personPeriod.Team.SetId(Guid.NewGuid());
			personPeriod.Team.Scorecard = ScorecardFactory.CreateScorecardCollection(_updatedOnDateTime)[0];
			personPeriod.Team.Site = SiteFactory.CreateSimpleSite();
			personPeriod.Team.Site.SetId(Guid.NewGuid());
			((IDeleteTag)personPeriod.Team).SetDeleted();

			_table.Rows.Clear();
			_acdTable.Rows.Clear();
			PersonTransformer.Transform(_personCollection, _intervalsPerDay, new DateOnly(2000, 1, 1), _table, _acdTable,
				_commonNameDescriptionSetting, new List<LogonInfo>());

			Assert.AreEqual(DBNull.Value, _table.Rows[0]["scorecard_code"]);
		}

		[Test]
		public void VerifyNoScorecardForDeletedSite()
		{
			// Team
			var _updatedOnDateTime = DateTime.Now;

			var personPeriod = _personCollection[0].PersonPeriodCollection.First();
			personPeriod.Team = TeamFactory.CreateSimpleTeam();
			personPeriod.Team.SetId(Guid.NewGuid());
			personPeriod.Team.Scorecard = ScorecardFactory.CreateScorecardCollection(_updatedOnDateTime)[0];
			personPeriod.Team.Site = SiteFactory.CreateSimpleSite();
			personPeriod.Team.Site.SetId(Guid.NewGuid());
			((IDeleteTag)personPeriod.Team.Site).SetDeleted();

			_table.Rows.Clear();
			_acdTable.Rows.Clear();
			PersonTransformer.Transform(_personCollection, _intervalsPerDay, new DateOnly(2000, 1, 1), _table, _acdTable,
				_commonNameDescriptionSetting, new List<LogonInfo>());

			Assert.AreEqual(DBNull.Value, _table.Rows[0]["scorecard_code"]);
		}

		[Test]
		public void VerifyPeriods()
		{
			IList<IPersonPeriod> personPeriods0 = new List<IPersonPeriod>(_personCollection[0].PersonPeriodCollection);
			IList<IPersonPeriod> personPeriods2 = new List<IPersonPeriod>(_personCollection[2].PersonPeriodCollection);
			IList<IPersonPeriod> personPeriods5 = new List<IPersonPeriod>(_personCollection[5].PersonPeriodCollection);
			TimeZoneInfo TimeZoneInfo0 = _personCollection[0].PermissionInformation.DefaultTimeZone();
			TimeZoneInfo TimeZoneInfo5 = _personCollection[5].PermissionInformation.DefaultTimeZone();

			DateTime validFromDate0 =
				 TimeZoneInfo0.SafeConvertTimeToUtc(
					  DateTime.SpecifyKind(personPeriods0[1].StartDate.Date, DateTimeKind.Unspecified));
			DateTime validToDate0 =
				 TimeZoneInfo0.SafeConvertTimeToUtc(
					  DateTime.SpecifyKind(personPeriods0[0].EndDate().Date, DateTimeKind.Unspecified));
			validToDate0 = validToDate0.AddDays(1);
			DateTime validToDate3 =
				 TimeZoneInfo5.SafeConvertTimeToUtc(
					  DateTime.SpecifyKind(personPeriods5[1].EndDate().Date, DateTimeKind.Unspecified)); //Eternity date
			DateTime eternityDate = new DateTime(2059, 12, 31);

			Assert.AreEqual(5, _table.Rows.Count);
			Assert.AreEqual(_personCollection[0].Id, _table.Rows[0]["person_code"]);
			Assert.AreEqual(_personCollection[2].Id, _table.Rows[2]["person_code"]);
			Assert.AreEqual(validFromDate0, ((DateTime)_table.Rows[1]["valid_from_date"]));
			Assert.AreEqual(validToDate0, ((DateTime)_table.Rows[0]["valid_to_date"]));

			Assert.AreEqual(new IntervalBase(validFromDate0, _intervalsPerDay).Id,
								 _table.Rows[0]["valid_from_interval_id"]);
			Assert.AreEqual(
				 new IntervalBase(PersonTransformer.GetPeriodIntervalEndDate(validToDate0, _intervalsPerDay),
										_intervalsPerDay).Id, _table.Rows[0]["valid_to_interval_id"]);
			Assert.AreEqual(
				 new IntervalBase(PersonTransformer.GetPeriodIntervalEndDate(validToDate3, _intervalsPerDay),
										_intervalsPerDay).Id, _table.Rows[4]["valid_to_interval_id"]);

			// Test personperiod with no end TerminalDate set. Then it should be set default to 2059-12-31
			Assert.AreEqual(personPeriods2.First().EndDate().Date, eternityDate);
			Assert.AreEqual(personPeriods2.First().EndDate().Date, (DateTime)_table.Rows[2]["valid_to_date"]);

			Assert.AreEqual(personPeriods2.First().EndDate().Date, _table.Rows[2]["valid_to_interval_start"]); // Eternity date 2059-12-31
			Assert.AreEqual(PersonTransformer.GetPeriodIntervalEndDate(validToDate0, _intervalsPerDay),
								 _table.Rows[0]["valid_to_interval_start"]);

			Assert.AreEqual(_personCollection[0].Name.FirstName, _table.Rows[0]["person_first_name"]);
			Assert.AreEqual(_personCollection[0].Name.LastName, _table.Rows[1]["person_last_name"]);
			//Employment start and and dates
			Assert.AreEqual(
				 TimeZoneInfo0.SafeConvertTimeToUtc(
					  DateTime.SpecifyKind(personPeriods0[1].StartDate.Date, DateTimeKind.Unspecified)),
				 (DateTime)_table.Rows[1]["employment_start_date"]);
			Assert.AreEqual(
				 TimeZoneInfo0.SafeConvertTimeToUtc(
					  DateTime.SpecifyKind(personPeriods0[1].EndDate().Date.AddDays(1), DateTimeKind.Unspecified)),
				 (DateTime)_table.Rows[1]["employment_end_date"]);

			//PersonPeriod id test
			Assert.IsTrue(personPeriods0[0].Id.HasValue);
			Assert.IsFalse(personPeriods2.First().Id.HasValue);
			Assert.AreEqual(personPeriods0[0].Id, _table.Rows[0]["person_period_code"]);
			Assert.AreEqual(DBNull.Value, _table.Rows[2]["person_period_code"]);
		}

		[Test]
		public void VerifyTheMatrixInternalData()
		{
			Assert.AreEqual(1, _table.Rows[0]["datasource_id"]);
		}

		[Test]
		public void VerifyAcd()
		{
			IList<IPersonPeriod> wrapper = new List<IPersonPeriod>(_personCollection[0].PersonPeriodCollection);
			TimeZoneInfo timeZoneInfo = _personCollection[0].PermissionInformation.DefaultTimeZone();

			Assert.AreEqual(1, _acdTable.Rows.Count);
			Assert.AreEqual("CODE", _acdTable.Rows[0]["acd_login_code"]);
			Assert.AreEqual(timeZoneInfo.SafeConvertTimeToUtc(wrapper[0].StartDate.Date),
								 (DateTime)_acdTable.Rows[0]["start_date"]);
			Assert.AreEqual(timeZoneInfo.SafeConvertTimeToUtc(wrapper[0].EndDate().Date).AddDays(1),
								 (DateTime)_acdTable.Rows[0]["end_date"]);
			Assert.AreEqual(wrapper[0].Id, _acdTable.Rows[0]["person_period_code"]);
			Assert.AreEqual(1, _acdTable.Rows[0]["datasource_id"]);
		}

		[Test]
		public void VerifyGetPeriodIntervalEndDate()
		{
			int mintesPerInterval = 1440 / _intervalsPerDay;
			var date1 = new DateTime(2009, 10, 23, 12, 0, 0);

			Assert.AreEqual(date1.AddMinutes(-mintesPerInterval),
								 PersonTransformer.GetPeriodIntervalEndDate(date1, _intervalsPerDay));
		}

		[Test]
		public void ShouldUseCommonAgentDescriptionFormatForPersonName()
		{
			var firstName = (string)_table.Rows[0]["person_first_name"];
			var lastName = (string)_table.Rows[0]["person_last_name"];
			var employmentNumber = (string)_table.Rows[0]["employment_number"];
			var personName = (string)_table.Rows[0]["person_name"];

			string expectedPersonName = string.Format(CultureInfo.InvariantCulture, "{0} - {1} {2}", employmentNumber, firstName, lastName);

			Assert.AreEqual(expectedPersonName, personName);
		}

		  [Test]
		public void ShouldUseDomainAndUserFromLogonInfo()
		{
			var person = _personCollection.First();
			person.SetId(Guid.NewGuid());
			var logonInfos = new List<LogonInfo>
			{
				new LogonInfo {Identity = "DOMAIN\\THENAME",  PersonId = person.Id.GetValueOrDefault()}
			};

			_table = new DataTable {Locale = Thread.CurrentThread.CurrentCulture};
			PersonInfrastructure.AddColumnsToDataTable(_table);

			PersonTransformer.Transform(new List<IPerson> {person}, _intervalsPerDay, new DateOnly(2000, 1, 1), _table,
				_acdTable, _commonNameDescriptionSetting, logonInfos);
			
			((string) _table.Rows[0]["windows_domain"]).Should().Be.EqualTo("DOMAIN");
			((string)_table.Rows[0]["windows_username"]).Should().Be.EqualTo("THENAME");
		}
		
		 [Test]
		  public void ShouldModifyPersonPeriodStartDateIfItsInFarFuture()
		  {
			  var personCollection = PersonFactory.CreatePersonGraphCollectionWithInfinitStart();
			  TimeZoneInfo timeZoneInfo = personCollection[0].PermissionInformation.DefaultTimeZone();
			  var table = new DataTable();
			  table.Locale = Thread.CurrentThread.CurrentCulture;
			  var acdTable = new DataTable();
			  acdTable.Locale = Thread.CurrentThread.CurrentCulture;
			  PersonInfrastructure.AddColumnsToDataTable(table);
			  AcdLogOnPersonInfrastructure.AddColumnsToDataTable(acdTable);
			  PersonTransformer.Transform(personCollection, _intervalsPerDay, new DateOnly(2000, 1, 1), table, acdTable,
				_commonNameDescriptionSetting,new List<LogonInfo>());
			  var validFromDate = timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2059, 12, 30));
			  Assert.AreEqual(validFromDate, table.Rows[0]["valid_from_date"]);
			  Assert.AreEqual(validFromDate, table.Rows[0]["employment_start_date"]);
			  Assert.AreEqual(new DateTime(2059, 12, 30), table.Rows[0]["valid_from_date_local"]);
			  Assert.AreEqual(validFromDate, acdTable.Rows[0]["start_date"]);
		  }
		
	}
}