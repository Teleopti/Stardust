using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Rta.Service;
using Teleopti.Ccc.Domain.Rta.ViewModels;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.AgentState.Reader
{
	[DatabaseTest]
	[TestFixture]
	public class OrderNameTest
	{
		public IAgentStateReadModelPersister Persister;
		public IAgentStateReadModelReader Target;
		public ICurrentBusinessUnit BusinessUnit;
		public IGlobalSettingDataRepository Settings;
		public WithUnitOfWork UnitOfWork;

		[Test]
		public void ShouldOrderByLastName()
		{
			var person1 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f8");
			var person2 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f9");
			UnitOfWork.Do(() =>
			{
				Persister.UpsertAssociation(new AssociationInfo
				{
					PersonId = person1,
					BusinessUnitId = BusinessUnit.Current().Id.Value
				});
				Persister.UpsertAssociation(new AssociationInfo
				{
					PersonId = person2,
					BusinessUnitId = BusinessUnit.Current().Id.Value
				});
				Persister.UpsertName(person1, "Ashley", "Baldi");
				Persister.UpsertName(person2, "Pierre", "Andeen");

				var setting = Settings.FindValueByKey(CommonNameDescriptionSetting.Key, new CommonNameDescriptionSetting());
				setting.AliasFormat = "{LastName} 	--  {unknownJunk} {FirstName}";
				Settings.PersistSettingValue(setting);
			});

			var result = UnitOfWork.Get(() => Target.Read(new AgentStateFilter {OrderBy = "Name"}));

			result.First().PersonId.Should().Be(person2);
			result.Last().PersonId.Should().Be(person1);
		}
		
		[Test]
		public void ShouldOrderByEmploymentNumber()
		{
			var person1 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f8");
			var person2 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f9");
			UnitOfWork.Do(() =>
			{
				Persister.UpsertAssociation(new AssociationInfo
				{
					PersonId = person1,
					BusinessUnitId = BusinessUnit.Current().Id.Value
				});
				Persister.UpsertAssociation(new AssociationInfo
				{
					PersonId = person2,
					BusinessUnitId = BusinessUnit.Current().Id.Value
				});
				Persister.UpsertName(person1, "Ashley", "Andeen");
				Persister.UpsertEmploymentNumber(person1, "2");
				Persister.UpsertName(person2, "Pierre", "Baldi");
				Persister.UpsertEmploymentNumber(person2, "1");

				var setting = Settings.FindValueByKey(CommonNameDescriptionSetting.Key, new CommonNameDescriptionSetting());
				setting.AliasFormat = "		{EmployeeNumber}	{LastName}	{FirstName}";
				Settings.PersistSettingValue(setting);
			});

			var result = UnitOfWork.Get(() => Target.Read(new AgentStateFilter {OrderBy = "Name"}));

			result.First().PersonId.Should().Be(person2);
			result.Last().PersonId.Should().Be(person1);
		}
	}
}