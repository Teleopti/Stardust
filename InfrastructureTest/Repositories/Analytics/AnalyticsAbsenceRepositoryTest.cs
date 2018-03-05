using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	public class AnalyticsAbsenceRepositoryTest
	{
		public IAnalyticsAbsenceRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		private AnalyticsDataFactory analyticsDataFactory;
		private UtcAndCetTimeZones _timeZones;
		private ExistingDatasources _datasource;
		private const int businessUnitId = 12;

		[SetUp]
		public void Setup()
		{
			analyticsDataFactory = new AnalyticsDataFactory();
			_timeZones = new UtcAndCetTimeZones();
			_datasource = new ExistingDatasources(_timeZones);
		}

		[Test]
		public void ShouldLoadAbsences()
		{
			var absence = new Absence(22, Guid.NewGuid(), "Freee", Color.LightGreen, _datasource, businessUnitId);
			analyticsDataFactory.Setup(absence);
			analyticsDataFactory.Persist();

			var absences = WithAnalyticsUnitOfWork.Get(() => Target.Absences());
			absences.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldLoadAbsence()
		{
			var id = Guid.NewGuid();
			var absence = new Absence(22, id, "Freee", Color.LightGreen, _datasource, businessUnitId);
			analyticsDataFactory.Setup(absence);
			analyticsDataFactory.Persist();

			var a = WithAnalyticsUnitOfWork.Get(() => Target.Absence(id));
			a.AbsenceCode.Should().Be.EqualTo(id);
			a.AbsenceId.Should().Be.EqualTo(22);
			a.AbsenceName.Should().Be.EqualTo("Freee");
			a.DisplayColor.Should().Be.EqualTo(Color.LightGreen.ToArgb());
		}

		[Test]
		public void ShouldLoadNullAbsenceForNotFound()
		{
			var id = Guid.NewGuid();
			var absence = new Absence(22, id, "Freee", Color.LightGreen, _datasource, businessUnitId);
			analyticsDataFactory.Setup(absence);
			analyticsDataFactory.Persist();

			var a = WithAnalyticsUnitOfWork.Get(() => Target.Absence(Guid.NewGuid()));
			a.Should().Be.Null();
		}

		[Test]
		public void ShouldAddAbsenceAndMapAllValues()
		{
			var analyticsAbsence = new AnalyticsAbsence
			{
				AbsenceCode = Guid.NewGuid(),
				AbsenceName = "Absence Name",
				AbsenceShortName = "Absence Short Name",
				BusinessUnitId = -1,
				DatasourceId = 1,
				DatasourceUpdateDate = new DateTime(2001, 1, 1),
				DisplayColor = 123,
				DisplayColorHtml = "#111111",
				InContractTime = true,
				InContractTimeName = AnalyticsAbsence.InContractTimeString,
				InPaidTime = true,
				InPaidTimeName = AnalyticsAbsence.InPaidTimeTimeString,
				InWorkTime = true,
				InWorkTimeName = AnalyticsAbsence.InWorkTimeTimeString,
				IsDeleted = false
			};

			WithAnalyticsUnitOfWork.Do(() => Target.AddAbsence(analyticsAbsence));
			WithAnalyticsUnitOfWork.Do(() => Target.Absences().Count.Should().Be.EqualTo(1));
			var absenceFromDb = WithAnalyticsUnitOfWork.Get(() => Target.Absences().First(a => a.AbsenceCode == analyticsAbsence.AbsenceCode));
			absenceFromDb.AbsenceCode.Should().Be.EqualTo(analyticsAbsence.AbsenceCode);
			absenceFromDb.AbsenceName.Should().Be.EqualTo(analyticsAbsence.AbsenceName);
			absenceFromDb.AbsenceShortName.Should().Be.EqualTo(analyticsAbsence.AbsenceShortName);
			absenceFromDb.BusinessUnitId.Should().Be.EqualTo(analyticsAbsence.BusinessUnitId);
			absenceFromDb.DatasourceId.Should().Be.EqualTo(analyticsAbsence.DatasourceId);
			absenceFromDb.DatasourceUpdateDate.Should().Be.EqualTo(analyticsAbsence.DatasourceUpdateDate);
			absenceFromDb.DisplayColor.Should().Be.EqualTo(analyticsAbsence.DisplayColor);
			absenceFromDb.DisplayColorHtml.Should().Be.EqualTo(analyticsAbsence.DisplayColorHtml);
			absenceFromDb.InContractTime.Should().Be.EqualTo(analyticsAbsence.InContractTime);
			absenceFromDb.InContractTimeName.Should().Be.EqualTo(analyticsAbsence.InContractTimeName);
			absenceFromDb.InPaidTime.Should().Be.EqualTo(analyticsAbsence.InPaidTime);
			absenceFromDb.InPaidTimeName.Should().Be.EqualTo(analyticsAbsence.InPaidTimeName);
			absenceFromDb.InWorkTime.Should().Be.EqualTo(analyticsAbsence.InWorkTime);
			absenceFromDb.InWorkTimeName.Should().Be.EqualTo(analyticsAbsence.InWorkTimeName);
			absenceFromDb.IsDeleted.Should().Be.EqualTo(analyticsAbsence.IsDeleted);
		}

		[Test]
		public void ShouldUpdateAbsenceAndMapAllValues()
		{
			var absenceCode = Guid.NewGuid();
			var absence = new Absence(22, absenceCode, "Freee", Color.LightGreen, _datasource, businessUnitId);
			analyticsDataFactory.Setup(absence);
			analyticsDataFactory.Persist();
			WithAnalyticsUnitOfWork.Do(() => Target.Absences().Count.Should().Be.EqualTo(1));

			var analyticsAbsence = new AnalyticsAbsence
			{
				AbsenceCode = absenceCode,
				AbsenceName = "New Absence Name",
				AbsenceShortName = "New Absence Short Name",
				BusinessUnitId = businessUnitId,
				DatasourceId = 1,
				DatasourceUpdateDate = new DateTime(2001, 1, 1),
				DisplayColor = 123,
				DisplayColorHtml = "#111111",
				InContractTime = false,
				InContractTimeName = AnalyticsAbsence.NotInContractTimeString,
				InPaidTime = true,
				InPaidTimeName = AnalyticsAbsence.InPaidTimeTimeString,
				InWorkTime = true,
				InWorkTimeName = AnalyticsAbsence.InWorkTimeTimeString,
				IsDeleted = true
			};

			WithAnalyticsUnitOfWork.Do(() => Target.UpdateAbsence(analyticsAbsence));
			WithAnalyticsUnitOfWork.Do(() => Target.Absences().Count.Should().Be.EqualTo(1));
			var absenceFromDb = WithAnalyticsUnitOfWork.Get(() => Target.Absences()).First(a => a.AbsenceCode == absenceCode);
			absenceFromDb.AbsenceCode.Should().Be.EqualTo(analyticsAbsence.AbsenceCode);
			absenceFromDb.AbsenceName.Should().Be.EqualTo(analyticsAbsence.AbsenceName);
			absenceFromDb.AbsenceShortName.Should().Be.EqualTo(analyticsAbsence.AbsenceShortName);
			absenceFromDb.BusinessUnitId.Should().Be.EqualTo(analyticsAbsence.BusinessUnitId);
			absenceFromDb.DatasourceId.Should().Be.EqualTo(analyticsAbsence.DatasourceId);
			absenceFromDb.DatasourceUpdateDate.Should().Be.EqualTo(analyticsAbsence.DatasourceUpdateDate);
			absenceFromDb.DisplayColor.Should().Be.EqualTo(analyticsAbsence.DisplayColor);
			absenceFromDb.DisplayColorHtml.Should().Be.EqualTo(analyticsAbsence.DisplayColorHtml);
			absenceFromDb.InContractTime.Should().Be.EqualTo(analyticsAbsence.InContractTime);
			absenceFromDb.InContractTimeName.Should().Be.EqualTo(analyticsAbsence.InContractTimeName);
			absenceFromDb.InPaidTime.Should().Be.EqualTo(analyticsAbsence.InPaidTime);
			absenceFromDb.InPaidTimeName.Should().Be.EqualTo(analyticsAbsence.InPaidTimeName);
			absenceFromDb.InWorkTime.Should().Be.EqualTo(analyticsAbsence.InWorkTime);
			absenceFromDb.InWorkTimeName.Should().Be.EqualTo(analyticsAbsence.InWorkTimeName);
			absenceFromDb.IsDeleted.Should().Be.EqualTo(analyticsAbsence.IsDeleted);
		}

		[Test]
		public void ShouldDeleteAbsenceWithoutADatasourceUpdateDate()
		{
			var absenceCode = Guid.NewGuid();
			var absence = new Absence(22, absenceCode, "Freee", Color.LightGreen, _datasource, businessUnitId)
			{
				SkipDatasourceUpdateDate = true
			};
			analyticsDataFactory.Setup(absence);
			analyticsDataFactory.Persist();
			WithAnalyticsUnitOfWork.Do(() => Target.Absences().Count.Should().Be.EqualTo(1));

			var analyticsAbsence = new AnalyticsAbsence
			{
				AbsenceCode = absenceCode,
				AbsenceName = "New Absence Name",
				AbsenceShortName = "New Absence Short Name",
				BusinessUnitId = businessUnitId,
				DatasourceId = 1,
				DatasourceUpdateDate = new DateTime(2001, 1, 1),
				DisplayColor = 123,
				DisplayColorHtml = "#111111",
				InContractTime = false,
				InContractTimeName = AnalyticsAbsence.NotInContractTimeString,
				InPaidTime = true,
				InPaidTimeName = AnalyticsAbsence.InPaidTimeTimeString,
				InWorkTime = true,
				InWorkTimeName = AnalyticsAbsence.InWorkTimeTimeString,
				IsDeleted = true
			};

			WithAnalyticsUnitOfWork.Do(() => Target.UpdateAbsence(analyticsAbsence));
			WithAnalyticsUnitOfWork.Do(() => Target.Absences().Count.Should().Be.EqualTo(1));
			var absenceFromDb = WithAnalyticsUnitOfWork.Get(() => Target.Absences()).First(a => a.AbsenceCode == absenceCode);
			absenceFromDb.AbsenceCode.Should().Be.EqualTo(analyticsAbsence.AbsenceCode);
			absenceFromDb.IsDeleted.Should().Be.EqualTo(analyticsAbsence.IsDeleted);
		}
	}
}