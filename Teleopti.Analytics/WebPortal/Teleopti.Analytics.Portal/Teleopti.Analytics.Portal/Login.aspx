<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Teleopti.Analytics.Portal.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Teleopti WFM Login</title>
	<link href="bootstrap.min.css" rel="stylesheet" />
	<link href="App_Themes/Theme1/Styles.css" rel="stylesheet" />
</head>
<body >
    <form id="form1" runat="server" >  
		 <div align="center" style="width: 100%; min-height: 50px;">
            <asp:Label ID="_labelInfo" runat="server" CssClass="TechnicalDetailHeader" EnableViewState="false"></asp:Label>
		</div>   
		<div style="width: 100%; padding-top: 200px" align="center">
			<asp:Login DisplayRememberMe="false" TextBoxStyle="CssClass='LoginTextBox'" ID="Login1" SkinID="Login" runat="server" DestinationPageUrl="~/PmContainer.aspx"></asp:Login>
		</div>
	    
    </form>
</body>

</html>

