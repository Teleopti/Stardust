<%@ Page Language="C#" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="ScheduleAdherence.aspx.cs" Inherits="Teleopti.Ccc.Web.Areas.Reporting.ScheduleAdherence" ValidateRequest="false" %>

<%@ Register TagPrefix="Analytics" Namespace="Teleopti.Analytics.Parameters" Assembly="Teleopti.Analytics.Parameters" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
	<script type="text/javascript" src="../../Content/jquery/jquery-1.12.4.min.js"></script>
	<script type="text/javascript" src="Content/Scripts/persianDatepicker.min.js"></script>
	<script type="text/javascript">
		//to refresh if frame in frame
		if (window.top != window.parent.self)
			window.parent.parent.location = window.parent.location;
		function hideThings() {
			$("body").css("cursor", "progress");
			$("#aspnetForm").fadeOut();
			$("#reportData").fadeOut();
			$("#divReportTable").fadeOut();
		}
	</script>
	<link href="Content/Styles/persianDatepicker-default.css" rel="stylesheet" />
	<link href="Content/Styles/Styles.css" rel="stylesheet" />
	<link href="Content/Styles/calendar.css" rel="stylesheet" />
</head>
<body class="ReportBody">
	<form id="aspnetForm" style="margin-top: 10px" runat="server">
		<asp:HiddenField ID="dateOffset" Value="0" runat="server" />
		<asp:ImageButton ID="buttonShowSelection" Visible="False" alt="Show selection" runat="server" OnClick="ShowSelection" src="images/down.png" /><asp:ImageButton ID="buttonHideSelection" alt="Hide selection" runat="server" OnClick="HideSelection" src="images/up.png" />
		<asp:ScriptManager ID="ScriptManager1" EnablePartialRendering="true" runat="server" EnableScriptGlobalization="true" EnableScriptLocalization="true" />
		<asp:Panel ID="selectionPanel" runat="server">
			<div class="Panel">
				<div class="DetailsView" style="height: 80%; overflow: auto">
					<Analytics:Selector LabelWidth="30%" List1Width="75%" ID="ParameterSelector" name="ParameterSelector" runat="server" OnInit="Selector_OnInit" SkipPermissions="true"></Analytics:Selector>
					<div style="float: left; width: 29%">
						<asp:ValidationSummary ID="ValidationSummary1" runat="server" ForeColor="Red" />
						<asp:Label ID="labelPermissionDenied" runat="server" ForeColor="Red" Font-Size="Large" Visible="false"></asp:Label>
					</div>
					<div style="float: right; width: 69%">
						<div id="buttonId" style="float: left; width: 33%;">
							<asp:ImageButton Style="float: right; margin-right: 25px" OnClientClick="hideThings();" OnClick="ButtonShowTheReport" ID="buttonShowReport" Width="48" Height="48" ImageUrl="images/icon-show.png" ToolTip='Show report' runat="server" />
						</div>
					</div>
					<asp:Label runat="server" ForeColor="red" ID="labelError"></asp:Label>
				</div>
			</div>
           
		</asp:Panel>
         
		<asp:Panel runat="server" ID="reportData" Visible="false">
		    <input runat="server" ID="lastGroupPage" hidden="hidden" />
			<table style="border: 0px">
				<tr>
					<td>
						<table width="100%">
							<tr>
								<td id="tdTodaysDateTime" runat="server" align="right" class="ReportDateTime">xx2008-07-02 18:09</td>
							</tr>
							<tr>
								<td></td>
							</tr>
							<tr>
								<td id="tdReportName" runat="server" class="ReportTitle">xxAgent Schedule Adherence</td>
							</tr>
							<tr>
								<td class="ReportSelectionBottom">
									<table class="ReportSelection">
										<tr id="trDates" runat="server">
											<td id="tdDatesLabel" runat="server" class="ReportSelectionLabel">xxDates:</td>
											<td id="tdDatesText" runat="server" colspan="3">xxDates</td>
										</tr>
										<tr id="trGroupPage" runat="server">
											<td id="tdGroupPageLabel" runat="server" class="ReportSelectionLabel">xxGroup Page:</td>
											<td id="tdGroupPageText" runat="server" colspan="3">xxContract</td>
										</tr>
										<tr id="trGroupPageGroup" runat="server">
											<td id="tdGroupPageGroupLabel" runat="server" class="ReportSelectionLabel">xxGroup:</td>
											<td id="tdGroupPageGroupText" runat="server" colspan="3">xxFull Time</td>
										</tr>
										<tr id="trGroupPageAgent" runat="server">
											<td id="tdGroupPageAgentLabel" runat="server" class="ReportSelectionLabel">xxAgent:</td>
											<td id="tdGroupPageAgentText" runat="server" colspan="3">xxKarl Kurla</td>
										</tr>
										<tr id="trSite" runat="server">
											<td id="tdSiteLabel" runat="server" class="ReportSelectionLabel">xxSite:</td>
											<td id="tdSiteText" runat="server" colspan="3">xxNamsos</td>
										</tr>
										<tr id="trTeam" runat="server">
											<td id="tdTeamLabel" runat="server" class="ReportSelectionLabel">xxTeam:</td>
											<td id="tdTeamText" runat="server" colspan="3">xxNS Team 13</td>
										</tr>
										<tr id="trAgent" runat="server">
											<td id="tdAgentLabel" runat="server" class="ReportSelectionLabel">xxAgent:</td>
											<td id="tdAgentText" runat="server" colspan="3">xxOle Bramserud</td>
										</tr>
										<tr>
											<td id="tdAdherenceCalculationLabel" runat="server" class="ReportSelectionLabel">xxtAdherence Calculation:</td>
											<td id="tdAdherenceCalculationText" runat="server" colspan="3">xxReady Time vs Scheduled Ready Time</td>
										</tr>
										<tr>
											<td id="tdSortOrderLabel" runat="server" class="ReportSelectionLabel">xxSort Order:</td>
											<td id="tdSortOrderText" runat="server" colspan="3">xxLast Name</td>
										</tr>
										<tr id="trTimeZoneParameter" runat="server">
											<td id="tdTimeZoneLabel" runat="server" class="ReportSelectionLabel">xxTime Zone:</td>
											<td id="tdTimeZoneText" runat="server" colspan="3">xx(GMT+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna</td>
										</tr>
										<tr id="DayButtons" runat="server">
											<td id="tdDateLabel" runat="server" class="ReportSelectionLabel">xxDate:</td>
											<td></td>
											<td  runat="server" align="center">
												<table runat="server" >
													<tr runat="server" >
														<td runat="server" >
															<asp:ImageButton ID="imageButtonPreviousDay" runat="server"
																ImageUrl="images/previous.gif" OnClientClick="hideThings();" OnClick="ImageButtonPreviousDayClick" /></td>
														<td id="tdDateText" runat="server" align="center">xx2008-07-02</td>
														<td runat="server" >
															<asp:ImageButton ID="imageButtonNextDay" runat="server"
																ImageUrl="images/next.gif" OnClientClick="hideThings();" OnClick="ImageButtonNextDayClick" /></td>

													</tr>
												</table>

											</td>
											<td></td>
										</tr>
									</table>
								</td>
							</tr>
						</table>
					</td>
				</tr>
				<tr>
					<td style="height: 10px"></td>
				</tr>
				<tr>
					<td>
						
					</td>
				</tr>
			</table>
		</asp:Panel>

	</form>
	<div id="divReportTable" runat="server"></div>
</body>
</html>
