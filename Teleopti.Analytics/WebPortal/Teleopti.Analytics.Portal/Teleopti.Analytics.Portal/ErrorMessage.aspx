<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ErrorMessage.aspx.cs" Inherits="Teleopti.Analytics.Portal.ErrorMessage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Teleopti WFM</title>
    <link href="App_Themes/Theme1/Styles.css" rel="stylesheet" type="text/css" />
</head>
<body style="filter: progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr='#CADAF9', EndColorStr= '#fcfcfc');">
    <form id="form1" runat="server">
    <table cellspacing="0" cellpadding="0" width="100%" border="0">
			<tr id="topmenu">
				<td>
				   
				</td>   
			</tr>
        <tr>
            <td style="height:100px" valign="middle" align="center">
                <asp:Label ID="_labelErrorMessage" runat="server" Text="Label" CssClass="ErrorLabel"></asp:Label>
            </td>
        </tr>
        <tr>
            <td align="center" valign="bottom" style="height:50px" >
                <asp:Label ID="_labelPossibleSolutionHeader" runat="server" CssClass="TechnicalDetailHeader"></asp:Label>
            </td>
        </tr>
        <tr>
            <td align="center">
                <asp:Label ID="_labelPossibleSolution" runat="server" CssClass="TechnicalDetail"></asp:Label>
            </td>
        </tr>
        <tr>
            <td align="center">
                <asp:Label ID="_labelTechnicalDetailHeader" runat="server" CssClass="TechnicalDetailHeader"></asp:Label>
            </td>
        </tr>
        <tr>
            <td align="center">
                <asp:Label ID="_labelUser" runat="server" CssClass="TechnicalDetail"></asp:Label>
            </td>
        </tr>
        <tr>
            <td align="center">
                <asp:Label ID="_labelWebAuthMode" runat="server" CssClass="TechnicalDetail"></asp:Label>
            </td>
        </tr>
        <tr>
            <td align="center">
                <asp:Label ID="_labelClientAuthMode" runat="server" CssClass="TechnicalDetail"></asp:Label>
            </td>
        </tr>
    </table>
    </form>
</body>
</html>
