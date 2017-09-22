<%@ Page Title="" Language="C#" MasterPageFile="Site1.Master" AutoEventWireup="true" CodeBehind="ShowReport.aspx.cs" Inherits="Teleopti.Analytics.Portal.PerformanceManager.ShowReport" EnableViewState="true" %>
<%@ Register src="View/AnalyzerReportView.ascx" tagname="AnalyzerReportView" tagprefix="uc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ReportTitle" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ContentPlaceHolder3" runat="server">
    <uc1:AnalyzerReportView ID="AnalyzerReportView1" runat="server" />
</asp:Content>
