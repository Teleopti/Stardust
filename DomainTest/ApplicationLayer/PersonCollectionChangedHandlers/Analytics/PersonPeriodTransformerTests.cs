using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[TestFixture]
	[DomainTest]
	public class PersonPeriodTransformerTests : IExtendSystem
	{
		public FakeGlobalSettingDataRepository GlobalSettingDataRepository;
		public FakeAnalyticsBusinessUnitRepository AnalyticsBusinessUnitRepository;
		public FakeAnalyticsTeamRepository AnalyticsTeamRepository;
		public FakeAnalyticsPersonPeriodRepository AnalyticsPersonPeriodRepository;
		public FakeAnalyticsSkillRepository AnalyticsSkillRepository;
		public PersonPeriodTransformer Target;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<PersonPeriodTransformer>();
		}

		[Test]
		public void PersonNameShouldBeFirstNameLastName()
		{
			GlobalSettingDataRepository.PersistSettingValue("CommonNameDescription",
				new CommonNameDescriptionSetting(
					$"{CommonNameDescriptionSetting.FirstName} {CommonNameDescriptionSetting.LastName}"));

			var person = PersonFactory.CreatePerson("First", "Last");
			Target.GetPersonName(person).Should().Be.EqualTo("First Last");
		}

		[Test]
		public void PersonNameShouldBeLastNameFirstName()
		{
			GlobalSettingDataRepository.PersistSettingValue("CommonNameDescription",
				new CommonNameDescriptionSetting(
					$"{CommonNameDescriptionSetting.LastName} {CommonNameDescriptionSetting.FirstName}"));

			var person = PersonFactory.CreatePerson("First", "Last");
			Target.GetPersonName(person).Should().Be.EqualTo("Last First");
		}

		[Test]
		public void PersonNameSettingsShouldNotBeCachedInTransformerObject()
		{
			GlobalSettingDataRepository.PersistSettingValue("CommonNameDescription",
				new CommonNameDescriptionSetting(
					$"{CommonNameDescriptionSetting.LastName} {CommonNameDescriptionSetting.FirstName}"));

			var person = PersonFactory.CreatePerson("First", "Last");
			person.SetEmploymentNumber("123");
			Target.GetPersonName(person).Should().Be.EqualTo("Last First");

			GlobalSettingDataRepository.PersistSettingValue("CommonNameDescription",
				new CommonNameDescriptionSetting(
					$"{CommonNameDescriptionSetting.LastName} {CommonNameDescriptionSetting.FirstName} - {CommonNameDescriptionSetting.EmployeeNumber}"));

			Target.GetPersonName(person).Should().Be.EqualTo("Last First - 123");
		}

		[Test]
		public void ShouldMapPersonPeriods()
		{
			const string notDefined = "Not Defined";
			const int businessUnitId = 9;
			const int skillsetId = 25;

			GlobalSettingDataRepository.PersistSettingValue("CommonNameDescription",
				new CommonNameDescriptionSetting(
					$"{CommonNameDescriptionSetting.LastName}, {CommonNameDescriptionSetting.FirstName}"));

			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(DateOnly.Today.AddDays(-1));
			var personPeriod = person.PersonPeriodCollection.First();

			var team = personPeriod.Team;
			var site = team.Site;
			var businessUnit = personPeriod.Team.BusinessUnitExplicit;

			AnalyticsBusinessUnitRepository.UseList = true;
			AnalyticsBusinessUnitRepository.AddOrUpdate(new AnalyticBusinessUnit
			{
				BusinessUnitId = businessUnitId,
				BusinessUnitCode = businessUnit.Id.GetValueOrDefault(),
				BusinessUnitName = businessUnit.Name
			});

			AnalyticsSkillRepository.AddSkillSet(new AnalyticsSkillSet
			{
				SkillsetId = skillsetId,
				SkillsetCode = Guid.NewGuid().ToString(),
				SkillsetName = "Test Skillset",
				BusinessUnitId = businessUnitId,
				DatasourceId = 1,
				InsertDate = DateTime.Now,
				UpdateDate = DateTime.Now,
				DatasourceUpdateDate = DateTime.Now
			});
			
			var siteId = AnalyticsPersonPeriodRepository.GetOrCreateSite(site.Id.GetValueOrDefault(),
				site.Description.Name, businessUnitId);
			
			var teamId = AnalyticsTeamRepository.GetOrCreate(team.Id.GetValueOrDefault(), siteId,
				team.Description.Name, businessUnitId);

			var result = Target.Transform(person, personPeriod, out var analyticsSkills);

			result.PersonCode.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			result.PersonPeriodCode.Should().Be.EqualTo(personPeriod.Id.GetValueOrDefault());
			result.PersonName.Should().Be.EqualTo($"{person.Name.LastName}, {person.Name.FirstName}");
			result.FirstName.Should().Be.EqualTo(person.Name.FirstName);
			result.LastName.Should().Be.EqualTo(person.Name.LastName);
			result.EmploymentNumber.Should().Be.EqualTo(person.EmploymentNumber);
			result.EmploymentTypeCode.Should().Be.EqualTo(null);
			result.EmploymentTypeName.Should().Be.EqualTo(personPeriod.PersonContract.Contract.EmploymentType.ToString());
			result.ContractCode.Should().Be.EqualTo(personPeriod.PersonContract.Contract.Id.GetValueOrDefault());
			result.ContractName.Should().Be.EqualTo(personPeriod.PersonContract.Contract.Description.Name);
			result.ParttimeCode.Should().Be.EqualTo(personPeriod.PersonContract.PartTimePercentage.Id.GetValueOrDefault());
			result.ParttimePercentage.Should().Be
				.EqualTo(personPeriod.PersonContract.PartTimePercentage.Percentage.Value * 100 + "%");
			result.TeamId.Should().Be.EqualTo(teamId);
			result.TeamCode.Should().Be.EqualTo(personPeriod.Team.Id.GetValueOrDefault());
			result.TeamName.Should().Be.EqualTo(personPeriod.Team.Description.Name);
			result.SiteId.Should().Be.EqualTo(siteId);
			result.SiteCode.Should().Be.EqualTo(personPeriod.Team.Site.Id.GetValueOrDefault());
			result.SiteName.Should().Be.EqualTo(personPeriod.Team.Site.Description.Name);
			result.BusinessUnitId.Should().Be.EqualTo(businessUnitId);
			result.BusinessUnitCode.Should().Be.EqualTo(personPeriod.Team.BusinessUnitExplicit.Id.GetValueOrDefault());
			result.BusinessUnitName.Should().Be.EqualTo(personPeriod.Team.BusinessUnitExplicit.Name);
			result.SkillsetId.Should().Be.EqualTo(-1); // No skill assigned to person period
			result.Email.Should().Be.EqualTo(person.Email);
			result.Note.Should().Be.EqualTo(person.Note);
			result.IsAgent.Should().Be.EqualTo(person.IsAgent(DateOnly.Today));
			result.IsUser.Should().Be.EqualTo(false);
			result.DatasourceId.Should().Be.EqualTo(1);
			result.DatasourceUpdateDate.Should().Be.EqualTo(person.UpdatedOn.GetValueOrDefault());
			result.ToBeDeleted.Should().Be.EqualTo(false);
			result.WindowsDomain.Should().Be.EqualTo(notDefined);
			result.WindowsUsername.Should().Be.EqualTo(notDefined);

			analyticsSkills.Should().Be.Null();
		}
	}
}