using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Absence;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Absences
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsAbsenceUpdaterTests : ISetup
	{
		public AnalyticsAbsenceUpdater Target;
		public FakeAnalyticsBusinessUnitRepository AnalyticsBusinessUnitRepository;
		public FakeAnalyticsAbsenceRepository AnalyticsAbsenceRepository;
		public FakeAbsenceRepository AbsenceRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AnalyticsAbsenceUpdater>();
		}

		[Test]
		public void ShouldAddAbsenceToAnalytics()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			AnalyticsAbsenceRepository.Absences().Count.Should().Be.EqualTo(0);
			var absence = AbsenceFactory.CreateAbsenceWithId();
			absence.InPaidTime = true;
			AbsenceRepository.Add(absence);

			Target.Handle(new AbsenceChangedEvent
			{
				AbsenceId = absence.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnitId
			});

			AnalyticsAbsenceRepository.Absences().Count.Should().Be.EqualTo(1);
			var analyticsAbsence = AnalyticsAbsenceRepository.Absences().First();
			analyticsAbsence.AbsenceCode.Should().Be.EqualTo(absence.Id.GetValueOrDefault());
			analyticsAbsence.InPaidTime.Should().Be.EqualTo(absence.InPaidTime);
			analyticsAbsence.InPaidTimeName.Should().Be.EqualTo(absence.InPaidTime ? AnalyticsAbsence.InPaidTimeTimeString : AnalyticsAbsence.NotInPaidTimeTimeString);
		}

		[Test]
		public void ShouldUpdateAbsenceToAnalytics()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			AnalyticsAbsenceRepository.Absences().Count.Should().Be.EqualTo(0);
			var absence = AbsenceFactory.CreateAbsence("New Absence Name");
			absence.SetId(Guid.NewGuid());
			absence.InPaidTime = true;
			AbsenceRepository.Add(absence);
			AnalyticsAbsenceRepository.AddAbsence(new AnalyticsAbsence
			{
				AbsenceCode = absence.Id.GetValueOrDefault(),
				AbsenceName = "Old Absence Name",
				InPaidTime = false,
				InPaidTimeName = AnalyticsAbsence.NotInPaidTimeTimeString
			});
			AnalyticsAbsenceRepository.Absences().Count.Should().Be.EqualTo(1);

			Target.Handle(new AbsenceChangedEvent
			{
				AbsenceId = absence.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnitId
			});

			AnalyticsAbsenceRepository.Absences().Count.Should().Be.EqualTo(1);
			var analyticsScenario = AnalyticsAbsenceRepository.Absences().First();
			analyticsScenario.AbsenceName.Should().Be.EqualTo("New Absence Name");
			analyticsScenario.InPaidTime.Should().Be.True();
			analyticsScenario.InPaidTimeName.Should().Be.EqualTo(AnalyticsAbsence.InPaidTimeTimeString);
		}

		[Test]
		public void ShouldSetAbsenceToDelete()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			AnalyticsAbsenceRepository.Absences().Count.Should().Be.EqualTo(0);
			var absence = AbsenceFactory.CreateAbsenceWithId();
			AbsenceRepository.Add(absence);
			AnalyticsAbsenceRepository.AddAbsence(new AnalyticsAbsence
			{
				AbsenceCode = absence.Id.GetValueOrDefault()
			});
			AnalyticsAbsenceRepository.Absences().Count.Should().Be.EqualTo(1);
			AnalyticsAbsenceRepository.Absences().First().IsDeleted.Should().Be.False();

			Target.Handle(new AbsenceDeletedEvent
			{
				AbsenceId = absence.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnitId
			});

			AnalyticsAbsenceRepository.Absences().Count.Should().Be.EqualTo(1);
			var analyticsAbsence = AnalyticsAbsenceRepository.Absences().First();
			analyticsAbsence.IsDeleted.Should().Be.True();
		}
	}
}