using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[TestFixture]
	public class PersonPeriodTransformerTests
	{
		private FakeAnalyticsPersonPeriodRepository fakeAnalyticsPersonPeriodRepository;
		private FakeAnalyticsSkillRepository fakeAnalyticsSkillRepository;
		private FakeAnalyticsBusinessUnitRepository fakeAnalyticsBusinessUnitRepository;
		private FakeAnalyticsTeamRepository fakeAnalyticsTeamRepository;
		private ThrowExceptionOnSkillMapError throwExceptionOnSkillMapError;
		private FakeAnalyticsDateRepository fakeAnalyticsDateRepository;
		private FakeAnalyticsTimeZoneRepository fakeAnalyticsTimeZoneRepository;
		private IAnalyticsIntervalRepository _analyticsIntervalRepository;
		private FakeGlobalSettingDataRepository _globalSettingDataRepository;
		private PersonPeriodTransformer _target;

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
			_analyticsIntervalRepository = new FakeAnalyticsIntervalRepository();
			_globalSettingDataRepository = new FakeGlobalSettingDataRepository();

			_target = new PersonPeriodTransformer(fakeAnalyticsPersonPeriodRepository,
				fakeAnalyticsSkillRepository,
				fakeAnalyticsBusinessUnitRepository,
				fakeAnalyticsTeamRepository,
				throwExceptionOnSkillMapError,
				fakeAnalyticsTimeZoneRepository,
				_analyticsIntervalRepository,
				_globalSettingDataRepository, new AnalyticsPersonPeriodDateFixer(fakeAnalyticsDateRepository, _analyticsIntervalRepository));
		}

		[Test]
		public void PersonNameShouldBeFirstNameLastName()
		{
			_globalSettingDataRepository.PersistSettingValue("CommonNameDescription",
				new CommonNameDescriptionSetting($"{CommonNameDescriptionSetting.FirstName} {CommonNameDescriptionSetting.LastName}"));
			
			var person = PersonFactory.CreatePerson("First", "Last");
			_target.GetPersonName(person).Should().Be.EqualTo("First Last");
		}

		[Test]
		public void PersonNameShouldBeLastNameFirstName()
		{
			_globalSettingDataRepository.PersistSettingValue("CommonNameDescription",
				new CommonNameDescriptionSetting($"{CommonNameDescriptionSetting.LastName} {CommonNameDescriptionSetting.FirstName}"));

			var person = PersonFactory.CreatePerson("First", "Last");
			_target.GetPersonName(person).Should().Be.EqualTo("Last First");
		}

		[Test]
		public void PersonNameSettingsShouldNotBeCachedInTransformerObject()
		{
			_globalSettingDataRepository.PersistSettingValue("CommonNameDescription",
				new CommonNameDescriptionSetting($"{CommonNameDescriptionSetting.LastName} {CommonNameDescriptionSetting.FirstName}"));

			var person = PersonFactory.CreatePerson("First", "Last");
			person.SetEmploymentNumber("123");
			_target.GetPersonName(person).Should().Be.EqualTo("Last First");

			_globalSettingDataRepository.PersistSettingValue("CommonNameDescription",
			new CommonNameDescriptionSetting($"{CommonNameDescriptionSetting.LastName} {CommonNameDescriptionSetting.FirstName} - {CommonNameDescriptionSetting.EmployeeNumber}"));

			_target.GetPersonName(person).Should().Be.EqualTo("Last First - 123");
		}
	}
}