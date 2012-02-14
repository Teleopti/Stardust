<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="report_agent_schedule_adherence.aspx.cs" Inherits="Teleopti.Analytics.Portal.Reports.Ccc.report_agent_schedule_adherence" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>xxAgent Schedule Adherence</title>
</head>
<body class="ReportBody">
    <form id="form1" runat="server">
    <table border="0">
        <tr>
            <td>
                <table border="0" cellspacing="0" width="100%">
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
                            <table class="ReportSelection" border="0">
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
                                <tr>
                                    <td id="tdDateLabel" runat="server" class="ReportSelectionLabel">xxDate:</td>
                                    <td align="right"><asp:ImageButton ID="imageButtonPreviousDay" runat="server" 
                                            ImageUrl="~/images/prev.gif" onclick="imageButtonPreviousDay_Click"/></td>
                                    <td id="tdDateText" runat="server" align="center">xx2008-07-02</td>
                                    <td align="left"><asp:ImageButton ID="imageButtonNextDay" runat="server" 
                                            ImageUrl="~/images/next.gif" onclick="imageButtonNextDay_Click"/></td>
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
                <div id="divReportTable" runat="server"></div>
            </td>
        </tr>
    </table>
    </form>
</body>
</html>
