using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Requests.Controller;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Web.Areas.MyTime.Core;

namespace Teleopti.Ccc.WebTest.Areas.Requests.Controller
{
	[TestFixture]
	[DomainTest]
	[WebTest]
	public class RequestsControllerTest: IIsolateSystem
	{
		public RequestsController Target;
		public FakeToggleManager ToggleManager;
		public FakePermissions Permissions;
		public FakePermissionProvider PermissionProvider;
		public ICurrentScenario CurrentScenario;
		public FakeDatabase Database;
		public FakePersonRepository PersonRepository;
		public FakePersonFinderReadOnlyRepository PersonFinderReadOnlyRepository;
		public FakeGroupingReadOnlyRepository GroupingReadOnlyRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeLicenseAvailability LicenseAvailability;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeToggleManager>().For<IToggleManager>();
			isolate.UseTestDouble<FakePermissions>().For<IAuthorization>();
			isolate.UseTestDouble<FakePermissionProvider>().For<IPermissionProvider>();
			isolate.UseTestDouble<FakePersonalSettingDataRepository>().For<IPersonalSettingDataRepository>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>(); 
			isolate.UseTestDouble<FakeLicenseAvailability>().For<ILicenseAvailability>();
			isolate.UseTestDouble(new FakeScenarioRepository(new Scenario("test") {DefaultScenario = true}))
				.For<IScenarioRepository>();
		}

		[Test]
		public void ShouldGetShiftTradeScheduleViewModelWithTimeLine()
		{
			var date = new DateTime(2018, 11, 21);
			var input = setupShiftTradeScheduleData(date);

			var result = Target.GetShiftTradeSchedules(input);

			result.TimeLine.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldGetShiftTradeScheduleViewModelWithCorrectStartPositionPercentageInPeriods()
		{
			var date = new DateTime(2018, 11, 21);
			var input = setupShiftTradeScheduleData(date);

			var result = Target.GetShiftTradeSchedules(input);

			result.PersonFromSchedule.Periods.First().StartPositionPercentage.Should().Not.Be.EqualTo(0.0);

			result.PersonToSchedule.Periods.First().StartPositionPercentage.Should().Not.Be.EqualTo(0.0);
		}

		[Test]
		public void ShouldGetShiftTradeScheduleViewModel()
		{
			var date = new DateTime(2018, 11, 21);
			var input = setupShiftTradeScheduleData(date);

			var result = Target.GetShiftTradeSchedules(input);

			result.PersonFromSchedule.Should().Not.Be.Null();
			result.PersonToSchedule.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldGetLicenseAvailability()
		{
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.WebOvertimeRequest);
			LicenseAvailability.HasLicense(DefinedLicenseOptionPaths.TeleoptiWfmOvertimeRequests);
			LicenseAvailability.HasLicense(DefinedLicenseOptionPaths.TeleoptiCccShiftTrader);
			var result = Target.GetLicenseAvailability();

			result.IsOvertimeRequestEnabled.Should().Be.True();
			result.IsShiftTradeRequestEnabled.Should().Be.True();
		}

		[Test]
		public void ShouldGetRequests()
		{
			var date = new DateTime(2018, 11, 21);
			var input = setupData(date);

			var result = Target.GetRequests(input);
			result.Requests.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetShift()
		{
			var date = new DateTime(2018, 11, 21);
			var input = setupData(date);

			var result = Target.GetRequests(input);
			result.Requests.First().Shifts.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetShiftPeriods()
		{
			var date = new DateTime(2018, 11, 21);
			var input = setupData(date);

			LoggedOnUser.SetDefaultTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo());

			var result = Target.GetRequests(input);
			result.Requests.First().Shifts.Count().Should().Be.EqualTo(1);
			result.Requests.First().Shifts.First().Periods.First().StartTime.Should().Be.EqualTo(date.AddHours(1));
			result.Requests.First().Shifts.First().Periods.First().EndTime.Should().Be.EqualTo(date.AddHours(10));
			result.Requests.First().Shifts.First().Periods.First().StartPositionPercentage.Should().Be.EqualTo(0.0);
			result.Requests.First().Shifts.First().Periods.First().EndPositionPercentage.Should().Be.EqualTo(1.0);
		}

		[Test]
		public void ShouldGetShiftIsNotDayOff()
		{
			var date = new DateTime(2018, 11, 21);
			var input = setupData(date);
			
			var result = Target.GetRequests(input);
			result.Requests.First().Shifts.First().IsDayOff.Should().Be.False();
		}

		[Test]
		public void ShouldGetBelongsToDate()
		{
			var expactedDate = new DateTime(2018, 11, 26, 8, 0, 0, DateTimeKind.Utc);
			var input = setupData(expactedDate, new List<DateTimePeriod>{new DateTimePeriod(expactedDate, expactedDate.AddHours(9))});

			LoggedOnUser.SetDefaultTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo());

			var result = Target.GetRequests(input);
			result.Requests.First().Shifts.First().BelongsToDate.Should().Be.EqualTo(expactedDate.Date.AddHours(1));
		}

		[Test]
		public void ShouldGetShiftCategory()
		{
			var expactedDate = new DateTime(2018, 11, 26);
			var input = setupData(expactedDate);

			var result = Target.GetRequests(input);
			result.Requests.First().Shifts.First().IsNotScheduled.Should().Be.False();
		}

		[Test]
		public void ShouldGetShiftCategoryName()
		{
			var expactedDate = new DateTime(2018, 11, 26);
			var expactedName = "blabla";
			var input = setupDataWithShiftCategory(expactedDate, expactedName, "bla", Color.AliceBlue);

			var result = Target.GetRequests(input);
			result.Requests.First().Shifts.First().ShiftCategory.Name.Should().Be.EqualTo(expactedName);
		}

		[Test]
		public void ShouldGetShiftCategoryShortName()
		{
			var expactedDate = new DateTime(2018, 11, 26);
			var expactedShortName = "blabla";
			var input = setupDataWithShiftCategory(expactedDate, "bla", expactedShortName, Color.AliceBlue);

			var result = Target.GetRequests(input);
			result.Requests.First().Shifts.First().ShiftCategory.ShortName.Should().Be.EqualTo(expactedShortName);
		}

		[Test]
		public void ShouldGetShiftCategoryColor()
		{
			var expactedDate = new DateTime(2018, 11, 26);
			var expactedColor = Color.Aqua;
			var input = setupDataWithShiftCategory(expactedDate, "bla", "ha", expactedColor);

			var result = Target.GetRequests(input);
			result.Requests.First().Shifts.First().ShiftCategory.DisplayColor.Should().Be.EqualTo(expactedColor.ToHtml());
		}

		[Test]
		public void ShouldGetAbsenceRequestsWithMultipleShifts()
		{
			var date = new DateTime(2018, 11, 21);
			var periods = new List<DateTimePeriod> { new DateTimePeriod(date.AddHours(8).Utc(), date.AddHours(17).Utc()), new DateTimePeriod(date.AddDays(1).AddHours(8).Utc(), date.AddDays(1).AddHours(17).Utc()) };
			var input = setupData(date, periods, date.AddDays(2));

			var result = Target.GetRequests(input);
			result.Requests.First().Shifts.Count().Should().Equals(2);
		}

		[Test]
		public void ShouldHasPermissionsWhenToggleOff()
		{
			ToggleManager.Disable(Toggles.WFM_Request_View_Permissions_77731);

			var result = Target.GetRequestsPermissions();
			
			result.Content.HasApproveOrDenyPermission.Should().Be.True();
			result.Content.HasCancelPermission.Should().Be.True();
			result.Content.HasEditSiteOpenHoursPermission.Should().Be.True();
			result.Content.HasReplyPermission.Should().Be.True();
		}

		[Test]
		public void ShouldHasApproveOrDenyPermission()
		{
			ToggleManager.Enable(Toggles.WFM_Request_View_Permissions_77731);
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.WebApproveOrDenyRequest);

			var result = Target.GetRequestsPermissions();

			result.Content.HasApproveOrDenyPermission.Should().Be.True();
		}

