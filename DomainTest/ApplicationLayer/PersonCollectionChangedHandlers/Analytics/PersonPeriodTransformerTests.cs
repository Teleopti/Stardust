using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[TestFixture]
	[DomainTest]
	public class PersonPeriodTransformerTests : ISetup
	{
		public FakeGlobalSettingDataRepository GlobalSettingDataRepository;
		public PersonPeriodTransformer Target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeGlobalSettingDataRepository>().For<IGlobalSettingDataRepository>();
			system.AddService<PersonPeriodTransformer>();
		}

		[Test]
		public void PersonNameShouldBeFirstNameLastName()
		{
			GlobalSettingDataRepository.PersistSettingValue("CommonNameDescription",
				new CommonNameDescriptionSetting($"{CommonNameDescriptionSetting.FirstName} {CommonNameDescriptionSetting.LastName}"));
			
			var person = PersonFactory.CreatePerson("First", "Last");
			Target.GetPersonName(person).Should().Be.EqualTo("First Last");
		}

		[Test]
		public void PersonNameShouldBeLastNameFirstName()
		{
			GlobalSettingDataRepository.PersistSettingValue("CommonNameDescription",
				new CommonNameDescriptionSetting($"{CommonNameDescriptionSetting.LastName} {CommonNameDescriptionSetting.FirstName}"));

			var person = PersonFactory.CreatePerson("First", "Last");
			Target.GetPersonName(person).Should().Be.EqualTo("Last First");
		}

		[Test]
		public void PersonNameSettingsShouldNotBeCachedInTransformerObject()
		{
			GlobalSettingDataRepository.PersistSettingValue("CommonNameDescription",
				new CommonNameDescriptionSetting($"{CommonNameDescriptionSetting.LastName} {CommonNameDescriptionSetting.FirstName}"));

			var person = PersonFactory.CreatePerson("First", "Last");
			person.SetEmploymentNumber("123");
			Target.GetPersonName(person).Should().Be.EqualTo("Last First");

			GlobalSettingDataRepository.PersistSettingValue("CommonNameDescription",
			new CommonNameDescriptionSetting($"{CommonNameDescriptionSetting.LastName} {CommonNameDescriptionSetting.FirstName} - {CommonNameDescriptionSetting.EmployeeNumber}"));

			Target.GetPersonName(person).Should().Be.EqualTo("Last First - 123");
		}
	}
}