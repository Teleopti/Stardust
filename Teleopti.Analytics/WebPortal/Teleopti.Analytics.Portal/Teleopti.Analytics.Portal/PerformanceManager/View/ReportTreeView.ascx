<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ReportTreeView.ascx.cs" Inherits="Teleopti.Analytics.Portal.PerformanceManager.View.ReportTreeView" %>

<div style="font-size: 12px;line-height:16px;">

        <div class="TreeHeader" style="padding-top:16px">
            Reports
        </div>

        <div>
            <asp:TreeView ID="TreeView2" runat="server" 
            onselectednodechanged="TreeView2_SelectedNodeChanged" 
            NodeStyle-HorizontalPadding="5"
            NodeStyle-VerticalPadding="3"
            SelectedNodeStyle-CssClass="selectedNode"
            ShowExpandCollapse="false"
            NodeWrap="true"
            ></asp:TreeView>
        </div>

</div>

 