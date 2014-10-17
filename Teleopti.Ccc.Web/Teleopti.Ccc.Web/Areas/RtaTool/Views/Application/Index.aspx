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
    <title>Teleopti Rta Tool</title>
    <meta name="description" content="" />
    <meta name="viewport" content="width=device-width,initial-scale=1.0" />
		
    <link rel="stylesheet" href="Content/bootstrap/Content/bootstrap.css" />
    <link rel="stylesheet" href="Content/bootstrap/Content/bootstrap-theme.css" />	
	
	  <%--<script>var require = { urlArgs: 'v=<%=new ResourceVersion().Version()%>' };</script>--%>
	  <script>var require = { urlArgs: 'v=<%=1%>' };</script>
	<script src="Areas/RtaTool/Content/Scripts/require/configuration.js"></script>
    <script data-main="Areas/RtaTool/Content/Scripts/main" type="text/javascript" src="Content/require/require.js"></script>

</head>

<body >
	
    <!--[if lt IE 7]>
        <p class="chromeframe">You are using an outdated browser. <a href="http://browsehappy.com/">Upgrade your browser today</a> or <a href="http://www.google.com/chromeframe/?redirect=true">install Google Chrome Frame</a> to better experience this application.</p>
    <![endif]-->
	<div class="container ">
		
		
	
		<table class="table">
			<tr>
				<td>Ashley Andeen</td>
				<td>
					<button class="btn btn-default" data-bind="click: answer">Answer</button>					
				</td>
				<td>
					<button class="btn btn-danger" data-bind="click: hangUp">Hang up</button>
				</td>
				
				<td>
					
				</td>
				<td>
					<input data-bind="value: specifiedState"/>
					<button class="btn btn-default" data-bind="click: sendState">Send</button>
				</td>
				<td></td>
				<td>
					<p>Logged in: <input type="checkbox" data-bind="checked: isLoggedOn" /></p>
				</td>
			</tr>
		</table>
	</div>

</body>
</html>
