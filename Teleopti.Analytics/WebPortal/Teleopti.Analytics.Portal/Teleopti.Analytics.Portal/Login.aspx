<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Teleopti.Analytics.Portal.Login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>TELEOPTI CCC Login</title>
    <link rel="shortcut icon" href="~/Images/ccc_menu.ico"/>
    <link href="App_Themes/Theme1/Styles.css" rel="stylesheet" type="text/css" />
</head>
<body style="filter: progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr=  '#CADAF9' , EndColorStr= '#fcfcfc' );">
    <form id="form1" runat="server" >
        <table cellspacing="0" cellpadding="0" width="100%" border="0">
			<tr id="topmenu">
				<td align="center" valign="bottom" style="height:50px">
                    <asp:Label ID="_labelInfo" runat="server" CssClass="TechnicalDetailHeader" EnableViewState="false"></asp:Label>
				</td>   
			</tr>
        <tr>
            <td style="height:300px" valign="middle" align="center">
                <asp:Login  DisplayRememberMe="false" ID="Login1" 
                SkinID="Login" runat="server" DestinationPageUrl="~/Selection.aspx">
                </asp:Login>
            </td>
        </tr>
    </table>
    
    </form>
</body>
</html>
