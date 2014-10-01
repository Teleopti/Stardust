<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
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
				<h3>Configured URL's</h3>
				<ul class="list-group" data-bind="foreach: configuredUrls">
					<li class="list-group-item configured-url" data-bind="text: Url + (Message ? ' (' + Message + ')' : ''), css: { 'list-group-item-success': Reachable == 'True', 'list-group-item-danger': Reachable == 'False' }"></li>
				</ul>
				<h3>Check bus</h3>
				<button data-bind="click: checkBus" class="btn btn-primary">Start check</button>
				
			</div>
		</div>
	</div>
</body>
</html>
