<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NewView.ascx.cs" Inherits="Teleopti.Analytics.Portal.PerformanceManager.View.NewView" %>
<table border="0" style="background-color: Silver" cellpadding="3">
    <tr>
        <td class="dialogTop" colspan="3" valign="middle">
            New Report
        </td>
    </tr>
    <tr>
        <td colspan="3" style="padding-bottom: 0">
            Name
        </td>
    </tr>
    <tr>
        <td colspan="3" style="padding-top: 0">
            <input type="text" id="inputTextName" name="inputTextName" runat="server" maxlength="100" onkeypress="keyPressHandler();" />
            <input type="text" id="Text1" style="visibility:hidden;display:none" value="Need this hidden textbox to workaround an IE bug regarding one textbox and enter key together with submit button" />
        </td>
    </tr>
    <tr>
        <td colspan="3">
            <span class="validate"></span>
        </td>
    </tr>
    <tr>
        <td style="width: 30%"></td>
        <td align="right">
            <asp:Button ID="buttonCreate" runat="server" Text="Create" OnClientClick="return validateNewReport();" />
        </td>
        <td align="right">
            <input type="button" id="buttonCancel" value="Cancel" onclick="hideNewPopup();" />
        </td>
    </tr>
</table>
