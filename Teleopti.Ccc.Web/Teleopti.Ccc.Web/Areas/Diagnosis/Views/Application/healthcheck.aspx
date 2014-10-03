﻿<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Teleopti.Ccc.Domain.Security" %>
<%@ Import Namespace="Teleopti.Ccc.Domain.Security.AuthorizationData" %>
<%@ Import Namespace="Teleopti.Ccc.Domain.Security.Principal" %>
<%@ Import Namespace="Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider" %>
<%
	var currentTeleoptiPrincipal = new CurrentTeleoptiPrincipal();
	if (!new PermissionProvider(new PrincipalAuthorization(currentTeleoptiPrincipal)).HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage))
   {
	   throw new PermissionException("Not permitted!");
   }
	var identity = (ITeleoptiIdentity) currentTeleoptiPrincipal.Current().Identity; %>
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
		
    <link rel="stylesheet" href="../../Content/bootstrap/Content/bootstrap.css" />
    <link rel="stylesheet" href="../../Content/bootstrap/Content/bootstrap-theme.css" />	
	
	  <%--<script>var require = { urlArgs: 'v=<%=new ResourceVersion().Version()%>' };</script>--%>
	  <script>var require = { urlArgs: 'v=<%=1%>' };</script>
	<script type="text/javascript">
		Teleopti = {
			BusinessUnitId: '<%= identity.BusinessUnit.Id.ToString() %>',
			DataSource: '<%= identity.DataSource.Application.Name %>'
		}
	</script>
    <script data-main="../../Areas/Diagnosis/Content/Scripts/healthmain" type="text/javascript" src="<%= Url.Content("~/Content/require/require.js") %>"></script>
</head>

<body >
	
    <!--[if lt IE 7]>
        <p class="chromeframe">You are using an outdated browser. <a href="http://browsehappy.com/">Upgrade your browser today</a> or <a href="http://www.google.com/chromeframe/?redirect=true">install Google Chrome Frame</a> to better experience this application.</p>
    <![endif]-->
	<div class="container">
		<div class="row">
			<div class="col-xs-12">
				<h1>Health Check</h1>
				<h3>Services</h3>
				<ul class="list-group services" data-bind="foreach: services">
					<li class="list-group-item" data-bind="text: DisplayName, css: { 'list-group-item-success': Status == 4,'list-group-item-danger': Status == 1 }"></li>
				</ul>
				<h3>ETL history</h3>
				<ul class="list-group etl-history" data-bind="foreach: etlJobHistory">
					<li class="list-group-item" data-bind="css: { 'list-group-item-danger': exception_msg }">
						<h4 class="list-group-item-heading" data-bind="text: new Date(parseInt(job_start_time.substr(6))).toISOString()"></h4>
						<h4 class="list-group-item-heading" data-bind="text: schedule_name + ' > ' + job_name + ' > ' + jobstep_name + ' [BU: ' + business_unit_name + ']'"></h4>
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
				<h3>Check bus</h3>
				<button data-bind="click: checkBus, enable: checkBusEnabled" class="btn btn-primary">Start check</button>
				<!-- ko with: busResults -->
				<h3>Bus results</h3>
				<p><b data-bind="text: MachineName"></b> (<span data-bind="text: OSFullName + ' ' + OSPlatform + ' ' + OSVersion"></span>)</p>
				<ul class="list-group">
					<li class="list-group-item">Delay (ms): <span data-bind="text: MillisecondsDifference"></span></li>
					<!-- ko foreach: Services -->
					<li class="list-group-item" data-bind="text: Name, css: { 'list-group-item-success': Status == 4, 'list-group-item-danger': Status == 1 }"></li>
					<!-- /ko -->
					<li class="list-group-item">Physical memory installed: <span data-bind="text: (TotalPhysicalMemory/(1024.0*1024*1024)).toFixed(2)"></span> GB</li>
					<li class="list-group-item">Physical memory consumed by bus: <span data-bind="text: (BusMemoryConsumption / (1024.0 * 1024 * 1024)).toFixed(2)"></span> GB</li>
					<li class="list-group-item">Physical memory available: <span data-bind="text: (AvailablePhysicalMemory / (1024.0 * 1024 * 1024)).toFixed(2)"></span> GB</li>
				</ul>
				<!-- /ko -->
				<h3>Configured URL:s</h3>
				<ul class="list-group" data-bind="foreach: configuredUrls">
					<li class="list-group-item configured-url" data-bind="text: Url + (Message ? ' (' + Message + ')' : ''), css: { 'list-group-item-success': Reachable == 'True', 'list-group-item-danger': Reachable == 'False' }"></li>
				</ul>
			</div>
		</div>
	</div>
</body>
</html>
