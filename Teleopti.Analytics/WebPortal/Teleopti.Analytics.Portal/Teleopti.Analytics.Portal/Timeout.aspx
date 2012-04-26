<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Timeout.aspx.cs" Inherits="Teleopti.Analytics.Portal.Timeout" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
	<script type="text/javascript">
		var SecondCounter = 0;
		var TotalSeconds = 5;

		function CreateTimer() {
			window.setTimeout("Tick()", 1000);
		}

		function Tick() {
			SecondCounter += 1;
			if (SecondCounter == TotalSeconds) {
				redirect();
				return;
			}
			window.setTimeout("Tick()", 1000);
		}
		
		function redirect() {
			var form = window.document.getElementById('form1');
			if (form) {
				form.submit();
			}
			return false;
		}
	</script>
</head>
<body style="filter: progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr='#CADAF9', EndColorStr= '#fcfcfc');" onload="CreateTimer();">
    <form id="form1" runat="server">
	<div align="center">
	<table>
		<tr>
			<td style="height:50px"></td>
		</tr>
		<tr>
			<td>
				<asp:Label ID="labelHeader" runat="server" CssClass="TechnicalDetailHeader"></asp:Label>
			</td>
		</tr>
		<tr>
			<td>
				<asp:Label ID="labelText" runat="server" CssClass="TechnicalDetail"></asp:Label>
			</td>
		</tr>
		<tr>
			<td>
				<a id="redirectLink" runat="server" class="TechnicalDetail" href="#" onclick="javascript:redirect();"></a>
			</td>
		</tr>
	</table>
	</div>
    </form>
</body>
</html>
