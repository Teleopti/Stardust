<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Schedule.aspx.cs" Inherits="SdkTestClientWeb.Schedule" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <asp:HyperLink runat="server" NavigateUrl="~/OrgTree.aspx" Text="Bakelibak" />
    
    <asp:GridView runat="server" ID="layersGrid" AutoGenerateColumns="false">
    <Columns>
        <asp:TemplateField HeaderText="Aktivitet">
            <ItemTemplate>
                <asp:Label ID="lblstart" runat="server" text='<%#Eval("Activity.Description") %>'></asp:Label>
            </ItemTemplate>                
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Start">
            <ItemTemplate>
                <asp:Label ID="lblstart" runat="server" text='<%#Eval("Period.UtcStartTime") %>'></asp:Label>
            </ItemTemplate>                
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Slut">      
            <ItemTemplate>
                <asp:Label ID="lblend" runat="server" text='<%#Eval("Period.UtcEndTime") %>'></asp:Label>
            </ItemTemplate>
        </asp:TemplateField>
    </Columns>
        </asp:GridView>
        
        <asp:Literal runat="server" ID="shiftCat" /><br />
        <asp:Literal runat="server" ID="abs" /><br />
        <asp:Literal runat="server" ID="PersonAcc" /><br />
        <asp:Literal runat="server" ID="AccountInfo" />
        <br /><br />
        Förkorta första mainshiftlagret med en minut och spara
        <asp:Button ID="changeSchedule" runat="server" Text="Do it!" OnClick="changeSchedule_Click" />
        
        <br /><br />
        Spara en icke-förändrad schedulepart
        <asp:Button ID="nonChangedSchedule" runat="server" Text="Do it!" OnClick="nonChangedSchedule_Click" />
    </div>
    </form>
</body>
</html>
