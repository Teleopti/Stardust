using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
	[TestFixture]
	public class PersonPeriodAssemblerTest
	{
		[Test]
		public void ShouldCreateDtoFromEntity()
		{
			var dateOnly = new DateOnly(2010, 1, 1);
			var person =
				PersonFactory.CreatePersonWithValidVirtualSchedulePeriodAndMustHave(PersonFactory.CreatePerson(), dateOnly)
					.WithId();

			var externalLogonAssembler = new ExternalLogOnAssembler();
			var personPeriodAssembler = new PersonPeriodAssembler(externalLogonAssembler);
			var externalLogOn = new ExternalLogOn(1, 2, "3", "4", true);
			var personPeriod = person.Period(dateOnly);
			personPeriod.Team.Site = SiteFactory.CreateSimpleSite().WithId();
			personPeriod.PersonContract.Contract.WithId();
			personPeriod.PersonContract.ContractSchedule.WithId();
			personPeriod.PersonContract.PartTimePercentage.WithId();
			person.AddExternalLogOn(externalLogOn, personPeriod);
			personPeriod.Note = "my note";

			var personPeriodDto = personPeriodAssembler.DomainEntityToDto(personPeriod);

			Assert.AreNotEqual(Guid.Empty, personPeriodDto.PersonId);
			Assert.IsNotNull(personPeriodDto.StartDate);
			Assert.IsNotNull(personPeriodDto.Team);
			Assert.IsNotNull(personPeriodDto.Note);
			Assert.AreNotEqual(Guid.Empty, personPeriodDto.ContractId);
			Assert.AreNotEqual(Guid.Empty, personPeriodDto.PartTimePercentageId);
			Assert.AreNotEqual(Guid.Empty, personPeriodDto.ContractScheduleId);
			Assert.AreNotEqual(Guid.Empty, personPeriodDto.ContractScheduleId);
			Assert.IsTrue(personPeriodDto.ExternalLogOn.Count == 1);
		}
	}
}
