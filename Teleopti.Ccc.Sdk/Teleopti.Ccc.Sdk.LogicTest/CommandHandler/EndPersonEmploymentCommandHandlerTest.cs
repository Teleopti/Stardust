using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Infrastructure;

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

			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var person = new Person();
			person.SetId(endPersonEmploymentCommandDto.PersonId);
			personRepository.Stub(x => x.Load(endPersonEmploymentCommandDto.PersonId)).Return(person);
			var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(MockRepository.GenerateMock<IUnitOfWork>());
			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);
			var target = new EndPersonEmploymentCommandHandler(personRepository, currentUnitOfWorkFactory);
			
			target.Handle(endPersonEmploymentCommandDto);

			person.TerminalDate.Should().Be.EqualTo(endPersonEmploymentCommandDto.Date.ToDateOnly());
			endPersonEmploymentCommandDto.Result.AffectedItems.Should().Be.EqualTo(1);
		}
	}
}