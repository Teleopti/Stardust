<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Teleopti.Ccc.Domain.Security.Principal" %>
<%
var identity = (ITeleoptiIdentity) CurrentTeleoptiPrincipal.Make().Current().Identity; 
%>
<!DOCTYPE html>
<!--[if lt IE 7]>      <html class="no-js lt-ie9 lt-ie8 lt-ie7"> <![endif]-->
<!--[if IE 7]>         <html class="no-js lt-ie9 lt-ie8"> <![endif]-->
<!--[if IE 8]>         <html class="no-js lt-ie9"> <![endif]-->
<!--[if gt IE 8]><!--> <html class="no-js"> <!--<![endif]-->
<head>
	<meta charset="utf-8" />
	<meta http-equiv="X-UA-Compatible" content="IE=Edge;IE=8;FF=3;OtherUA=4,chrome=1" />
	<title>Teleopti WFM Health Check Tool</title>
	<meta name="description" content="" />
	<meta name="viewport" content="width=device-width,initial-scale=1.0" />
		
	<link rel="stylesheet" href="<%= Url.Content("~/Content/bootstrap/Content/bootstrap.css") %>" />
	<link rel="stylesheet" href="<%= Url.Content("~/Content/bootstrap/Content/bootstrap-theme.css") %>" />
	
	  <%--<script>var require = { urlArgs: 'v=<%=new ResourceVersion().Version()%>' };</script>--%>
	  <script>var require = { urlArgs: 'v=<%=1%>' };</script>
	<script type="text/javascript">
		Teleopti = {
			BusinessUnitId: '<%= identity.BusinessUnitId.ToString() %>',
			DataSource: '<%= identity.DataSource.Application.Name %>'
		}
	</script>
	<script src="Areas/HealthCheck/Content/Scripts/require/configuration.js"></script>
	<script data-main="Areas/HealthCheck/Content/Scripts/main" type="text/javascript" src="<%= Url.Content("~/Content/require/require.js") %>"></script>
</head>

