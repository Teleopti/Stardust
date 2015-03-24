<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="Teleopti.Ccc.Web.Areas.Reporting.Index" ValidateRequest="false" %>
<%@ Register TagPrefix="Analytics"  Namespace="Teleopti.Analytics.Parameters" Assembly="Teleopti.Analytics.Parameters" %>


<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" dir="ltr">
<head runat="server">
	<title></title>
	<script type="text/javascript" src="../../Content/jquery/jquery-1.10.2.min.js"></script>
	<script type="text/javascript" src="Content/Scripts/persianDatepicker.min.js"></script>
	<script type="text/javascript">
		//ensure report menu is closed
		$('html').on('click', function () {
			if (parent) {
				parent.$('.dropdown').removeClass('open');
			}
		});
	</script>
	<link href="Content/Styles/persianDatepicker-default.css" rel="stylesheet" />
	<link href="Content/Styles/Styles.css" rel="stylesheet" />
	<link href="Content/Styles/calendar.css" rel="stylesheet" />
</head>

<body >
	<form id="aspnetForm" runat="server">
		<asp:ScriptManager ID="ScriptManager1" EnablePartialRendering="true" runat="server" EnableScriptGlobalization="true" EnableScriptLocalization="true" />
		<div class="Panel">
			<div class="DetailsView" style="height: 80%; overflow: auto">
				<Analytics:Selector LabelWidth="30%" List1Width="75%" ID="ParameterSelector" name="ParameterSelector" runat="server" OnInit="Selector_OnInit"></Analytics:Selector>
				<div style="float: left; width: 29%">
					<asp:ValidationSummary ID="ValidationSummary1" runat="server" ForeColor="Red" />
					<asp:Label ID="labelPermissionDenied" runat="server" ForeColor="Red" Font-Size="Large" Visible="false"></asp:Label>
				</div>
				<div style="float: right; width: 69%">
					<div style="float: left; width: 33%;">
						<asp:ImageButton Style="float: right;margin-right: 25px" formtarget="_blank" OnClick="ButtonShowClickPdf" ID="buttonShowPdf" Width="48" Height="48" ImageUrl="images/filetype_pdf.png" ToolTip='' runat="server" />
					</div>
					<div style="float: right; width: 15%">
						<asp:ImageButton Style="float: left" formtarget="_blank" OnClick="ButtonShowClickImage" ID="buttonShowImage" Width="48" Height="48" ImageUrl="images/icon_show.gif" ToolTip='' runat="server" />
					</div>
					<div style="float: right; width: 20%">
						<asp:ImageButton Style="float: left" formtarget="_blank" OnClick="ButtonShowClickExcel" ID="buttonShowExcel" Width="48" Height="48" ImageUrl="images/excel.png" ToolTip='' runat="server" />
					</div>
					<div style="float: right; width: 20%">
						<asp:ImageButton Style="float: left" formtarget="_blank" OnClick="ButtonShowClickWord" ID="buttonShowWord"  ImageUrl="images/icon.doc.png" ToolTip='' runat="server" />
					</div>
					
				</div>
			</div>
	</div>
	</form>
</body>
</html>
