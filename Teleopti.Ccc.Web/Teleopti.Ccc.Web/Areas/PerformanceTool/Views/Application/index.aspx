<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>

<%@ Import Namespace="Teleopti.Ccc.Web.Core" %>
<!DOCTYPE html>
<!--[if lt IE 7]>      <html class="no-js lt-ie9 lt-ie8 lt-ie7"> <![endif]-->
<!--[if IE 7]>         <html class="no-js lt-ie9 lt-ie8"> <![endif]-->
<!--[if IE 8]>         <html class="no-js lt-ie9"> <![endif]-->
<!--[if gt IE 8]><!-->
<html class="no-js">
<!--<![endif]-->
<head>
	<meta charset="utf-8" />
	<meta http-equiv="X-UA-Compatible" content="IE=Edge;IE=8;FF=3;OtherUA=4,chrome=1" />
	<title>Teleopti WFM Performance Tool</title>
	<meta name="description" content="" />
	<meta name="viewport" content="width=device-width,initial-scale=1.0" />

	<link rel="stylesheet" href="Content/bootstrap/Content/bootstrap.css" />
	<link rel="stylesheet" href="Content/bootstrap/Content/bootstrap-theme.css" />

	<link href="content/favicon.ico" rel="shortcut icon" type="image/x-icon" />

	<script>var require = { urlArgs: 'v=<%=new ResourceVersion().Version()%>' };</script>
	<script src="Areas/PerformanceTool/Content/Scripts/require/configuration.js"></script>
	<script data-main="Areas/PerformanceTool/Content/Scripts/main" type="text/javascript" src="Content/require/require.js"></script>

</head>
<style>
	header {
		background-color: #3f5f5f;
		text-shadow: 0px 0px 30px rgba(0, 0, 0, 1);
		color: white;
	}

		header .jumbotron {
			background-color: transparent;
		}

	.scenario-selector {
		width: 100%;
	}

	.scenario-configuration {
		font-family: monospace;
		width: 97%;
		height: 270px;
	}
</style>
<body>
	<!--[if lt IE 7]>
        <p class="chromeframe">You are using an outdated browser. <a href="http://browsehappy.com/">Upgrade your browser today</a> or <a href="http://www.google.com/chromeframe/?redirect=true">install Google Chrome Frame</a> to better experience this application.</p>
    <![endif]-->

	<nav class="navbar navbar-inverse navbar-static-top">
		<div class="navbar-inner">
			<div class="container">
				<ul class="nav navbar-nav">
					<li class="dropdown">
						<a href="#"
							class="dropdown-toggle nowrap navbar-brand"
							data-toggle="dropdown">Teleopti <b class="caret"></b>
						</a>
						<ul class="dropdown-menu">
							<li><a href="authentication/signout" tabindex="-1"><i class="glyphicon glyphicon-eject"></i><span>Log out</span></a></li>
						</ul>
					</li>
				</ul>
			</div>
		</div>
	</nav>

	<header>
		<br />
		<div class="container">
			<div class="jumbotron">
				<h1>Performance</h1>
				<p>n' stuff ;)</p>
			</div>
		</div>
	</header>

	<section data-bind="template: 'page'">
	</section>

	<script type="text/html" id="page">
		<div class="container">
			<div class="row">
				<br />
				<div class="col-md-6">
					<form>
						<fieldset>
							<h2>Scenario</h2>
							<label>Scenario</label>
							<select disabled="disabled" class="scenario-selector" data-bind="options: Scenarios, optionsText: 'Name', optionsValue: 'Name', value: ScenarioName, enable: EnableForm"></select>
							<label>Configuration</label>
							<textarea disabled="disabled" class="scenario-configuration" data-bind="value: Configuration, valueUpdate: 'afterkeydown', enable: EnableForm"></textarea>
							<label></label>
							<button disabled="disabled" class="btn btn-lg btn-primary run" data-bind="text: RunButtonText, enable: RunButtonEnabled, click: Run"></button>
						</fieldset>
					</form>
				</div>
				<div class="col-md-6">
					<h2>Progress</h2>
					<!-- ko foreach: ProgressItems() -->
					<p class="message-count">
						<span class="message-successes label label-success" data-bind="text: Successes"></span>
						<span class="message-failures label label-danger" data-bind="text: Failures"></span>
						<span class="message-target label label-info" data-bind="text: Target"></span>
						<span data-bind="text: Text"></span>
					</p>
					<!-- /ko -->
				</div>
				<div class="col-md-6">
					<h2>Result</h2>
					<p class="total-run-time"><span class="label label-info" data-bind="text: TotalRunTime"></span>Total run time</p>
					<p class="total-time-to-send-commands"><span class="label label-info" data-bind="text: TotalTimeToSendCommands"></span>Total time to send commands</p>
					<p class="scenarios-per-second"><span class="label label-info" data-bind="text: ScenariosPerSecond"></span>Scenarios per second</p>
				</div>
				<div class="col-md-6" data-bind="if: RunDone">
					<h2></h2>
					<div class="alert alert-success result-success">Finished!</div>
				</div>
				<div class="col-md-6">
					<h2>Hey!!  :-)</h2>
					<div data-bind="html: Text"></div>
				</div>
			</div>
		</div>
	</script>

</body>
</html>
