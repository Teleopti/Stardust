<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Teleopti.Ccc.Web.Core" %>
<!DOCTYPE html>
<!--[if lt IE 7]>      <html class="no-js lt-ie9 lt-ie8 lt-ie7"> <![endif]-->
<!--[if IE 7]>         <html class="no-js lt-ie9 lt-ie8"> <![endif]-->
<!--[if IE 8]>         <html class="no-js lt-ie9"> <![endif]-->
<!--[if gt IE 8]><!--> <html class="no-js"> <!--<![endif]-->
	<head>
	    <meta charset="utf-8" />
        <meta http-equiv="X-UA-Compatible" content="IE=Edge;IE=8;FF=3;OtherUA=4,chrome=1" />
		<title>Teleopti CCC Anywhere</title>
		<meta name="description" content="" />
		<meta name="viewport" content="width=device-width,initial-scale=1.0" />
		<meta name="apple-mobile-web-app-capable" content="yes" />
		
		<link rel="stylesheet" href="Content/jqueryui/smoothness/jquery-ui-1.10.1.custom.css" />
		<link rel="stylesheet" href="Content/bootstrap/Content/bootstrap.css" />
		<link rel="stylesheet" href="Content/bootstrap/Content/bootstrap-theme.css" />
		<link rel="stylesheet" href="Content/moment-datepicker/datepicker.css" />
		<link rel="stylesheet" href="Content/select2/select2.css"/>
		<link rel="stylesheet" href="Content/bootstrap-timepicker/css/bootstrap-timepicker.css" />

		<link rel="stylesheet" href="Areas/Anywhere/Content/Styles/helpers.css" />
		<link rel="stylesheet" href="Areas/Anywhere/Content/Styles/main.css" />

		<link rel="stylesheet" href="Areas/Anywhere/Content/Styles/team-schedule.css" />
		<link rel="stylesheet" href="Areas/Anywhere/Content/Styles/person-schedule.css" />
		<link rel="stylesheet" href="Areas/Anywhere/Content/Styles/realtimeadherence.css" />

        
        <link href="content/favicon.ico" rel="shortcut icon" type="image/x-icon"/>
        
	    <script>var require = { urlArgs: 'v=<%=new ResourceVersion().Version()%>' };</script>
		<script src="Areas/Anywhere/Content/Scripts/require/configuration.js"></script>
        <script data-main="Areas/Anywhere/Content/Scripts/main" type="text/javascript" src="Content/require/require.js"></script>
		
	</head>
	<body>
		<!--[if lt IE 7]>
			<p class="chromeframe">You are using an outdated browser. <a href="http://browsehappy.com/">Upgrade your browser today</a> or <a href="http://www.google.com/chromeframe/?redirect=true">install Google Chrome Frame</a> to better experience this application.</p>
		<![endif]-->
	</body>
</html>
