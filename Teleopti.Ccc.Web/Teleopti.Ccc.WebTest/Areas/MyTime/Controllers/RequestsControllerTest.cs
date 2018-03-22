using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Areas.Requests.Core.Provider;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;
using Teleopti.Ccc.WebTest.Core.Requests.DataProvider;
using Teleopti.Ccc.WebTest.Core.Requests.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	[RequestsTest]
	public class RequestsControllerTest : ISetup
	{
		public RequestsController Target;
		public IPersonRequestRepository PersonRequestRepository;
		public FakeAbsenceRepository AbsenceRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakePersonRepository PersonRepository;
		public IRequestApprovalServiceFactory RequestApprovalServiceFactory;
		public IScheduleStorage ScheduleStorage;
		public ICurrentScenario CurrentScenario;
		public MutableNow Now;
		public IRequestCommandHandlingProvider CommandHandlingProvider;
		public FakePermissionProvider PermissionProvider;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<RequestsController>();
			system.UseTestDouble<FakeLinkProvider>().For<ILinkProvider>();
			system.UseTestDouble<FakePeopleForShiftTradeFinder>().For<IPeopleForShiftTradeFinder>();
			system.UseTestDouble<FakePermissionProvider>().For<IPermissionProvider>();
			system.UseTestDouble<FakePersonalSettingDataRepository>().For<IPersonalSettingDataRepository>();
			system.UseTestDouble(new FakeScenarioRepository(new Scenario("test") { DefaultScenario = true }))
				.For<IScenarioRepository>();
			system.UseTestDouble<RequestApprovalServiceFactory>().For<IRequestApprovalServiceFactory>();
			system.UseTestDouble<FakeLicensedFunctionProvider>().For<ILicensedFunctionsProvider>();
		}

		public void ShouldReturnRequestsPartialView()
		{
			//var target = new RequestsController(MockRepository.GenerateMock<IRequestsViewModelFactory>(), null, null, null, null,
			//	new FakePermissionProvider(), null, null, null, null, null);

			//var result = target.Index();

			//result.ViewName.Should().Be.EqualTo("RequestsPartial");
		}

		public void ShouldReturnViewModelForIndex()
		{
			//var viewModelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();
			//var target = new RequestsController(viewModelFactory, null, null, null, null, new FakePermissionProvider(), null,
			//	null, null, null, null);

			//viewModelFactory.Stub(x => x.CreatePageViewModel()).Return(new RequestsViewModel());

			//var result = target.Index();
			//var model = result.Model as RequestsViewModel;

			//model.Should().Not.Be.Null();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnViewModelForPaging()
		{
			var paging = new Paging();
			var filter = new RequestListFilter { HideOldRequest = false, IsSortByUpdateDate = true };

			var result = Target.Requests(paging, filter);

			((IEnumerable<RequestViewModel>)result.Data).Count().Should().Be(0);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldPersistTextRequestForm()
		{
			var form = new TextRequestForm();

			var result = Target.TextRequest(form);
			var data = result.Data as RequestViewModel;

			PersonRequestRepository.Find(Guid.Parse(data.Id)).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistShiftTradeRequest()
		{
			var personFrom = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			PersonRepository.Add(personFrom);
			LoggedOnUser.SetFakeLoggedOnUser(personFrom);

			var personTo = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			PersonRepository.Add(personTo);

			var form = new ShiftTradeRequestForm
			{
				Message = "test",
				Dates = new List<DateOnly> { DateOnly.Today },
				PersonToId = personTo.Id.Value
			};
			var result = Target.ShiftTradeRequest(form);
			var data = result.Data as RequestViewModel;

			PersonRequestRepository.Find(Guid.Parse(data.Id)).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistAbsenceRequestForm()
		{
			var absence = new Absence().WithId();
			AbsenceRepository.Add(absence);
			var form = new AbsenceRequestForm
			{
				AbsenceId = absence.Id.Value,
				Message = "test",
				Subject = "subject",
				Period = new DateTimePeriodForm
				{
					StartDate = DateOnly.Today,
					StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
					EndDate = DateOnly.Today,
					EndTime = new TimeOfDay(TimeSpan.FromHours(9))
				}
			};
			var result = Target.AbsenceRequest(form);
			var data = result.Data as RequestViewModel;

			PersonRequestRepository.Find(Guid.Parse(data.Id)).Should().Not.Be.Null();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnErrorFromAbsenceRequestFormWhenPersistError()
		{
			var person = PersonFactory.CreatePersonWithId(Guid.NewGuid());

			Now.Is(TimeZoneHelper.ConvertToUtc(DateOnly.Today.Date, person.PermissionInformation.DefaultTimeZone()));

			var workflowControlSet = new WorkflowControlSet();
			var absence = new Absence().WithId();
			AbsenceRepository.Add(absence);
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenRollingPeriod
			{
				Absence = absence,
				AbsenceRequestProcess = new GrantAbsenceRequest(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator(),
				BetweenDays = new MinMax<int>(0, 100),
				OpenForRequestsPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(10)),
				PersonAccountValidator = new AbsenceRequestNoneValidator()
			});
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Add(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);


			var form = new AbsenceRequestForm
			{
				AbsenceId = absence.Id.Value,
				Message = "test",
				Subject = "subject",
				Period = new DateTimePeriodForm
				{
					StartDate = DateOnly.Today,
					StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
					EndDate = DateOnly.Today,
					EndTime = new TimeOfDay(TimeSpan.FromHours(9))
				}
			};
			var result = Target.AbsenceRequest(form);

			var data = result.Data as RequestViewModel;
			var absenceRequest = PersonRequestRepository.Find(Guid.Parse(data.Id));
			absenceRequest.IsApproved.Should().Be(true);
			absenceRequest.Persisted();

			// update period
			form.EntityId = absenceRequest.Id;
			form.Period = new DateTimePeriodForm
			{
				StartDate = DateOnly.Today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(7)),
				EndDate = DateOnly.Today,
				EndTime = new TimeOfDay(TimeSpan.FromHours(9))
			};

			var context = new FakeHttpContext("/");
			context.SetResponse(new FakeHttpResponse());
			Target.ControllerContext = new ControllerContext(context, new RouteData(), Target);

			result = Target.AbsenceRequest(form);
			var modelStateResult = result.Data as ModelStateResult;
			modelStateResult.Should().Not.Be.Null();
			modelStateResult.Errors.ElementAt(0).Should().Be("Your request state has been changed, you cannot update/delete it.");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnErrorFromAbsenceRequestFormWhenModelError()
		{
			var form = new AbsenceRequestForm();

			var validationResults = new List<ValidationResult>();
			Validator.TryValidateObject(form, new ValidationContext(form),
				validationResults);

			validationResults.Count.Should().Be(1);
			validationResults[0].ErrorMessage.Should().Be("Missing subject");
		}

		[Test]
		public void ShouldReturnErrorMessageOnInvalidModelFromTextRequest()
		{
			var sb = new StringBuilder(2001);
			for (int i = 0; i < 2001; i++)
			{
				sb.Append("1");
			}
			var form = new AbsenceRequestForm
			{
				Subject = "test",
				Message = sb.ToString()
			};

			var validationResults = new List<ValidationResult>();
			Validator.TryValidateObject(form, new ValidationContext(form),
				validationResults, true);
			validationResults.Count.Should().Be(1);
			validationResults[0].ErrorMessage.Should().Be("Message too long");
		}

		[Test]
		public void ShouldReturnErrorMessageOnInvalidModelFromShiftTradeRequest()
		{
			var sb = new StringBuilder(2001);
			for (int i = 0; i < 2001; i++)
			{
				sb.Append("1");
			}

			var form = new ShiftTradeRequestForm
			{
				Subject = "test",
				Message = sb.ToString()
			};

			var validationResults = new List<ValidationResult>();
			Validator.TryValidateObject(form, new ValidationContext(form),
				validationResults, true);
			validationResults.Count.Should().Be(1);
			validationResults[0].ErrorMessage.Should().Be("Message too long");
		}

		[Test]
		public void ShouldDeleteTextRequest()
		{
			var form = new TextRequestForm();
			var result = Target.TextRequest(form);
			var data = result.Data as RequestViewModel;

			Target.RequestDelete(Guid.Parse(data.Id));
			PersonRequestRepository.Find(Guid.Parse(data.Id)).Should().Be(null);
		}

		[Test]
		public void ShouldGetRequestById()
		{
			var form = new TextRequestForm { Subject = "text" };
			var result = Target.TextRequest(form);
			var data = result.Data as RequestViewModel;

			var requestViewModel = Target.RequestDetail(Guid.Parse(data.Id)).Data as RequestViewModel;
			requestViewModel.Should().Not.Be(null);
			requestViewModel.Subject.Should().Be("text");
		}

		[Test]
		public void ShouldGetShiftTradePeriodInformation()
		{
			var personFrom = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			PersonRepository.Add(personFrom);
			LoggedOnUser.SetFakeLoggedOnUser(personFrom);
			Now.Is(TimeZoneHelper.ConvertToUtc(DateOnly.Today.Date, LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone()));

			var workflowControlSet = new WorkflowControlSet
			{
				AnonymousTrading = true,
				ShiftTradeOpenPeriodDaysForward = new MinMax<int>(1, 10)
			};
			personFrom.WorkflowControlSet = workflowControlSet;

			var shiftTradeRequestPeriod = Target.ShiftTradeRequestPeriod();
			var data = (ShiftTradeRequestsPeriodViewModel)shiftTradeRequestPeriod.Data;

			data.HasWorkflowControlSet.Should().Be.EqualTo(true);
			data.MiscSetting.AnonymousTrading.Should().Be.EqualTo(true);
			data.OpenPeriodRelativeStart.Should().Be.EqualTo(1);
			data.OpenPeriodRelativeEnd.Should().Be.EqualTo(10);
			data.NowYear.Should().Be.EqualTo(DateOnly.Today.Date.Year);
			data.NowMonth.Should().Be.EqualTo(DateOnly.Today.Date.Month);
			data.NowDay.Should().Be.EqualTo(DateOnly.Today.Date.Day);
		}

		[Test]
		public void ShouldGetIdOfTeamIBelongTo()
		{
			var today = DateOnly.Today;
			var personFrom = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			PersonRepository.Add(personFrom);
			LoggedOnUser.SetFakeLoggedOnUser(personFrom);

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(today).WithId();
			var myTeam = TeamFactory.CreateTeam("Team", "Site").WithId();
			personPeriod.Team = myTeam;
			personFrom.AddPersonPeriod(personPeriod);
			var result = Target.ShiftTradeRequestMyTeam(today);

			var data = (string)result.Data;
			data.Should().Be.EqualTo(myTeam.Id.Value.ToString());
		}

		[Test]
		public void ShouldGetIdOfSiteIBelongTo()
		{
			var today = DateOnly.Today;
			var personFrom = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			PersonRepository.Add(personFrom);
			LoggedOnUser.SetFakeLoggedOnUser(personFrom);

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(today).WithId();
			var myTeam = new Team().WithId();
			personPeriod.Team = myTeam;

			var site = new Site("site").WithId();
			site.AddTeam(myTeam);
			personFrom.AddPersonPeriod(personPeriod);

			var result = Target.ShiftTradeRequestMySite(today);

			var data = (string)result.Data;
			data.Should().Be.EqualTo(myTeam.Site.Id.Value.ToString());
		}

		[Test]
		public void ShouldApproveShiftTradeRequest()
		{
			var personFrom = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			PersonRepository.Add(personFrom);
			LoggedOnUser.SetFakeLoggedOnUser(personFrom);

			var personTo = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			var applicationRole = ApplicationRoleFactory.CreateRole("test", "test");
			applicationRole.AddApplicationFunction(ApplicationFunctionFactory.CreateApplicationFunction(DefinedRaptorApplicationFunctionPaths.ViewSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunctionFactory.CreateApplicationFunction(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb));
			applicationRole.AvailableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.Everyone }; ;
			personTo.PermissionInformation.AddApplicationRole(applicationRole);
			PersonRepository.Add(personTo);

			var form = new ShiftTradeRequestForm
			{
				Message = "test",
				Dates = new List<DateOnly> { DateOnly.Today },
				PersonToId = personTo.Id.Value
			};
			var result = Target.ShiftTradeRequest(form);
			var data = result.Data as RequestViewModel;

			var approveResult = Target.ApproveShiftTrade(new ShiftTradeRequestReplyForm { ID = Guid.Parse(data.Id), Message = "" });
			var approveResultData = approveResult.Data as RequestViewModel;
			approveResultData.IsDenied.Should().Be(false);
			approveResultData.IsNew.Should().Be(true);
			approveResultData.Status.Should().Be(Resources.WaitingThreeDots);
		}

		[Test]
		public void ShouldDenyShiftTradeRequest()
		{
			var personFrom = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			PersonRepository.Add(personFrom);
			LoggedOnUser.SetFakeLoggedOnUser(personFrom);

			var personTo = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			var applicationRole = ApplicationRoleFactory.CreateRole("test", "test");
			applicationRole.AddApplicationFunction(ApplicationFunctionFactory.CreateApplicationFunction(DefinedRaptorApplicationFunctionPaths.ViewSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunctionFactory.CreateApplicationFunction(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb));
			applicationRole.AvailableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.Everyone };
			personTo.PermissionInformation.AddApplicationRole(applicationRole);
			PersonRepository.Add(personTo);

			var form = new ShiftTradeRequestForm
			{
				Message = "test",
				Dates = new List<DateOnly> { DateOnly.Today },
				PersonToId = personTo.Id.Value
			};
			var result = Target.ShiftTradeRequest(form);
			var data = result.Data as RequestViewModel;

			LoggedOnUser.SetFakeLoggedOnUser(personTo);

			var denyResult = Target.DenyShiftTrade(new ShiftTradeRequestReplyForm { ID = Guid.Parse(data.Id), Message = "" });
			var denyResultData = denyResult.Data as RequestViewModel;
			denyResultData.IsDenied.Should().Be(true);
		}

		[Test]
		public void ShouldGetShiftTradeDetails()
		{
			var personFrom = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			personFrom.SetName(new Name("xxx", "personFrom"));
			PersonRepository.Add(personFrom);
			LoggedOnUser.SetFakeLoggedOnUser(personFrom);

			var personTo = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			personTo.SetName(new Name("yyy", "personTo"));
			var applicationRole = ApplicationRoleFactory.CreateRole("test", "test");
			applicationRole.AddApplicationFunction(ApplicationFunctionFactory.CreateApplicationFunction(DefinedRaptorApplicationFunctionPaths.ViewSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunctionFactory.CreateApplicationFunction(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb));
			applicationRole.AvailableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.Everyone }; ;
			personTo.PermissionInformation.AddApplicationRole(applicationRole);
			PersonRepository.Add(personTo);

			var form = new ShiftTradeRequestForm
			{
				Message = "test",
				Dates = new List<DateOnly> { DateOnly.Today },
				PersonToId = personTo.Id.Value
			};
			var result = Target.ShiftTradeRequest(form);
			var data = result.Data as RequestViewModel;

			var shiftTradeRequestSwapDetails = (IList<ShiftTradeSwapDetailsViewModel>)Target.ShiftTradeRequestSwapDetails(Guid.Parse(data.Id)).Data;
			Assert.That(shiftTradeRequestSwapDetails.First().From.Name, Is.EqualTo("xxx personFrom"));
			Assert.That(shiftTradeRequestSwapDetails.First().To.Name, Is.EqualTo("yyy personTo"));
		}

		[Test]
		public void ReSendShiftTrade_WhenStatusIsReffered_ShouldSetTheShiftTradeStatusToOkByMe()
		{
			var personFrom = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			personFrom.SetName(new Name("xxx", "personFrom"));
			PersonRepository.Add(personFrom);
			LoggedOnUser.SetFakeLoggedOnUser(personFrom);

			var personTo = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			personTo.SetName(new Name("yyy", "personTo"));
			var applicationRole = ApplicationRoleFactory.CreateRole("test", "test");
			applicationRole.AddApplicationFunction(ApplicationFunctionFactory.CreateApplicationFunction(DefinedRaptorApplicationFunctionPaths.ViewSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunctionFactory.CreateApplicationFunction(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb));
			applicationRole.AvailableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.Everyone }; ;
			personTo.PermissionInformation.AddApplicationRole(applicationRole);
			PersonRepository.Add(personTo);

			var form = new ShiftTradeRequestForm
			{
				Message = "test",
				Dates = new List<DateOnly> { DateOnly.Today },
				PersonToId = personTo.Id.Value
			};
			var result = Target.ShiftTradeRequest(form);
			var data = result.Data as RequestViewModel;

			var shiftTradeRequest = PersonRequestRepository.Find(Guid.Parse(data.Id));
			(shiftTradeRequest.Request as IShiftTradeRequest).Refer(new PersonRequestAuthorizationCheckerForTest());

			Target.ResendShiftTrade(Guid.Parse(data.Id));
			(shiftTradeRequest.Request as IShiftTradeRequest).GetShiftTradeStatus(new EmptyShiftTradeRequestChecker()).Should()
				.Be(ShiftTradeStatus.OkByMe);
		}

		[Test]
		public void ShouldReturnPersonalAccountPermission()
		{
			var result = Target.PersonalAccountPermission();
			result.Data.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldGetAnonymousTrading()
		{
			var personFrom = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			PersonRepository.Add(personFrom);
			LoggedOnUser.SetFakeLoggedOnUser(personFrom);

			var workflowControlSet = new WorkflowControlSet
			{
				AnonymousTrading = true,
				ShiftTradeOpenPeriodDaysForward = new MinMax<int>(1, 10)
			};
			personFrom.WorkflowControlSet = workflowControlSet;

			var result = Target.GetShiftTradeRequestMiscSetting(Guid.Empty);
			var data = (ShiftTradeRequestMiscSetting)result.Data;
			data.AnonymousTrading.Should().Be.True();
		}

		[Test]
		public void ShouldAllowCancelAbsenceRequest()
		{
			Now.Is(new DateTime(2016,3,1,0,0,0,DateTimeKind.Utc));

			var person = PersonFactory.CreatePerson("Bill", "Bloggins").WithId();
			var data = doCancelAbsenceRequestMyTimeSpecificValidation(person, new DateTimePeriod(2016, 03, 02, 2016, 03, 03));
			data.ErrorMessages.Should().Be.Empty();
		}

		[Test]
		public void ShouldValidatePermissionForCancelAbsenceRequest()
		{
			var person = PersonFactory.CreatePerson("Bill", "Bloggins").WithId();
			var data = doCancelAbsenceRequestMyTimeSpecificValidation(person, new DateTimePeriod(2016, 03, 02, 2016, 03, 03),
				false);
			data.ErrorMessages.Should().Contain(Resources.InsufficientPermission);
		}

		[Test]
		public void ShouldValidateCancellationThresholdForCancelAbsenceRequest()
		{
			var today = DateTime.Today.ToUniversalTime();
			Now.Is(today.Date.AddDays(12));

			var person = PersonFactory.CreatePerson("Bill", "Bloggins").WithId();

			var data = doCancelAbsenceRequestMyTimeSpecificValidation(person, new DateTimePeriod(today, today.AddDays(1)),true,10);

			var workflowControlSet = person.WorkflowControlSet;
			data.ErrorMessages.Should()
				.Contain(string.Format(Resources.AbsenceRequestCancellationThresholdExceeded,
					workflowControlSet.AbsenceRequestCancellationThreshold));
		}

		[Test]
		public void ShouldHandleNullTeamIdListInFilter()
		{
			shouldHandleTeamIdsCorrectly(null);
		}

		[Test]
		public void ShouldHandleInvalidGuidInTeamIdListInFilter()
		{
			var teamIds = $"abc, {Guid.NewGuid()},{Guid.NewGuid()} , {Guid.NewGuid()} ,123";
			shouldHandleTeamIdsCorrectly(teamIds);
		}

		private  void shouldHandleTeamIdsCorrectly(string teamIds)
		{
			//var timeFilterHelper = MockRepository.GenerateMock<ITimeFilterHelper>();
			//timeFilterHelper.Stub(x => x.GetFilter(new DateOnly(), "", "", false, false))
			//	.IgnoreArguments().Return(null);

			//var viewModelFactory = MockRepository.GenerateMock<IRequestsShiftTradeScheduleViewModelFactory>();
			//viewModelFactory.Stub(x => x.CreateViewModel(new ShiftTradeScheduleViewModelData()))
			//	.IgnoreArguments().Return(new ShiftTradeScheduleViewModel());

			//var toggleManager = new TrueToggleManager();

			//using (var target = new RequestsController(null, null, null, null, null, null,
			//	timeFilterHelper, toggleManager, viewModelFactory, null, null))
			//{
			var filter = new ScheduleFilter
			{
				TeamIds = teamIds
			};

			var result = Target.ShiftTradeRequestSchedule(new DateOnly(DateTime.Now), filter,
				new Paging { Skip = 0, Take = 1, TotalCount = 10 });

			result.Should().Not.Be.Null();
		}

		private RequestCommandHandlingResult doCancelAbsenceRequestMyTimeSpecificValidation(IPerson person, DateTimePeriod period, bool hasPermission = true, int? absenceRequestCancellationThreshold = null)
		{
			if (hasPermission)
			{
				PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.MyTimeCancelRequest,
					new DateOnly(period.StartDateTime), person);
			}

			var workflowControlSet =
				new WorkflowControlSet {AbsenceRequestCancellationThreshold = absenceRequestCancellationThreshold};
			var absence = AbsenceFactory.CreateAbsence("Holiday").WithId();
			AbsenceRepository.Add(absence);
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenRollingPeriod
			{
				Absence = absence,
				AbsenceRequestProcess = new PendingAbsenceRequest(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator(),
				BetweenDays = new MinMax<int>(0, 100),
				OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(period.StartDateTime.Date.AddDays(-1)), new DateOnly(period.EndDateTime.Date.AddDays(1))),
				PersonAccountValidator = new AbsenceRequestNoneValidator()
			});
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Add(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var result = Target.AbsenceRequest(new AbsenceRequestForm
			{
				AbsenceId = absence.Id.Value,
				Period = new DateTimePeriodForm
				{
					StartDate = new DateOnly(period.StartDateTime.Date),
					StartTime = new TimeOfDay(period.StartDateTime.TimeOfDay),
					EndDate = new DateOnly(period.EndDateTime.Date),
					EndTime = new TimeOfDay(period.EndDateTime.TimeOfDay)
				},
				Subject = "test"
			});
			var data = result.Data as RequestViewModel;
			var requestId = Guid.Parse(data.Id);

			CommandHandlingProvider.ApproveRequests(new[] {requestId}, string.Empty);

			var cancelRequestResult = Target.CancelRequest(requestId);
			return (RequestCommandHandlingResult)cancelRequestResult.Data;
		}

	}
}