<%@ Page Language="C#" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>
	<head>
		<title>All Tests</title>
		
		<link rel="stylesheet" href="../../../../Content/Scripts/qunit.css" type="text/css" media="screen" />
		<script type="text/javascript" src="../../../../Content/jquery/jquery-1.10.2.js"></script>
		<script type="text/javascript" src="../../../../Content/jqueryui/jquery-ui-1.10.2.custom.js"></script>
		<script type="text/javascript" src="../../../../Content/Scripts/qunit.js"></script>
		<script type="text/javascript" src="../../../../Content/Scripts/knockout-2.2.1.js"></script>
		<script type="text/javascript" src="../../../../Content/moment/moment.js"></script>
		<script type="text/javascript" src="jquery.ui.calendarselectable.js"></script>
		
        <%
            var version = DateTime.UtcNow.Ticks;
        %>
		
        <script type="text/javascript" src="Teleopti.MyTimeWeb.Common.js?bust=<%=version%>"></script>
        <script type="text/javascript" src="Teleopti.MyTimeWeb.Ajax.js?bust=<%=version%>"></script>
        <script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.js?bust=<%=version%>"></script>
        <script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.DayViewModel.js?bust=<%=version%>"></script>
        <script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel.js?bust=<%=version%>"></script>
        <script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.SelectionViewModel.js?bust=<%=version%>"></script>
        <script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.PreferencesAndScheduleViewModel.js?bust=<%=version%>"></script>
		<script type="text/javascript" src="Teleopti.MyTimeWeb.Portal.js?bust=<%=version%>"></script>
        <script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MonthViewModel.js?bust=<%=version%>"></script>

        <script type="text/javascript" src="Teleopti.MyTimeWeb.Schedule.MonthViewModel.Tests.js?bust=<%=version%>"></script>
        <script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.Tests.js?bust=<%=version%>"></script>
        <script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.DayViewModel.Tests.js?bust=<%=version%>"></script>
        <script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel.Tests.js?bust=<%=version%>"></script>
        <script type="text/javascript" src="Teleopti.MyTimeWeb.Preference.SelectionViewModel.Tests.js?bust=<%=version%>"></script>

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