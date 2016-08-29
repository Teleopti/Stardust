<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="Teleopti.Ccc.Web.Areas.Reporting.Index" ValidateRequest="false" Buffer="true" %>
<%@ Register TagPrefix="Analytics"  Namespace="Teleopti.Analytics.Parameters" Assembly="Teleopti.Analytics.Parameters" %>


<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" dir="ltr">
<head runat="server">
	<title></title>
	<script type="text/javascript" src="../../Content/jquery/jquery-1.12.4.min.js"></script>
	<script type="text/javascript" src="Content/Scripts/persianDatepicker.min.js"></script>
	<script type="text/javascript">
		//ensure report menu is closed
		$('html').on('click', function () {
			if (parent) {
				parent.$('.dropdown').removeClass('open');
			}
		});
		//to refresh if frame in frame
		if (window.top != window.parent.self)
			window.parent.parent.location = window.parent.location;

		var cookieName = "fileIsDownloadedToken";
		var fileDownloadCheckTimer;
		function checkForDownload() {
			fileDownloadCheckTimer = setInterval(function () {
				var cookieValue = getCookie(cookieName);
				if (cookieValue) {
					document.body.style.cursor = 'auto';
					clearInterval(fileDownloadCheckTimer);
					document.cookie = cookieName + '=; path=/';
				}
					
			}, 2000);
		}

		function getCookie(cname) {
			var name = cname + "=";
			var theValue = "";
			//console.log('cookies=' + document.cookie);
			var ca = document.cookie.split(';');
			for (var i = 0; i < ca.length; i++) {
				var c = ca[i];
				while (c.charAt(0) == ' ') {
					c = c.substring(1);
				}
				if (c.indexOf(name) == 0) {
					theValue = c.substring(name.length, c.length);
				}
				if(theValue !== "")
					return theValue;
			}
			return theValue;
		}
		function submitIt(button) {
			var oldThingy = getCookie(cookieName);
			if(oldThingy !== "")
				document.cookie = cookieName + '=; path=/';
			document.body.style.cursor = 'wait';
			checkForDownload();
			__doPostBack(button, '');
		}
			
	</script>
	<link href="Content/Styles/persianDatepicker-default.css" rel="stylesheet" />
	<link href="Content/Styles/Styles.css" rel="stylesheet" />
	<link href="Content/Styles/calendar.css" rel="stylesheet" />
</head>

<body  style="height: 100%;" >
	<form id="aspnetForm" runat="server">
		<asp:ScriptManager ID="ScriptManager1" EnablePartialRendering="true" runat="server" EnableScriptGlobalization="true" EnableScriptLocalization="true" />
		<div class="Panel">
			<div class="DetailsView" style="height: 80%; overflow: auto">
				<Analytics:Selector LabelWidth="30%" List1Width="75%" ID="ParameterSelector" name="ParameterSelector" runat="server" OnInit="Selector_OnInit" SkipPermissions="true"></Analytics:Selector>
				<div style="float: left; width: 29%">
					<asp:ValidationSummary ID="ValidationSummary1" runat="server" ForeColor="Red" />
					<asp:Label ID="labelPermissionDenied" runat="server" ForeColor="Red" Font-Size="Large" Visible="false"></asp:Label>
				</div>
				<div style="float: right; width: 69%">
				    <div style="float: right; width: 20%">
						<asp:ImageButton Style="float: left" formtarget="_self" OnClientClick="submitIt('buttonShowWord')" UseSubmitBehavior="false"   OnClick="ButtonShowClickWord" ID="buttonShowWord"  ImageUrl="images/icon.doc.png" ToolTip='' runat="server" />
					</div>
					
					<div style="float: right; width: 20%">
						<asp:ImageButton Style="float: left" formtarget="_self" OnClientClick="submitIt('buttonShowExcel')" UseSubmitBehavior="false"  OnClick="ButtonShowClickExcel" ID="buttonShowExcel" Width="48" Height="48" ImageUrl="images/excel.png" ToolTip='' runat="server" />
					</div>
					<div style="float:right; width: 20%;">
						<asp:ImageButton Style="float: left;" formtarget="_blank" OnClientClick="submitIt('buttonShowPdf')" UseSubmitBehavior="false" OnClick="ButtonShowClickPdf" ID="buttonShowPdf" Width="48" Height="48" ImageUrl="images/filetype_pdf.png" ToolTip='' runat="server" />
					</div>
                    <div style="float:left; width: 25%">
						<asp:ImageButton Style="float:right; " formtarget="_self" OnClientClick="submitIt('buttonShowImage')" UseSubmitBehavior="false" OnClick="ButtonShowClickImage" ID="buttonShowImage" Width="48" Height="48" ImageUrl="images/icon-show.png" ToolTip='' runat="server" />
					</div>
				</div>
				
			</div>
	</div>
	</form>
	<asp:Label runat="server" ForeColor="red" ID="labelError"></asp:Label>
	<div style="width: 100%; height: 100%">&nbsp</div>
</body>
	
</html>
