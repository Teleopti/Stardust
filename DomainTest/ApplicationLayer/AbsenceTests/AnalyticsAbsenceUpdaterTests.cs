using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Absence;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceTests
{
	[TestFixture]
	public class AnalyticsAbsenceUpdaterTests
	{
		private AnalyticsAbsenceUpdater _target;
		private FakeAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private FakeAnalyticsAbsenceRepository _analyticsAbsenceRepository;
		private FakeAbsenceRepository _absenceRepository;

		[SetUp]
		public void Setup()
		{
			_analyticsBusinessUnitRepository = new FakeAnalyticsBusinessUnitRepository();
			_absenceRepository = new FakeAbsenceRepository();
			_analyticsAbsenceRepository = new FakeAnalyticsAbsenceRepository();
			_target = new AnalyticsAbsenceUpdater(_absenceRepository, _analyticsAbsenceRepository,  _analyticsBusinessUnitRepository);
		}

		[Test]
		public void ShouldAddAbsenceToAnalytics()
		{
			_analyticsAbsenceRepository.Absences().Count.Should().Be.EqualTo(0);
			var absence = AbsenceFactory.CreateAbsenceWithId();
			absence.InPaidTime = true;
			_absenceRepository.Add(absence);
			_target.Handle(new AbsenceChangedEvent
			{
				AbsenceId = absence.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			});

			_analyticsAbsenceRepository.Absences().Count.Should().Be.EqualTo(1);
			var analyticsAbsence = _analyticsAbsenceRepository.Absences().First();
			analyticsAbsence.AbsenceCode.Should().Be.EqualTo(absence.Id.GetValueOrDefault());
			analyticsAbsence.InPaidTime.Should().Be.EqualTo(absence.InPaidTime);
			analyticsAbsence.InPaidTimeName.Should().Be.EqualTo(absence.InPaidTime ? AnalyticsAbsence.InPaidTimeTimeString : AnalyticsAbsence.NotInPaidTimeTimeString);
		}

		[Test]
		public void ShouldUpdateAbsenceToAnalytics()
		{
			_analyticsAbsenceRepository.Absences().Count.Should().Be.EqualTo(0);
			var absence = AbsenceFactory.CreateAbsence("New Absence Name");
			absence.SetId(Guid.NewGuid());
			absence.InPaidTime = true;
			_absenceRepository.Add(absence);
			_analyticsAbsenceRepository.AddAbsence(new AnalyticsAbsence
			{
				AbsenceCode = absence.Id.GetValueOrDefault(),
				AbsenceName = "Old Absence Name",
				InPaidTime = false,
				InPaidTimeName = AnalyticsAbsence.NotInPaidTimeTimeString
			});
			_analyticsAbsenceRepository.Absences().Count.Should().Be.EqualTo(1);

			_target.Handle(new AbsenceChangedEvent
			{
				AbsenceId = absence.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			});

			_analyticsAbsenceRepository.Absences().Count.Should().Be.EqualTo(1);
			var analyticsScenario = _analyticsAbsenceRepository.Absences().First();
			analyticsScenario.AbsenceName.Should().Be.EqualTo("New Absence Name");
			analyticsScenario.InPaidTime.Should().Be.True();
			analyticsScenario.InPaidTimeName.Should().Be.EqualTo(AnalyticsAbsence.InPaidTimeTimeString);
		}

		[Test]
		public void ShouldSetAbsenceToDelete()
		{
			_analyticsAbsenceRepository.Absences().Count.Should().Be.EqualTo(0);
			var absence = AbsenceFactory.CreateAbsenceWithId();
			_absenceRepository.Add(absence);
			_analyticsAbsenceRepository.AddAbsence(new AnalyticsAbsence
			{
				AbsenceCode = absence.Id.GetValueOrDefault()
			});
			_analyticsAbsenceRepository.Absences().Count.Should().Be.EqualTo(1);
			_analyticsAbsenceRepository.Absences().First().IsDeleted.Should().Be.False();

			_target.Handle(new AbsenceDeletedEvent
			{
				AbsenceId = absence.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			});

			_analyticsAbsenceRepository.Absences().Count.Should().Be.EqualTo(1);
			var analyticsAbsence = _analyticsAbsenceRepository.Absences().First();
			analyticsAbsence.IsDeleted.Should().Be.True();
		}
	}
}