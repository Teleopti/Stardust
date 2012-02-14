<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ActionBarView.ascx.cs" Inherits="Teleopti.Analytics.Portal.PerformanceManager.View.ActionBarView" %>
<table border="0" width="100%">
    <tr>
        <td class="TreeHeader">
            Actions
        </td>
    </tr>
    <tr>
        <td> 
            <a id="aNewReport" name="aNewReport" runat="server" href="javascript:void(0);" onclick="return false;" class="actionLink">New report...</a>
        </td>
    </tr>
    <tr>
        <td>
            <asp:LinkButton ID="linkButtonDeleteMode" runat="server" Text="Delete mode on" CssClass="actionLink"></asp:LinkButton>
        </td>
    </tr>
    <tr>
        <td class="stretchActionBarWidth">&nbsp;</td>
    </tr>
</table>