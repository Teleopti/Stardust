<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ActionBarView.ascx.cs" Inherits="Teleopti.Analytics.Portal.PerformanceManager.View.ActionBarView" %>
<div style="font-size: 12px;line-height:20px">

        <div class="TreeHeader">
            Actions
        </div>
 
        <div> 
            <a id="aNewReport" name="aNewReport" runat="server" href="javascript:void(0);" onclick="return false;" class="actionLink">New report...</a>
        </div>

        <div>
            <asp:LinkButton ID="linkButtonDeleteMode" runat="server" Text="Delete mode on" CssClass="actionLink"></asp:LinkButton>
        </div>

        <div class="stretchActionBarWidth">&nbsp;</div>
 
</div>