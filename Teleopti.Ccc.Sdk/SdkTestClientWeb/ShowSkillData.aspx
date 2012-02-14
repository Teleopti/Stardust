<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ShowSkillData.aspx.cs" Inherits="SdkTestClientWeb.ShowSkillData" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:GridView runat="server" ID="_skillDataGrid">
            <Columns>
                <asp:TemplateField HeaderText="Start">
                    <ItemTemplate>
                        <asp:Label ID="lblstart" runat="server" Text='<%#Eval("Period.UtcStartTime") %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>
    </form>
</body>
</html>
