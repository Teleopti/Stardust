using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Auditing
{
	[TestFixture]
	public class AuditSettingTest
	{
		private IAuditSetting target;
		private IAuditSetter auditSettingProvider;
			
		[SetUp]
		public void Setup()
		{
			auditSettingProvider = new AuditSetter((s) => new AuditSetting());
			target = new AuditSetting();
		}

		[Test]
		public void TurnoffScheduleAuditing()
		{
			target.TurnOffScheduleAuditing(auditSettingProvider);

			target.IsScheduleEnabled.Should().Be.False();
		}

		[Test]
		public void TurnOnScheduleAuditing()
		{
			var auditSettingRepository = MockRepository.GenerateMock<IAuditSettingRepository>();
			target.TurnOnScheduleAuditing(auditSettingRepository, auditSettingProvider);

			target.IsScheduleEnabled.Should().Be.True();
			auditSettingRepository.AssertWasCalled(rep => rep.TruncateAndMoveScheduleFromCurrentToAuditTables());
		}

		[Test]
		public void ShouldAuditScheduleDataInDefaultScenario()
		{
			var auditSettingRepository = MockRepository.GenerateMock<IAuditSettingRepository>();
			target.TurnOnScheduleAuditing(auditSettingRepository, auditSettingProvider);

			var scenario = new Scenario("default") {DefaultScenario = true};
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Person(), 
			                                                               scenario, new DateTimePeriod(2000, 1, 1, 2000, 1, 12));
			target.ShouldBeAudited(pa).Should().Be.True();
			target.ShouldBeAudited(pa.MainActivities().First()).Should().Be.True();
		}

		[Test]
		public void ShouldNotAuditWhenAuditingIsTurnedOff()
		{
			target.TurnOffScheduleAuditing(auditSettingProvider);

			var scenario = new Scenario("default") { DefaultScenario = true };
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Person(),
																								scenario, new DateTimePeriod(2000, 1, 1, 2000, 1, 12));
			target.ShouldBeAudited(pa).Should().Be.False();
			target.ShouldBeAudited(pa.MainActivities().First()).Should().Be.False();
		}

		[Test]
		public void ShouldNotAuditScheduleDataInNonDefaultScenario()
		{
			var auditSettingRepository = MockRepository.GenerateMock<IAuditSettingRepository>();
			target.TurnOnScheduleAuditing(auditSettingRepository, auditSettingProvider);

			var scenario = new Scenario("default") {DefaultScenario = false};
			var pa = PersonAbsenceFactory.CreatePersonAbsence(new Person(), scenario, new DateTimePeriod(2000, 1, 1, 2000, 1, 12));
			target.ShouldBeAudited(pa).Should().Be.False();
			target.ShouldBeAudited(pa.Layer).Should().Be.False();
		}

		[Test]
		public void ShouldNotAuditNonScheduleData()
		{
			var auditSettingRepository = MockRepository.GenerateMock<IAuditSettingRepository>();
			target.TurnOnScheduleAuditing(auditSettingRepository, auditSettingProvider);

			var person = new Person();

			target.ShouldBeAudited(person).Should().Be.False();
		}

		[Test]
		public void ShouldNotAuditNonEntities()
		{
			var auditSettingRepository = MockRepository.GenerateMock<IAuditSettingRepository>();
			target.TurnOnScheduleAuditing(auditSettingRepository, auditSettingProvider);

			target.ShouldBeAudited(new object()).Should().Be.False();
		}

		[Test]
		public void ShouldNotAuditScheduleDataWithNoScenario()
		{
			var auditSettingRepository = MockRepository.GenerateMock<IAuditSettingRepository>();
			var prefDay = new PreferenceDay(new Person(), new DateOnly(), new PreferenceRestriction());
			target.TurnOnScheduleAuditing(auditSettingRepository, auditSettingProvider);

			target.ShouldBeAudited(prefDay);
		}
	}
}