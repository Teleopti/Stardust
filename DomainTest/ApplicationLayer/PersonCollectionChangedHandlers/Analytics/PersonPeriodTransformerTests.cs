using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[TestFixture]
	public class PersonPeriodTransformerTests
	{
		private PersonPeriodTransformer transformer;
		private FakeAnalyticsPersonPeriodRepository fakeAnalyticsPersonPeriodRepository;
		private FakeAnalyticsSkillRepository fakeAnalyticsSkillRepository;
		private FakeAnalyticsBusinessUnitRepository fakeAnalyticsBusinessUnitRepository;
		private FakeAnalyticsTeamRepository fakeAnalyticsTeamRepository;
		private ThrowExceptionOnSkillMapError throwExceptionOnSkillMapError;
		private FakeAnalyticsDateRepository fakeAnalyticsDateRepository;
		private FakeAnalyticsTimeZoneRepository fakeAnalyticsTimeZoneRepository;

		[SetUp]
		public void Setup()
		{
			 fakeAnalyticsPersonPeriodRepository = new FakeAnalyticsPersonPeriodRepository();
			 fakeAnalyticsSkillRepository = new FakeAnalyticsSkillRepository();
			 fakeAnalyticsBusinessUnitRepository = new FakeAnalyticsBusinessUnitRepository();
			 fakeAnalyticsTeamRepository = new FakeAnalyticsTeamRepository();
			 throwExceptionOnSkillMapError = new ThrowExceptionOnSkillMapError();
			 fakeAnalyticsDateRepository = new FakeAnalyticsDateRepository();
			 fakeAnalyticsTimeZoneRepository = new FakeAnalyticsTimeZoneRepository();
		}

		[Test]
		public void PersonNameShouldBeFirstNameLastName()
		{
			transformer = new PersonPeriodTransformer(fakeAnalyticsPersonPeriodRepository,
				fakeAnalyticsSkillRepository,
				fakeAnalyticsBusinessUnitRepository,
				fakeAnalyticsTeamRepository,
				throwExceptionOnSkillMapError,
				fakeAnalyticsDateRepository,
				fakeAnalyticsTimeZoneRepository,
				new CommonNameDescriptionSetting($"{CommonNameDescriptionSetting.FirstName} {CommonNameDescriptionSetting.LastName}"));

			var person = PersonFactory.CreatePerson("First", "Last");
			transformer.GetPersonName(person).Should().Be.EqualTo("First Last");
		}

		[Test]
		public void PersonNameShouldBeLastNameFirstName()
		{
			transformer = new PersonPeriodTransformer(fakeAnalyticsPersonPeriodRepository,
				fakeAnalyticsSkillRepository,
				fakeAnalyticsBusinessUnitRepository,
				fakeAnalyticsTeamRepository,
				throwExceptionOnSkillMapError,
				fakeAnalyticsDateRepository,
				fakeAnalyticsTimeZoneRepository,
				new CommonNameDescriptionSetting($"{CommonNameDescriptionSetting.LastName} {CommonNameDescriptionSetting.FirstName}"));

			var person = PersonFactory.CreatePerson("First", "Last");
			transformer.GetPersonName(person).Should().Be.EqualTo("Last First");
		}
	}
}