<body >
	
	<!--[if lt IE 7]>
		<p class="chromeframe">You are using an outdated browser. <a href="http://browsehappy.com/">Upgrade your browser today</a> or <a href="http://www.google.com/chromeframe/?redirect=true">install Google Chrome Frame</a> to better experience this application.</p>
	<![endif]-->
	<style>
		input[type=date] { line-height: initial; }
	</style>
	<div id="Health-Check" class="container">
		<div class="row">
			<div class="col-xs-12">
				<h1>Health Check</h1>
				<div data-bind="ifnot: hasPermission">
					No Permission!
				</div>
				<div data-bind="if: hasPermission">
					<h3>Services</h3>
					<ul class="list-group services" data-bind="foreach: services">
						<li class="list-group-item" data-bind="css: { 'list-group-item-success': Status == 4, 'list-group-item-danger': Status == 1 }">
							<span data-bind="text: DisplayName"></span>
							<i class="pull-right glyphicon glyphicon-ok" data-bind="visible: Status == 4"></i>
						</li>
					</ul>
					<h3>ETL log objects</h3>
					<ul class="list-group etl-log-objects" data-bind="foreach: logObjects">
						<li class="list-group-item">
							<h4 class="list-group-item-heading" data-bind="text: new Date(last_update).toISOString() + ' (last updated interval)'"></h4>
							<h4 class="list-group-item-heading" data-bind="text: log_object_id + ' - ' + log_object_desc + ' > ' + detail_desc"></h4>
							<p class="list-group-item-text" data-bind="text: 'Procedure name: ' + proc_name"></p>
						</li>
					</ul>
					<h3>Stardust Status</h3>
					<li class="list-group-item stardust" data-bind="css: { 'list-group-item-success': StardustSuccess() == true }">
						<span data-bind="visible: StardustSuccess()">Success</span>
						<i class="pull-right glyphicon glyphicon-ok" data-bind="visible: StardustSuccess()"></i>
					</li>
					
					<h3>Hangfire Failed Events</h3>
					<li class="list-group-item" data-bind="css: { 'list-group-item-success': HangfireFailCount() == 0, 'list-group-item-danger': HangfireFailCount() > 0 }">
						<span data-bind="text: HangfireFailCount()"></span>
						<i class="pull-right glyphicon glyphicon-ok" data-bind="visible: HangfireFailCount() == 0"></i>
						<i class="pull-right glyphicon glyphicon-warning-sign" data-bind="visible: HangfireFailCount() > 0"></i>
					</li>

					<h3>ETL history</h3>
					<ul class="list-group etl-history" data-bind="foreach: etlJobHistory">
						<li class="list-group-item" data-bind="css: { 'list-group-item-danger': exception_msg }">
							<h4 class="list-group-item-heading" data-bind="text: new Date(job_start_time).toISOString()"></h4>
							<h4 class="list-group-item-heading" data-bind="text: schedule_name + ' > ' + job_name + ' > ' + jobstep_name + ' BU: ' + business_unit_name"></h4>
							<p class="list-group-item-text" data-bind="text: 'Affected rows: ' + jobstep_affected_rows + ', Duration: ' + jobstep_duration_s"></p>
							<!-- ko if: exception_msg -->
							<p class="list-group-item-text"><b data-bind="text: exception_msg"></b></p>
							<p class="list-group-item-text" data-bind="text: exception_trace"></p>
							<p class="list-group-item-text"><b data-bind="text: inner_exception_msg"></b></p>
							<p class="list-group-item-text" data-bind="text: inner_exception_trace"></p>
							<!-- /ko -->
						</li>
					</ul>
					<button class="btn btn-default" data-bind="click: loadAllEtlJobHistory">Load all ETL job history from current week</button>
					<h3>Validate &amp; fix readmodels</h3>
					<div>
						<!-- ko if: readModelCheckAndFixJobId() && !readModelCheckAndFixJobPollingResult() -->
						<p>Could not retrieve status of last job. See <a target="_blank" data-bind="attr: { href: getCheckAndFixJobUrl() }">details</a> or start a new one below. </p>
						<!-- /ko -->
						<!-- ko if: readModelCheckAndFixJobId() && readModelCheckAndFixJobPollingResult() && readModelCheckAndFixJobIsRunning() -->
						<p>Last job is <b>running</b>. See <a target="_blank" data-bind="attr: { href: getCheckAndFixJobUrl() }">details</a>. </p>
						<!-- /ko -->
						<!-- ko if: readModelCheckAndFixJobId() && readModelCheckAndFixJobPollingResult() && !readModelCheckAndFixJobIsRunning() -->
						<p>Last job has <b>finished</b>. See <a target="_blank" data-bind="attr: { href: getCheckAndFixJobUrl() }">details</a>. </p>
						<!-- /ko -->
						<!-- ko if: !readModelCheckAndFixJobId() || ( !readModelCheckAndFixJobIsRunning() || !readModelCheckAndFixJobPollingResult() ) -->
						<form class="form-inline">
							<div class="form-group">
								<label for="readmodelCheckStartDate">Start date</label>
								<input id="readmodelCheckStartDate" class="datepicker" type="date" data-bind="value: readModelCheckStartDate"/>
							</div>
							<div class="form-group">
								<label for="readmodelCheckEndDate">End date</label>
								<input id="readmodelCheckEndDate" class="datepicker" type="date" data-bind="value: readModelCheckEndDate"/>
							</div>
						</form>
						<div style="margin: 1em 0;">
							<button class="btn btn-primary" data-bind="click: checkAndFixReadModels">Validate & Fix</button>
						</div>
						<!-- /ko -->
					</div>
					
					<details>
						<summary>Re-initialize readmodels</summary>
						<p>Before re-initializing readmodels, please make sure to truncate following tables. Otherwise, it will cause a system error. </p>
						<ul>
							<li>ReadModel.ScheduleProjectionReadOnly</li>
							<li>ReadModel.PersonScheduleDay</li>
							<li>ReadModel.ScheduleDay</li>
						</ul>
						<!-- ko if: reinitReadModelsJobId() && !reinitReadModelsJobPollingResult() -->
						<p>Could not retrieve status of last attempt. See <a target="_blank" data-bind="attr: { href: getReinitJobUrl() }">details</a> or start again below. </p>
						<!-- /ko -->
						<!-- ko if: reinitReadModelsJobId() && reinitReadModelsJobPollingResult() && reinitReadModelsJobIsRunning() -->
						<p>Last job is <b>running</b>. See <a target="_blank" data-bind="attr: { href: getReinitJobUrl() }">details</a>. </p>
						<!-- /ko -->
						<!-- ko if: reinitReadModelsJobId() && reinitReadModelsJobPollingResult() && !reinitReadModelsJobIsRunning() -->
						<p>Last job is <b>done</b>. See <a target="_blank" data-bind="attr: { href: getReinitJobUrl() }">details</a>. </p>
						<!-- /ko -->
						<!-- ko if: !reinitReadModelsJobId() || ( !reinitReadModelsJobIsRunning() || !reinitReadModelsJobPollingResult() ) -->
						<div>
							<div>
								<label>
									<input type="checkbox" data-bind="checked: iHaveEmptiedTables"/>
									I have emptied the readmodel tables
								</label>
							</div>
						</div>
						<div>
							<button class="btn btn-default" data-bind="enable: iHaveEmptiedTables, click: reinitReadModels">Re-initialize</button>
						</div>
						<!-- /ko -->
					</details>
					
					<h3>Configured URL:s</h3>
					<ul class="list-group" data-bind="foreach: configuredUrls">
						<li class="list-group-item configured-url" data-bind="css: { 'list-group-item-success': Reachable == true }">
							<span data-bind="text: Url"></span>
							<i class="pull-right glyphicon glyphicon-ok" data-bind="visible: Reachable"></i>
						</li>
					</ul>
				</div>
			</div>
		</div>
	</div>
</body>
</html>
