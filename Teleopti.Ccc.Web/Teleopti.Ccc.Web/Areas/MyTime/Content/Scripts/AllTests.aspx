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
		<script type="text/javascript" src="../../../../Content/Scripts/qunit.js"></script>
		<script type="text/javascript" src="../../../../Content/Scripts/knockout-2.2.1.js"></script>
		<script type="text/javascript" src="../../../../Content/moment/moment.js"></script>
		<script type="text/javascript" src="../../../../Content/moment/moment-with-locales.min.js"></script>

		<script type="text/javascript" src="../../../../Content/jalaali-calendar-datepicker/moment-jalaali.js"></script>
		<script type="text/javascript" src="../../../../Content/moment-datepicker/moment-datepicker.js"></script>
		<script type="text/javascript" src="../../../../Content/jalaali-calendar-datepicker/moment-jalaali-ext.js"></script>
		<script type="text/javascript" src="../../../../Content/jalaali-calendar-datepicker/moment-datepicker-jalaali-ext.js"></script>

		<script type="text/javascript" src="jquery.ui.calendarselectable.js"></script>
		<script type="text/javascript" src="../../../../Content/jquery-plugin/jquery.touchSwipe.min.js"></script>

		<%
		    var version = DateTime.UtcNow.Ticks;
		%>
		
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Common.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Ajax.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.List.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.RequestViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.ShiftTradeRequestDetailViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.ShiftTradeViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MonthViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.DayViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.SelectionViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.PreferencesAndScheduleViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Portal.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.AbsenceReportViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MonthViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.ProbabilityOptionViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.OvertimeAvailabilityViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.WeekViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.ProbabilityViewmodel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.ProbabilityModels.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.Viewmodels.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MobileDayViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Settings.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Settings.SettingsViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.StudentAvailability.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.StudentAvailability.DayViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.StudentAvailability.PeriodFeedbackViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.TeamScheduleViewModel.js?bust=<%=version %>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.TeamScheduleUtility.js?bust=<%=version %>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.AlertActivity.js?bust=<%=version %>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Test.js?bust=<%=version %>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MobileDay.js?bust=<%=version %>"></script>


		
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Common.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Settings.SettingsViewModel.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.ShiftTradeViewModel.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.WeekViewModel.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.Viewmodels.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.ProbabilityViewmodel.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MobileDayViewModel.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.RequestViewModel.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.List.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MonthViewModel.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.DayViewModel.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.SelectionViewModel.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.StudentAvailability.PeriodFeedbackViewModel.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel.Tests.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.TeamScheduleViewModel.Tests.js?bust=<%=version %>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Request.ChooseHistoryViewModel.Tests.js?bust=<%=version %>"></script>		
		<script type="text/javascript" src="Teleopti.MyTimeWeb.AlertActivity.Tests.js?bust=<%=version %>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Portal.Tests.js?bust=<%=version %>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MobileDay.Tests.js?bust=<%=version %>"></script>
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