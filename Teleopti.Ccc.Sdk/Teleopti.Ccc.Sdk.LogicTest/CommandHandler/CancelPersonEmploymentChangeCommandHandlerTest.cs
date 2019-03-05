using System;
using System.ServiceModel;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.Sdk.WcfHost.Ioc;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[DomainTest]
	public class CancelPersonEmploymentChangeCommandHandlerTest : IExtendSystem
	{
		public CancelPersonEmploymentChangeCommandHandler Target;
		public FakePersonRepository PersonRepository;
		

		[Test]
		public void ShouldRemovePersonPeriodWithStartDateSameAsInputDate()
		{
			var person = PersonFactory.CreatePerson().WithId();

			person.AddPersonPeriod(new PersonPeriod(
				new DateOnly(new DateTime(2019, 02, 20)),
				new PersonContract(new Contract("Previous Person Period"), new PartTimePercentage("110%"), new ContractScheduleWorkingMondayToFriday()),
				new Team()));

			person.AddPersonPeriod(new PersonPeriod(
				new DateOnly(new DateTime(2019, 02, 25)),
				new PersonContract(new Contract("Person Period with same start date"), new PartTimePercentage("110%"), new ContractScheduleWorkingMondayToFriday()),
				new Team()));

			PersonRepository.Has(person);

			Target.Handle(new CancelPersonEmploymentChangeCommandDto
			{
				Date = new DateOnlyDto
				{
					DateTime = new DateTime(2019, 02, 25)
				},
				PersonId = person.Id.Value
			});

			Assert.AreEqual(1, person.PersonPeriodCollection.Count);
		}

		[Test]
		public void ShouldKeepPreviousPersonPeroids()
		{
			var person = PersonFactory.CreatePerson().WithId();

			person.AddPersonPeriod(new PersonPeriod(
				new DateOnly(new DateTime(2019, 02, 20)),
				new PersonContract(new Contract("Previous Person Period #1"), new PartTimePercentage("110%"), new ContractScheduleWorkingMondayToFriday()),
				new Team()));

			person.AddPersonPeriod(new PersonPeriod(
				new DateOnly(new DateTime(2019, 02, 22)),
				new PersonContract(new Contract("Previous Person Period #2"), new PartTimePercentage("110%"), new ContractScheduleWorkingMondayToFriday()),
				new Team()));

			PersonRepository.Has(person);

			Target.Handle(new CancelPersonEmploymentChangeCommandDto
			{
				Date = new DateOnlyDto
				{
					DateTime = new DateTime(2019, 02, 25)
				},
				PersonId = person.Id.Value
			});

			Assert.AreEqual(2, person.PersonPeriodCollection.Count);
		}

		[Test]
		public void NonExistantUserShouldThrow()
		{
			var message = string.Empty;

			try
			{
				Target.Handle(new CancelPersonEmploymentChangeCommandDto
				{
					Date = new DateOnlyDto
					{
						DateTime = new DateTime(2019, 02, 25)
					},
					PersonId = Guid.NewGuid()
				});
			}
			catch (FaultException e)
			{
				message = e.Message;
			}

			Assert.IsNotEmpty(message);
		}

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<CancelPersonEmploymentChangeCommandHandler>();
			extend.AddModule(new AssemblerModule());
		}
	}
}
