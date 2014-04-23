<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Auth.aspx.cs" Inherits="Teleopti.Ccc.Sdk.WcfHost.WebForm1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:RadioButtonList ID="RadioButtonList1" runat="server">
            <asp:ListItem Selected="True">ClickOnce</asp:ListItem>
            <asp:ListItem>URL Protocol</asp:ListItem>
        </asp:RadioButtonList>
            URL: <asp:TextBox ID="url" runat="server" Width="311px"></asp:TextBox>
        <br />
            <asp:DropDownList runat="server" ID="dataSources" />
    <asp:TextBox runat="server" ID="userName" />
    <asp:Button runat="server" ID="startMyTime" Text="Start MyTime" onclick="Unnamed1_Click" />
    </div>
    </form>
</body>
</html>