		[Test]
		public void ShouldNotHasApproveOrDenyPermission()
		{
			ToggleManager.Enable(Toggles.WFM_Request_View_Permissions_77731);

			var result = Target.GetRequestsPermissions();

			result.Content.HasApproveOrDenyPermission.Should().Be.False();
		}

		[Test]
		public void ShouldHasCancelPermission()
		{
			ToggleManager.Enable(Toggles.WFM_Request_View_Permissions_77731);
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.WebCancelRequest);

			var result = Target.GetRequestsPermissions();

			result.Content.HasCancelPermission.Should().Be.True();
		}

		[Test]
		public void ShouldNotHasCancelPermission()
		{
			ToggleManager.Enable(Toggles.WFM_Request_View_Permissions_77731);

			var result = Target.GetRequestsPermissions();

			result.Content.HasCancelPermission.Should().Be.False();
		}

		[Test]
		public void ShouldHasReplyPermission()
		{
			ToggleManager.Enable(Toggles.WFM_Request_View_Permissions_77731);
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.WebReplyRequest);

			var result = Target.GetRequestsPermissions();

			result.Content.HasReplyPermission.Should().Be.True();
		}

		[Test]
		public void ShouldNotHasReplyPermission()
		{
			ToggleManager.Enable(Toggles.WFM_Request_View_Permissions_77731);

			var result = Target.GetRequestsPermissions();

			result.Content.HasReplyPermission.Should().Be.False();
		}

		[Test]
		public void ShouldHasEditSiteOpenHoursPermission()
		{
			ToggleManager.Enable(Toggles.WFM_Request_View_Permissions_77731);
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.WebEditSiteOpenHours);

			var result = Target.GetRequestsPermissions();

			result.Content.HasEditSiteOpenHoursPermission.Should().Be.True();
		}

		[Test]
		public void ShouldNotHasEditSiteOpenHoursPermission()
		{
			ToggleManager.Enable(Toggles.WFM_Request_View_Permissions_77731);

			var result = Target.GetRequestsPermissions();

			result.Content.HasEditSiteOpenHoursPermission.Should().Be.False();
		}

		private AllRequestsFormData setupDataWithShiftCategory(DateTime date, string name, string shortName, Color color)
		{
			var scenarioId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();

			CurrentScenario.Current().WithId(scenarioId);

			Database.WithPerson(personId)
				.WithScenario(scenarioId)
				.WithTeam(teamId, "myTeam")
				.WithPeriod(DateOnly.MinValue.ToString())
				.WithShiftCategory(null, name, shortName, color)
				.WithSchedule(date.Date.AddHours(8).ToString(), date.Date.AddHours(17).ToString())
				.WithAbsenceRequest(personId, date.ToString());

			setUpPersonRelatedInfo(personId);
			setUpLogonUser();

			return getInputForm(teamId, date, null);
		}

		private AllRequestsFormData setupData
			(DateTime requestStartTime, IEnumerable<DateTimePeriod> schedulesPeriods = null, DateTime? requestEndTime = null)
		{
			var scenarioId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();

			CurrentScenario.Current().WithId(scenarioId);

			Database.WithPerson(personId)
				.WithScenario(scenarioId)
				.WithTeam(teamId, "myTeam")
				.WithPeriod(DateOnly.MinValue.ToString())
				.WithSchedules(schedulesPeriods ?? new List<DateTimePeriod> { new DateTimePeriod(requestStartTime.AddHours(8).Utc(), requestStartTime.AddHours(17).Utc()) })
				.WithAbsenceRequest(personId, requestStartTime, requestEndTime.HasValue ? requestEndTime.Value : requestStartTime);

			setUpPersonRelatedInfo(personId);
			setUpLogonUser();

			return getInputForm(teamId, requestStartTime, requestEndTime);
		}

		private ShiftTradeScheduleForm setupShiftTradeScheduleData(DateTime date)
		{
			var scenarioId = Guid.NewGuid();
			var personFromId = Guid.NewGuid();
			var personToId = Guid.NewGuid();

			CurrentScenario.Current().WithId(scenarioId);

			Database.WithPerson(personFromId)
				.WithScenario(scenarioId)
				.WithPeriod(date.ToDateOnly().ToString())
				.WithSchedule(date.Date.AddHours(9).ToString(), date.Date.AddHours(18).ToString())
				.WithPerson(personToId)
				.WithScenario(scenarioId)
				.WithPeriod(date.ToDateOnly().ToString())
				.WithSchedule(date.Date.AddHours(8).ToString(), date.Date.AddHours(17).ToString());

			setUpPersonRelatedInfo(personFromId);
			setUpLogonUser();


			return new ShiftTradeScheduleForm
			{
				PersonFromId = personFromId,
				PersonToId = personToId,
				RequestDate = date
			};
		}

		private AllRequestsFormData getInputForm(Guid teamId, DateTime start, DateTime? end)
		{
			return new AllRequestsFormData
			{
				StartDate = new DateOnly(start).AddDays(-1),
				EndDate = end.HasValue ? new DateOnly(end.Value).AddDays(1) : new DateOnly(start).AddDays(1),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>(),
				SelectedGroupIds = new[] { teamId.ToString() }
			};
		}

		private void setUpPersonRelatedInfo(Guid personId)
		{
			var person = PersonRepository.Get(personId);

			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			PersonFinderReadOnlyRepository.Has(person);
			GroupingReadOnlyRepository.Has(new ReadOnlyGroupDetail { PersonId = personId });
		}

		private void setUpLogonUser()
		{
			var logonUser = PersonFactory.CreatePersonWithGuid("logon", "user");
			LoggedOnUser.SetFakeLoggedOnUser(logonUser);
			LoggedOnUser.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			setLogonUserPermissions();
			PersonFinderReadOnlyRepository.Has(logonUser);
		}

		private void setLogonUserPermissions()
		{
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.WebRequests);
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules);
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
			var role = ApplicationRoleFactory.CreateRole("test", "test");
			role.AddApplicationFunction(ApplicationFunctionFactory.CreateApplicationFunction(DefinedRaptorApplicationFunctionPaths.WebRequests));
			role.AvailableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.Everyone };
			LoggedOnUser.CurrentUser().PermissionInformation.AddApplicationRole(role);
		}
	}
}
