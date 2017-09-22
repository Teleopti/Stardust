<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AddReportView.ascx.cs"
    Inherits="Teleopti.Analytics.Portal.PerformanceManager.View.AddReportView" %>
<table style="width: 100%;" border="0">
    <tr>
        <td>
            <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
        </td>
        <td>            
            <asp:Button ID="Button1" runat="server" Text="New" onclick="Button1_Click" />
        </td>
    </tr>
</table>
