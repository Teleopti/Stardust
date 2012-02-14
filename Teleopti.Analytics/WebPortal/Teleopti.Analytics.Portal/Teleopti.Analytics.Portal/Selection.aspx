<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Selection.aspx.cs" Inherits="Teleopti.Analytics.Portal.Selection" ValidateRequest="false" %>
<%@ Register TagPrefix="Analytics"  Namespace="Teleopti.Analytics.Parameters" Assembly="Teleopti.Analytics.Parameters" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>


<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<script type="text/javascript">

function window.onload()
		{
		  setSize();
		}
		
function setSize(){
		//alert(document.all.AllParts.offsetHeight);
		//alert(window.document.all.menuTD.style.height);
		try{
		//alert(document.body.offsetHeight);
		   window.document.all.ViewerFrame.style.height = document.body.offsetHeight - 0;
		   //alert(window.document.all.Viewer.style.height);
		   //window.document.all.ViewerFrame.style.width = document.all.AllParts.offsetWidth;
			}
			catch(e){}
		}
</script>
<html xmlns="http://www.w3.org/1999/xhtml" dir="ltr">
<head runat="server">
    <title>Selection</title>
    <link rel="shortcut icon" href="~/Images/ccc_menu.ico"/>
</head>
<body>
    <form id="aspnetForm" runat="server">
    <table width="100%" border="0" cellpadding="1" cellspacing="0"><tr><td style="width:100%"></td><td style="white-space:nowrap" align="right"><asp:Label ID="LoggedOnUser" runat="server" Text=""></asp:Label></td></tr></table>
    
   <asp:ScriptManager ID="ScriptManager1" EnablePartialRendering="true" runat="server"  EnableScriptGlobalization="true" EnableScriptLocalization="true" />
    
    <div class="Panel">
        <asp:HiddenField ID="HiddenID" runat="server" />
        <asp:HiddenField ID="HiddenUserID" runat="server" />
        <asp:HiddenField ID="ParametersValid" runat="server" Value="0" />
        <ajaxToolkit:CollapsiblePanelExtender ID="CPEReports" runat="Server" TargetControlID="Reports_ContentPanel"
            ExpandControlID="tdTogglePanel" CollapseControlID="tdTogglePanel"
            Collapsed="false" ExpandDirection="Vertical" ImageControlID="ImageReportsToggle"
            ExpandedImage="~/images/collapse.jpg" ExpandedText='xxCollapse' CollapsedImage="~/images/expand.jpg"
            CollapsedText='xxExpand' SuppressPostBack="true" />
        <asp:Panel ID="Reports_HeaderPanel" runat="server" Style="cursor: pointer;">
            <table cellspacing="0" width="100%" border="0" cellpadding="0">
                <tr>
                    <td id="tdTogglePanel" runat="server" style="vertical-align: middle" class="Caption">
                        <asp:Image ID="ImageReportsToggle" runat="server" ImageUrl="~/images/collapse.jpg" />
                        <asp:Label ID="labelRepCaption" runat="server" Text="xxxRapportnamnet"></asp:Label>
                    </td>
                    <td colspan="3" style="vertical-align: middle" class="Caption">
                        <div style="float: left; width: 50px">
                        </div>
                        <div style="float: left;">
                            <asp:Image ID="ImageLoading" Visible="false" runat="server" SkinID="Loading" />
                            <asp:UpdateProgress ID="Progress" runat="server">
                                <ProgressTemplate>
                                    <asp:Image ID="Image1" runat="server" SkinID="Loading" />
                                </ProgressTemplate>
                            </asp:UpdateProgress>
                        </div>
                    </td>
                    <td align="right" style="vertical-align: middle; width: 25px;" class="Caption">
                        <asp:ImageButton ID="ImageButtonHelp" runat="server" ImageUrl="~/images/Question_16x16.png" ToolTip="xxHelp" OnClientClick="javascript:return false;" />
                    </td>
                </tr>
            </table>
        </asp:Panel>
        <asp:Panel ID="Reports_ContentPanel" runat="server" Height="0px">
            <table cellspacing="0" width="100%" border="0" cellpadding="1">
                <tr>
                    <td class="DetailsView" style="width: 70%; border-right: solid 1 blue;">
                        <Analytics:Selector LabelWidth="30%" List1Width="75%" ID="Parameter" runat="server" OnInit="Selector_OnInit">
                        </Analytics:Selector>
                    </td>
                </tr>
            </table>
            <table width="100%" cellpadding="0" cellspacing="0">
                <tr>
                    <td align="center">
                        <asp:ImageButton OnClick="ButtonShow_Click" ID="buttonShow" SkinID="Show" ToolTip='' 
                            runat="server" />
                        <asp:Label ID="labelPermissionDenied" runat="server" ForeColor="Red" Font-Size="Large" Visible="false"></asp:Label>
                    </td>
                </tr>
            </table>
        </asp:Panel>
    </div>
    
        <iframe  runat="server" id="ViewerFrame" name="ViewerFrame" style="height:670px; overflow:hidden; margin:0px padding:0px; display:none;" src="" width="100%" frameborder="1"  allowtransparency="true" ></iframe>
    
    <!--<div class="Panel" style="overflow:hidden;border-top:none;height:680px" id="Viewer" ></div>    -->     
    
    </form>
</body>
</html>
