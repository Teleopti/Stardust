using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.WcfHost.Ioc;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	[DomainTest]
	public class AddPersonCommandHandlerTest : IIsolateSystem, IExtendSystem
	{
		public FakePersonRepository PersonRepository;
		public FakeWorkflowControlSetRepository WorkflowControlSetRepository;
		public AddPersonCommandHandler Target;
		public PostHttpRequestFake PostHttpRequest;

		[Test]
		public void PersonIsAddedSuccessfully()
		{
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
			WorkflowControlSetRepository.Add(workflowControlSet);

			PostHttpRequest.SetReturnValue(new PersistPersonInfoResult{ApplicationLogonNameIsValid = true,IdentityIsValid = true,PasswordStrengthIsValid = true});
			
			Target.Handle(addPersonCommandDto);

			PersonRepository.LoadAll().Single(p =>
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
				((IDeleteTag)p).IsDeleted == addPersonCommandDto.IsDeleted
				).Should().Not.Be.Null();

			var credentials = PostHttpRequest.SentJson;
			credentials.Should().Contain(addPersonCommandDto.ApplicationLogonName);
			credentials.Should().Contain(addPersonCommandDto.ApplicationLogOnPassword);
			credentials.Should().Contain(addPersonCommandDto.Identity);
			addPersonCommandDto.Result.AffectedItems.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotPersistPersonIfFailedToSaveTenantData()
		{
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

			PostHttpRequest.SetReturnValue(new PersistPersonInfoResult { ApplicationLogonNameIsValid = false, IdentityIsValid = true, PasswordStrengthIsValid = true });

			Target.Handle(addPersonCommandDto);

			addPersonCommandDto.Result.AffectedItems.Should().Be.EqualTo(0);
			addPersonCommandDto.Result.AffectedId.Should().Be.EqualTo(null);
		}


		[Test]
		public void ShouldThrowIfNotNullUsernameAndNullPassword()
		{
			var addPersonCommandDto = new AddPersonCommandDto
			{
				FirstName = "first",
				LastName = "last",
				Email = "test@teleopti.com",
				EmploymentNumber = "employmentNumber",
				Identity = "identity1",
				ApplicationLogonName = "logonname",
				ApplicationLogOnPassword = null,
				Note = "note1",
				WorkWeekStart = DayOfWeek.Sunday,
				TimeZoneId = TimeZoneInfo.Utc.Id,
				CultureLanguageId = CultureInfo.CurrentCulture.LCID,
				UICultureLanguageId = CultureInfo.CurrentUICulture.LCID,
				IsDeleted = true
			};

			Assert.Throws<ArgumentException>(() => Target.Handle(addPersonCommandDto));
		}
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<AddPersonCommandHandler>();
			extend.AddModule(new AssemblerModule());
		}

		public void Isolate(IIsolate isolate)
		{

			isolate.UseTestDouble<ChangePasswordTenantClient>().For<IChangePasswordTenantClient>();
			isolate.UseTestDouble<TenantPeopleSaver>().For<ITenantPeopleSaver>();
			isolate.UseTestDouble<TenantDataManagerClient>().For<ITenantDataManagerClient>();
			isolate.UseTestDouble<TenantPeopleLoader>().For<ITenantPeopleLoader>();

			isolate.UseTestDouble<PostHttpRequestFake>().For<IPostHttpRequest>();
			isolate.UseTestDouble<GetHttpRequestFake>().For<IGetHttpRequest>();
			isolate.UseTestDouble<FakeCurrentTenantCredentials>().For<ICurrentTenantCredentials>();

		}
	}
}