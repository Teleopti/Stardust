<%@ Import Namespace="Teleopti.Analytics.Portal.PerformanceManager.View"%>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ReportTreeView.ascx.cs" Inherits="Teleopti.Analytics.Portal.PerformanceManager.View.ReportTreeView" %>

<table>
    <tr>
        <td class="TreeHeader">
            Reports
        </td>
    </tr>
    <tr>
        <td>
            <asp:TreeView ID="TreeView2" runat="server" 
            onselectednodechanged="TreeView2_SelectedNodeChanged" 
            NodeStyle-HorizontalPadding="5"
            NodeStyle-VerticalPadding="3"
            SelectedNodeStyle-CssClass="selectedNode"
            ShowExpandCollapse="false"
            >
</asp:TreeView>
        </td>
    </tr>
</table>



