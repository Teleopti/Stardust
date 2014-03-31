<%@ Page EnableTheming="true" Language="C#" AutoEventWireup="true" CodeBehind="ReportViewer.aspx.cs" Inherits="Teleopti.Analytics.Portal.ReportViewer" %>
<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" dir="ltr">
<head runat="server">
    <title>Viewer</title>
</head>
<body style="margin:0; padding:0">
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>
        <rsweb:ReportViewer BackColor="#96bacc" Height="670px" ShowBackButton="False" DocumentMapCollapsed="true" HyperlinkTarget="ReportViewer"  ID="ReportViewer1" runat="server"  Width="100%">
        </rsweb:ReportViewer>       
    </form>
</body>
</html>
