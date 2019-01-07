<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>

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
	<title>Teleopti WFM Anywhere</title>
	<meta name="description" content="" />
	<meta name="viewport" content="width=device-width,initial-scale=1.0" />
	<meta name="apple-mobile-web-app-capable" content="yes" />

	<link rel="stylesheet" href="Content/jqueryui/smoothness/jquery-ui-1.10.2.custom.css" />
	<link rel="stylesheet" href="Content/bootstrap/Content/bootstrap.css" />
	<link rel="stylesheet" href="Content/bootstrap/Content/bootstrap-theme.css" />

	<link href="~/WFM/dist/ng2/assets/favicon/favicon.ico?v=2" rel="shortcut icon" type="image/x-icon" />
	<script type="text/javascript" src="Content/jquery/jquery-1.12.4.min.js"></script>
	<script type="text/javascript">
		function showPendingTime(pendingTimeInSecond) {
			$("#pendingTime").html(pendingTimeInSecond);

			if (pendingTimeInSecond <= 0) {
				var targetUrl = "./WFM";
				if (window.location.href.indexOf("/Anywhere#teamschedule/") > 0)
					targetUrl += "/#/teams";
				window.location.href = targetUrl;
			} else {
				pendingTimeInSecond -= 1;
				setTimeout(function () {
					showPendingTime(pendingTimeInSecond);
				}, 1000);
			}
		}

		showPendingTime(<%= ViewData.Eval("WaitingTimeInSecond") %>);
	</script>
</head>

<body>
	<nav class="navbar navbar-inverse navbar-static-top">
		<div class="container">
			<div class="navbar-header">
				<a class="navbar-brand">Teleopti</a>
			</div>
			<div>
				<ul class="nav navbar-nav">
					<li>
						<a href="#"><span>
								<%= ViewData.Eval("ResTeamSchedule") %></span></a>
					</li>

					<li id="link-realtimeadherence">
						<a href="#"><span>
								<%= ViewData.Eval("ResRta") %></span></a>
					</li>

					<li id="reports" class="dropdown">
						<a href="#"><span>
								<%= ViewData.Eval("ResReports") %></span>
							<b class="caret"></b>
						</a>
					</li>
				</ul>
			</div>
			<ul class="nav navbar-nav navbar-right">
				<li class="dropdown">
					<a href="#" class="dropdown-toggle">
						<span>
							<small>
								<%= ViewData.Eval("CurrentBusinessUnit") %></small>
						</span>
						<b class="caret"></b>
					</a>
				</li>
				<li class="dropdown">
					<a href="#" class="dropdown-toggle">
						<i class="glyphicon glyphicon-user"></i>
						<span class="user-name">
							<small>
								<%= ViewData.Eval("CurrentUserName") %></small>
						</span>
						<b class="caret"></b>
					</a>
				</li>
			</ul>
		</div>
	</nav>
	<div class="container">
		<div class="row" style="background: #bada55; text-align: left; color: #333 !important; text-decoration: none !important;">
			<h2 class="text-center">
				<%= ViewData.Eval("ResMyTeamMigrated") %>
			</h2>
		</div>
		<div class="row">
			<h4 class="text-center">
				<%= ViewData.Eval("ResPageWillBeRedirected") %>
			</h4>
		</div>
	</div>
</body>

</html>