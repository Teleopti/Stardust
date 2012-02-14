<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AgentScorecard.aspx.cs" Inherits="Teleopti.Analytics.Portal.Reports.Ccc.AgentScorecard" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<script runat="server" >
        protected String GetTime()
        {
            return DateTime.Now.ToString("t");
        }

    </script>
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    
</head>
<body >
    <form id="form1" runat="server">
     
    <asp:Panel CssClass="Scorecard" Direction="LeftToRight" ID="ReportPanel"   runat="server" width="550px"  BorderStyle="Solid" BorderWidth="1px" BorderColor="Blue">
        <table cellpadding="10" cellspacing="0" width="520px">
            <tr>
                <td>
                <asp:Label class="ScorecardTitle" runat="server" ID="ScorecardLabel" Text="" Visible="false"></asp:Label>
                <asp:DropDownList class="ScorecardTitle" AutoPostBack="true"  onselectedindexchanged="ScorecardsCombo_SelectedIndexChanged" Visible="true" id="ScorecardsCombo" runat="server" DataValueField="id" DataTextField="name">
                </asp:DropDownList>
                
                </td>
                <td>
                
                <asp:Label ID="LoggedOnUser" runat="server" Text=""></asp:Label>
                </td>
            </tr>
            
            <tr>
                <td colspan="2">
                    <asp:Table CssClass="ScorecardTable" CellSpacing="0" CellPadding="0"  Width="520" runat="server" ID="ResultTable">
                        <asp:TableRow>
                            <asp:TableCell CssClass="ScorecardResultHeader"><%=GetResource("ResKpi")%></asp:TableCell>
                            <asp:TableCell CssClass="ScorecardResultHeader" Wrap="false" ColumnSpan="2" ><%=GetResource("ResActual")%></asp:TableCell>
                            <asp:TableCell CssClass="ScorecardResultHeader"><%=GetResource("ResTarget")%></asp:TableCell>
                            <asp:TableCell CssClass="ScorecardResultHeader"><%=GetResource("ResTeamActual")%></asp:TableCell>
                            <asp:TableCell CssClass="ScorecardResultHeader"><%=GetResource("ResPreviousPeriod")%></asp:TableCell>
                            <asp:TableCell CssClass="ScorecardResultHeader"><%=GetResource("ResTrend")%></asp:TableCell>
                        </asp:TableRow>
                    </asp:Table>
                </td>
            </tr>
        </table>
                
    </asp:Panel>
    </form>
</body>
</html>
