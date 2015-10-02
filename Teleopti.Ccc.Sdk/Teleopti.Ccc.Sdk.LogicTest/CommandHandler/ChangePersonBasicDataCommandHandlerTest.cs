using System;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	public class ChangePersonBasicDataCommandHandlerTest
	{
		[Test]
		public void ChangePersonBasicDataSuccessfully()
		{
			var changePersonBasicDataCommandDto = new ChangePersonBasicDataCommandDto
			{
				PersonId=Guid.NewGuid(),
				FirstName = "first",
				LastName = "last",
				Email = "test@teleopti.com",
				EmploymentNumber = "employmentNumber",
				Note = "note1",
				WorkWeekStart = DayOfWeek.Sunday,
				WorkflowControlSetId = Guid.NewGuid(),
				CultureLanguageId = CultureInfo.CurrentCulture.LCID,
				UICultureLanguageId = CultureInfo.CurrentUICulture.LCID,
				IsDeleted = true
			};

			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var workflowControlSetRepository = MockRepository.GenerateMock<IWorkflowControlSetRepository>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(MockRepository.GenerateMock<IUnitOfWork>());
			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);
			var workflowControlSet = new WorkflowControlSet("wcs1");
			workflowControlSet.SetId(changePersonBasicDataCommandDto.WorkflowControlSetId.Value);
			workflowControlSetRepository.Stub(x => x.Get(changePersonBasicDataCommandDto.WorkflowControlSetId.Value)).Return(workflowControlSet);
			var person = new Person();
			person.SetId(changePersonBasicDataCommandDto.PersonId);
			personRepository.Stub(x => x.Load(changePersonBasicDataCommandDto.PersonId)).Return(person);
			var target = new ChangePersonBasicDataCommandHandler(personRepository, currentUnitOfWorkFactory, workflowControlSetRepository);
			
			target.Handle(changePersonBasicDataCommandDto);

			person.Name.FirstName.Should().Be.EqualTo(changePersonBasicDataCommandDto.FirstName);
			person.Name.LastName.Should().Be.EqualTo(changePersonBasicDataCommandDto.LastName);
			person.Email.Should().Be.EqualTo(changePersonBasicDataCommandDto.Email);
			person.EmploymentNumber.Should().Be.EqualTo(changePersonBasicDataCommandDto.EmploymentNumber);
			person.Note.Should().Be.EqualTo(changePersonBasicDataCommandDto.Note);
			person.FirstDayOfWeek.Should().Be.EqualTo(changePersonBasicDataCommandDto.WorkWeekStart);
			person.WorkflowControlSet.Id.Should().Be.EqualTo(changePersonBasicDataCommandDto.WorkflowControlSetId);
			person.PermissionInformation.CultureLCID().Should().Be.EqualTo(changePersonBasicDataCommandDto.CultureLanguageId);
			person.PermissionInformation.UICultureLCID().Should().Be.EqualTo(changePersonBasicDataCommandDto.UICultureLanguageId);
			person.IsDeleted.Should().Be.EqualTo(changePersonBasicDataCommandDto.IsDeleted);
			changePersonBasicDataCommandDto.Result.AffectedItems.Should().Be.EqualTo(1);
		}
	}
}