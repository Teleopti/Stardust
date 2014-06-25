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
    <title>Teleopti WFM Diagnosis Tool</title>
    <meta name="description" content="" />
    <meta name="viewport" content="width=device-width,initial-scale=1.0" />
		
    <link rel="stylesheet" href="Content/bootstrap/Content/bootstrap.css" />
    <link rel="stylesheet" href="Content/bootstrap/Content/bootstrap-theme.css" />	
	
	  <%--<script>var require = { urlArgs: 'v=<%=new ResourceVersion().Version()%>' };</script>--%>
	  <script>var require = { urlArgs: 'v=<%=1%>' };</script>

    <script data-main="Areas/Diagnosis/Content/Scripts/main" type="text/javascript" src="Content/require/require.js"></script>

</head>

<body >
	
    <!--[if lt IE 7]>
        <p class="chromeframe">You are using an outdated browser. <a href="http://browsehappy.com/">Upgrade your browser today</a> or <a href="http://www.google.com/chromeframe/?redirect=true">install Google Chrome Frame</a> to better experience this application.</p>
    <![endif]-->
  <div id="bjs">
	<div>messagebroker is running...</div>
	<br/>
	
	<div>sent: </div><div data-bind="text: sentPings"></div>
	<br/>
	<div>left: </div><div data-bind="text: pongsLeft"></div>

	<br/>
	</div>
	
	
<p>Number of pings: <input data-bind="value: numberOfPings" /></p>

   <button class="btn btn-danger" data-bind="click: sendPing">Send</button>
   <button class="btn btn-danger" data-bind="click: logMe">Log me</button>
</body>
</html>
