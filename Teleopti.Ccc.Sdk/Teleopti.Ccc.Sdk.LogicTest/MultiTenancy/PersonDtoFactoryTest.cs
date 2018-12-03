using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.WcfHost.Service.Factory;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.Sdk.LogicTest.MultiTenancy
{
	[TenantSdkTest]
	public class PersonDtoFactoryTest: IIsolateSystem
	{
		public PersonDtoFactory PersonDtoFactory;
		public FakeTenantLogonDataManager TenantLogonDataManager;
		public FakePersonRepository PersonRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeTenantLogonDataManager>().For<ITenantLogonDataManager>();
		}

		[Test]
		public void ShouldSetApplicationLogonNameWhenGetPersons()
		{
			var pers = new Person().WithId().WithName(new Name("Ola", "Håkansson"));
			PersonRepository.Add(pers);
			TenantLogonDataManager.SetLogon(pers.Id.GetValueOrDefault(), "inloggningsnamn", "DOMAIN/NAME");
			var res = PersonDtoFactory.GetPersons(false);
			res.First().ApplicationLogOnName.Should().Be.EqualTo("inloggningsnamn");
			res.First().Identity.Should().Be.EqualTo("DOMAIN/NAME");
		}

		[Test]
		public void ShouldSetLogonInfoOnTeamMembers()
		{
			var team = new Team().WithId();
			var pers = new Person().WithId().WithName(new Name("Ola", "Håkansson")).WithPersonPeriod(team);
			var pers2 = new Person().WithId().WithName(new Name("Lars", "Håkansson")).WithPersonPeriod(team);
			var pers3 = new Person().WithId().WithName(new Name("Hans", "Håkansson")).WithPersonPeriod(team);

			
			PersonRepository.Add(pers);
			PersonRepository.Add(pers2);
			PersonRepository.Add(pers3);
			TenantLogonDataManager.SetLogon(pers.Id.GetValueOrDefault(), "inloggningsnamn", "DOMAIN/NAME");
			TenantLogonDataManager.SetLogon(pers2.Id.GetValueOrDefault(), "pers2", "DOMAIN/pers2");
			TenantLogonDataManager.SetLogon(pers3.Id.GetValueOrDefault(), "pers3", "DOMAIN/pers3");
			var res = PersonDtoFactory.GetPersonTeamMembers(new PersonDto {Id = pers.Id}, DateTime.UtcNow);
			res.Count.Should().Be(3);
			res.Count(r => r.ApplicationLogOnName.Equals("inloggningsnamn")).Should().Be(1);
			res.Count(r => r.ApplicationLogOnName.Equals("pers2")).Should().Be(1);
			res.Count(r => r.ApplicationLogOnName.Equals("pers3")).Should().Be(1);
		}
		
		[Test]
		public void ShouldSetLogonInfoWhenGetPersonsByTeam()
		{
			var team = new Team().WithId();
			var pers = new Person().WithId().WithName(new Name("Ola", "Håkansson")).WithPersonPeriod(team);
			var pers2 = new Person().WithId().WithName(new Name("Lars", "Håkansson")).WithPersonPeriod(team);
			var pers3 = new Person().WithId().WithName(new Name("Hans", "Håkansson")).WithPersonPeriod(team);


			PersonRepository.Add(pers);
			PersonRepository.Add(pers2);
			PersonRepository.Add(pers3);
			TenantLogonDataManager.SetLogon(pers.Id.GetValueOrDefault(), "inloggningsnamn", "DOMAIN/NAME");
			TenantLogonDataManager.SetLogon(pers2.Id.GetValueOrDefault(), "pers2", "DOMAIN/pers2");
			TenantLogonDataManager.SetLogon(pers3.Id.GetValueOrDefault(), "pers3", "DOMAIN/pers3");

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var res = PersonDtoFactory.GetPersonsByTeam(new TeamDto {Id = team.Id}, DateTime.UtcNow,
					new PersonCollection("Raptor/Global/ViewSchedules", new List<Person> {pers, pers2, pers3},
						new DateOnly(DateTime.UtcNow)));
				res.Count.Should().Be(3);
				res.Count(r => r.ApplicationLogOnName.Equals("inloggningsnamn")).Should().Be(1);
				res.Count(r => r.ApplicationLogOnName.Equals("pers2")).Should().Be(1);
				res.Count(r => r.ApplicationLogOnName.Equals("pers3")).Should().Be(1);
			}
		}

		[Test]
		public void ShouldSetLognInfoOnLoggedOnPerson()
		{
			var pers = ((IUnsafePerson) TeleoptiPrincipal.CurrentPrincipal).Person;
			PersonRepository.Add(pers);
			TenantLogonDataManager.SetLogon(pers.Id.GetValueOrDefault(), "inloggningsnamn", "DOMAIN/NAME");
			var res = PersonDtoFactory.GetLoggedOnPerson();
			res.ApplicationLogOnName.Should().Be.EqualTo("inloggningsnamn");
			res.Identity.Should().Be.EqualTo("DOMAIN/NAME");
		}
	}

	public class FakeWorkflowControlsetAssembler : Assembler<IWorkflowControlSet, WorkflowControlSetDto>
	{
		public override WorkflowControlSetDto DomainEntityToDto(IWorkflowControlSet entity)
		{
			return new WorkflowControlSetDto();
		}

		public override IWorkflowControlSet DtoToDomainEntity(WorkflowControlSetDto dto)
		{
			throw new System.NotImplementedException();
		}
	}
}