<%@ Page EnableTheming="true" Language="C#" AutoEventWireup="true" CodeBehind="ReportViewer.aspx.cs" Inherits="Teleopti.Analytics.Portal.ReportViewer" %>
<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" dir="ltr">
<head runat="server">
    <title>Viewer</title>
</head>
<body style="margin:0; padding:0; background-color:white; background-image:none">
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>
        <rsweb:ReportViewer BackColor="LightSteelBlue" Height="670px"  DocumentMapCollapsed="true" HyperlinkTarget="ReportViewer"  ID="ReportViewer1" runat="server"  Width="98%">
        </rsweb:ReportViewer>       
    </form>
</body>
</html>
