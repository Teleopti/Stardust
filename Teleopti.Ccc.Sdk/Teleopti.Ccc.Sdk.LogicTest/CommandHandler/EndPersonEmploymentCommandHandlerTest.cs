using System;
using System.ServiceModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	public class EndPersonEmploymentCommandHandlerTest
	{
		[Test]
		public void EndPersonEmploymentSuccessfully()
		{
			var endPersonEmploymentCommandDto = new EndPersonEmploymentCommandDto
			{
				PersonId = Guid.NewGuid(),
				Date = new DateOnlyDto(2015, 10, 1)
			};

			var personRepository = new FakePersonRepository();
			var person = new Person().WithId(endPersonEmploymentCommandDto.PersonId);
			personRepository.Has(person);
			var currentUnitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();
			var target = new EndPersonEmploymentCommandHandler(personRepository, currentUnitOfWorkFactory);
			
			target.Handle(endPersonEmploymentCommandDto);

			person.TerminalDate.Should().Be.EqualTo(endPersonEmploymentCommandDto.Date.ToDateOnly());
			endPersonEmploymentCommandDto.Result.AffectedItems.Should().Be.EqualTo(1);
		}

		[Test]
		public void EndPersonEmploymentShouldFailWhenNotPermitted()
		{
			var endPersonEmploymentCommandDto = new EndPersonEmploymentCommandDto
			{
				PersonId = Guid.NewGuid(),
				Date = new DateOnlyDto(2015, 10, 1)
			};

			var personRepository = new FakePersonRepository();
			var person = new Person().WithId(endPersonEmploymentCommandDto.PersonId);
			personRepository.Has(person);
			var currentUnitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();
			var target = new EndPersonEmploymentCommandHandler(personRepository, currentUnitOfWorkFactory);
			
			using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
			{
				Assert.Throws<FaultException>(() => target.Handle(endPersonEmploymentCommandDto));
			}
		}

		[Test]
		public void EndPersonEmploymentSuccessfullyWithClearingEverythingAfterLeavingDate()
		{
			var endPersonEmploymentCommandDto = new EndPersonEmploymentCommandDto
			{
				PersonId = Guid.NewGuid(),
				Date = new DateOnlyDto(2015, 10, 1),
				ClearAfterLeavingDate = true,
			};

			var personRepository = new FakePersonRepository();
			var person = new Person().WithId(endPersonEmploymentCommandDto.PersonId);
			personRepository.Has(person);
			var currentUnitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();
			var target = new EndPersonEmploymentCommandHandler(personRepository, currentUnitOfWorkFactory);

			target.Handle(endPersonEmploymentCommandDto);

			person.TerminalDate.Should().Be.EqualTo(endPersonEmploymentCommandDto.Date.ToDateOnly());
			endPersonEmploymentCommandDto.Result.AffectedItems.Should().Be.EqualTo(1);
		}
	}
}