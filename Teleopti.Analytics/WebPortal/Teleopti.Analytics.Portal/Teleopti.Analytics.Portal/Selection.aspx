<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Selection.aspx.cs" Inherits="Teleopti.Analytics.Portal.Selection" ValidateRequest="false" %>
<%@ Register TagPrefix="Analytics"  Namespace="Teleopti.Analytics.Parameters" Assembly="Teleopti.Analytics.Parameters" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" dir="ltr">
<head runat="server">
    <title>Selection</title>
    <link rel="shortcut icon" href="~/Images/favicon.ico"/>
</head>
<body>
    <form id="aspnetForm" runat="server">
		<asp:ScriptManager ID="ScriptManager1" EnablePartialRendering="true" runat="server"  EnableScriptGlobalization="true" EnableScriptLocalization="true" />
		<asp:HiddenField ID="HiddenID" runat="server" />
		<asp:HiddenField ID="HiddenUserID" runat="server" />
		<asp:HiddenField ID="ParametersValid" runat="server" Value="0" /> 

		<div style="float: right">
			<div style="float:right; margin-left:10px;margin-right: 10px;padding-bottom: 5px"><asp:LinkButton CausesValidation="False" runat="server"  ID="SignOutButton" CssClass="SignOut" OnClick="SignOut" Text="xxSign Out"></asp:LinkButton></div>
			<div style="white-space:nowrap; float:right" ><asp:Label ID="LoggedOnUser" runat="server" Text=""></asp:Label></div>
		</div>
			
		<div class="Panel">
			<ajaxToolkit:CollapsiblePanelExtender ID="CPEReports" runat="Server" TargetControlID="Reports_ContentPanel"
				ExpandControlID="Reports_HeaderPanel" CollapseControlID="Reports_HeaderPanel"
				Collapsed="false" ExpandDirection="Vertical" ImageControlID="ImageReportsToggle"
				ExpandedImage="~/images/up.png" ExpandedText='xxCollapse' CollapsedImage="~/images/down.png"
				CollapsedText='xxExpand' SuppressPostBack="true" />
				<div class="Caption">
					<asp:Panel ID="Reports_HeaderPanel" runat="server" Style="cursor: pointer;" >
					<div style="float: left;padding-top: 2px">
						<asp:Image ID="ImageReportsToggle" runat="server" ImageUrl="~/images/down.png" />
					</div>
					<div style="float: left;padding-top: 2px">
						<asp:Label ID="labelRepCaption" CssClass="ReportName" runat="server" Text="xxxRapportnamnet"></asp:Label>
					</div>
					
				</asp:Panel>
					<div style="float: right; width: 25px; padding-top: 3px">
						<asp:ImageButton ID="ImageButtonHelp" runat="server" ImageUrl="~/images/Question_16x16.png" ToolTip="xxHelp" OnClientClick="javascript:return false;" />
					</div>
				</div>
			
			<asp:Panel ID="Reports_ContentPanel" runat="server" Height="0px">
				<div class="DetailsView" >
					<Analytics:Selector LabelWidth="30%" List1Width="75%" ID="Parameter" runat="server" OnInit="Selector_OnInit">
					</Analytics:Selector>
				</div>
				<div>
					<asp:ValidationSummary ID="ValidationSummary1" runat="server" ForeColor="Red"/>
				</div>
				<div style="text-align: center;padding-top: 10px;height:60px">
					<asp:ImageButton  OnClick="ButtonShow_Click" ID="buttonShow" SkinID="Show" ToolTip='' runat="server" />
					<asp:Label ID="labelPermissionDenied" runat="server" ForeColor="Red" Font-Size="Large" Visible="false"></asp:Label>
				</div>
			</asp:Panel>
		</div>
		<div>
			<iframe  runat="server" id="ViewerFrame" name="ViewerFrame" style="display:none" src="" width="100%"  allowtransparency="true" ></iframe>
		</div>
		
    </form>
</body>
</html>
