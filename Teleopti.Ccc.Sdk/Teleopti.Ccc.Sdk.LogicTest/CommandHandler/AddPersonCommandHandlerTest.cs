using System;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	public class AddPersonCommandHandlerTest
	{
		[Test]
		public void PersonIsAddedSuccessfully()
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var tenantDataManager = MockRepository.GenerateMock<ITenantDataManager>();
			var workflowControlSetRepository = MockRepository.GenerateMock<IWorkflowControlSetRepository>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);
			tenantDataManager.Stub(x => x.SaveTenantData(Arg<TenantAuthenticationData>.Is.Anything))
				.Return(new SavePersonInfoResult
				{
					Success = true
				});
			var target = new AddPersonCommandHandler(personRepository, currentUnitOfWorkFactory, tenantDataManager, workflowControlSetRepository);
			var addPersonCommandDto = new AddPersonCommandDto
			{
				FirstName = "first",
				LastName = "last",
				Email = "test@teleopti.com",
				EmploymentNumber = "employmentNumber",
				Identity = "identity1",
				ApplicationLogonName = "logonname",
				ApplicationLogOnPassword = "password",
				Note = "note1",
				WorkWeekStart = DayOfWeek.Sunday,
				WorkflowControlSetId = Guid.NewGuid(),
				TimeZoneId = TimeZoneInfo.Utc.Id,
				CultureLanguageId = CultureInfo.CurrentCulture.LCID,
				UICultureLanguageId = CultureInfo.CurrentUICulture.LCID,
				IsDeleted = true
			};
			var workflowControlSet = new WorkflowControlSet("wcs1");
			workflowControlSet.SetId(addPersonCommandDto.WorkflowControlSetId.Value);
			workflowControlSetRepository.Stub(x => x.Get(addPersonCommandDto.WorkflowControlSetId.Value)).Return(workflowControlSet);
			target.Handle(addPersonCommandDto);

			personRepository.AssertWasCalled(x => x.Add(Arg<Person>.Matches(p =>
				p.Name.FirstName == addPersonCommandDto.FirstName &&
				p.Name.LastName == addPersonCommandDto.LastName &&
				p.Email == addPersonCommandDto.Email &&
				p.EmploymentNumber == addPersonCommandDto.EmploymentNumber &&
				p.Note == addPersonCommandDto.Note &&
				p.FirstDayOfWeek == addPersonCommandDto.WorkWeekStart &&
				p.WorkflowControlSet.Id.Value == addPersonCommandDto.WorkflowControlSetId.Value &&
				p.PermissionInformation.DefaultTimeZone().Id == addPersonCommandDto.TimeZoneId &&
				p.PermissionInformation.CultureLCID() == addPersonCommandDto.CultureLanguageId &&
				p.PermissionInformation.UICultureLCID() == addPersonCommandDto.UICultureLanguageId &&
				p.IsDeleted == addPersonCommandDto.IsDeleted
				)));
			tenantDataManager.AssertWasCalled(x => x.SaveTenantData(Arg<TenantAuthenticationData>.Matches(t =>
				t.ApplicationLogonName == addPersonCommandDto.ApplicationLogonName &&
				t.Password == addPersonCommandDto.ApplicationLogOnPassword &&
				t.Identity == addPersonCommandDto.Identity
				)));

			unitOfWork.AssertWasCalled(x=>x.PersistAll());
			addPersonCommandDto.Result.AffectedItems.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotPersistPersonIfFailedToSaveTenantData()
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var tenantDataManager = MockRepository.GenerateMock<ITenantDataManager>();
			var workflowControlSetRepository = MockRepository.GenerateMock<IWorkflowControlSetRepository>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);
			tenantDataManager.Stub(x => x.SaveTenantData(Arg<TenantAuthenticationData>.Is.Anything))
				.Return(new SavePersonInfoResult
				{
					Success = false
				});
			var target = new AddPersonCommandHandler(personRepository, currentUnitOfWorkFactory, tenantDataManager, workflowControlSetRepository);

			var addPersonCommandDto = new AddPersonCommandDto
			{
				FirstName = "first",
				LastName = "last",
				Email = "test@teleopti.com",
				EmploymentNumber = "employmentNumber",
				Identity = "identity1",
				ApplicationLogonName = "logonname",
				ApplicationLogOnPassword = "password",
				Note = "note1",
				WorkWeekStart = DayOfWeek.Sunday,
				TimeZoneId = TimeZoneInfo.Utc.Id,
				CultureLanguageId = CultureInfo.CurrentCulture.LCID,
				UICultureLanguageId = CultureInfo.CurrentUICulture.LCID,
				IsDeleted = true
			};
			target.Handle(addPersonCommandDto);

			unitOfWork.AssertWasNotCalled(x=>x.PersistAll());
			addPersonCommandDto.Result.AffectedItems.Should().Be.EqualTo(0);
			addPersonCommandDto.Result.AffectedId.Should().Be.EqualTo(null);
		}

	}
}