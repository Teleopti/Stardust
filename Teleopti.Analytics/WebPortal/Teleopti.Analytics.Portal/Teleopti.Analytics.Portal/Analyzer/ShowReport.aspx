<%@ Page  Language="C#" AutoEventWireup="true" CodeBehind="ShowReport.aspx.cs" Inherits="Teleopti.Analytics.Portal.Analyzer.ShowReport" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Report</title>

</head>
<body onunload="return window_onunload()">
    <form id="form1" runat="server">
        <table width="100%" style="height:100%">
        
            <tr>
                <td>
                    <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>
                </td>
            </tr> 
            <tr style="height:99%">
                <td>
                    <iframe scrolling="no" frameborder="0" style="height:600px" width="100%" src="<%= ReportUrl %>">                    
                    </iframe>
                </td>
            </tr>
        </table>
    </form>
</body>
</html>
