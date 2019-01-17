<%@ Page Language="C#" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>

<head>
	<title>All Tests</title>

	<link rel="stylesheet" href="../../../../Content/Scripts/qunit.css" type="text/css" media="screen" />
	<style>
		#qunit-tests li li.fail {
				word-break: break-all;
				white-space: pre-wrap;
			}
		</style>
	<script type="text/javascript" src="../../../../Content/jquery/jquery-1.12.4.js"></script>
	<script type="text/javascript" src="../../../../Content/jqueryui/jquery-ui-1.10.2.custom.js"></script>
	<script type="text/javascript" src="../../../../Content/jqueryui/jquery-ui-1.10.2.slider.js"></script>
	<script type="text/javascript" src="../../../../Content/Scripts/qunit.js"></script>
	<script type="text/javascript" src="../../../../Content/Scripts/knockout-2.2.1.js"></script>
	<script type="text/javascript" src="../../../../Content/bootstrap/Scripts/bootstrap.js"></script>
	<script type="text/javascript" src="../../../../Content/moment/moment.js"></script>
	<script type="text/javascript" src="../../../../Content/moment/moment-with-locales.min.js"></script>
	<script type="text/javascript" src="../../../../Content/moment-timezone/moment-timezone-with-data.min.js"></script>

	<script type="text/javascript" src="../../../../Content/jalaali-calendar-datepicker/moment-jalaali.js"></script>
	<script type="text/javascript" src="../../../../Content/moment-datepicker/moment-datepicker.js"></script>
	<script type="text/javascript" src="../../../../Content/jalaali-calendar-datepicker/moment-jalaali-ext.js"></script>
	<script type="text/javascript" src="../../../../Content/jalaali-calendar-datepicker/moment-datepicker-jalaali-ext.js"></script>

	<script type="text/javascript" src="jquery.ui.calendarselectable.js"></script>
	<script type="text/javascript" src="../../../../Content/jquery-plugin/jquery.touchSwipe.min.js"></script>
	<script type="text/javascript" src="../../../../Content/signals/signals.min.js"></script>
	<script type="text/javascript" src="../../../../Content/signalr/jquery.signalR-2.3.0.min.js"></script>
	<script type="text/javascript" src="../../../../Content/signalr/broker-hubs.js"></script>

	<script type="text/javascript" src="../../../../Content/hasher/hasher.min.js"></script>
	<script type="text/javascript" src="../../../../Content/select2/select2.js"></script>
	<script type="text/javascript" src="qrcode.js"></script>

	<%
			var version = DateTime.UtcNow.Ticks;
		%>

	<script type="text/javascript" src="knockout.bindings.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Common.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Ajax.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.MessageBroker.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.List.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.RequestViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.ShiftTradeRequestDetailViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.ShiftTradeViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.DayViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.SelectionViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.PreferencesAndScheduleViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.MustHaveCountViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Portal.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.TimelineViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.LayerViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.AbsenceReportViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MonthViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.ProbabilityOptionViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.OvertimeAvailabilityViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.WeekViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.ProbabilityViewmodel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.ProbabilityModels.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.DayViewModel.js?bust=<%=version%>"></script>

	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MobileMonth.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MobileMonth.DataService.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MobileMonthViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.NewTeamSchedule.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.NewTeamSchedule.DataService.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.NewTeamScheduleViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Settings.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Settings.SettingsViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.AppGuide.WFMApp.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.AppGuide.WFMAppViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.StudentAvailability.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.StudentAvailability.DayViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.StudentAvailability.PeriodFeedbackViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.TeamScheduleViewModel.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.TeamScheduleUtility.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.AlertActivity.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Asm.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MobileStartDay.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MobileStartDay.DataService.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.RequestDetail.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.FakeData.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.AsmMessage.List.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Notifier.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.PollScheduleUpdates.js?bust=<%=version %>"></script>

	<!--Test files starting from here: Teleopti.MyTimeWeb.Test.js should be the first one cause there are some basic settings-->
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Test.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Common.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.PollScheduleUpdates.Tests.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.Probability.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.RequestDetail.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Settings.SettingsViewModel.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.AppGuide.WFMAppViewModel.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.ShiftTradeViewModel.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.WeekViewModel.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.DayViewModel.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.TimelineViewModel.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.ProbabilityModels.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.ProbabilityViewmodel.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.NewTeamSchedule.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.RequestViewModel.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.List.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MonthViewModel.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.DayViewModel.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.MustHaveCountViewModel.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.SelectionViewModel.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.StudentAvailability.PeriodFeedbackViewModel.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.TeamScheduleViewModel.Tests.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.ChooseHistoryViewModel.Tests.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.AlertActivity.Tests.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Asm.Tests.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Portal.Tests.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MobileStartDay.Tests.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel.Tests.js?bust=<%=version%>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MobileMonth.Tests.js?bust=<%=version%>">
	</script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel.Tests.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.AsmMessage.List.Tests.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.ShiftTradeRequestDetailViewModel.Tests.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel.js?bust=<%=version %>"></script>
	<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel.Tests.js?bust=<%=version %>"></script>

</head>

<body>
	<h1 id="qunit-header">All tests</h1>
	<h2 id="qunit-banner"></h2>
	<div id="qunit-testrunner-toolbar"></div>
	<h2 id="qunit-userAgent"></h2>
	<ol id="qunit-tests"></ol>
	<div id="qunit-fixture"></div>
</body>

</html>