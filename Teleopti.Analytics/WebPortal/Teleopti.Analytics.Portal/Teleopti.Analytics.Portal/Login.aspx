<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Teleopti.Analytics.Portal.Login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Teleopti WFM Login</title>
    <link rel="shortcut icon" href="~/Images/ccc_menu.ico"/>
</head>
<body >
    <form id="form1" runat="server" >  
		 <div align="center" style="width: 100%; min-height: 50px;">
            <asp:Label ID="_labelInfo" runat="server" CssClass="TechnicalDetailHeader" EnableViewState="false"></asp:Label>
		</div>   
		<div style="width: 100%; padding-top: 200px" align="center">
			<asp:Login DisplayRememberMe="true" ID="Login1" SkinID="Login" runat="server" DestinationPageUrl="~/Selection.aspx"></asp:Login>
		</div>
	    
    </form>
</body>
</html>
