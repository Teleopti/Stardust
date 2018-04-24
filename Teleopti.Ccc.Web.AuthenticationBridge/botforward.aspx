<%@ Page Language="C#" AutoEventWireup="true" EnableSessionState="False" EnableViewState="False" EnableViewStateMac="False" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
<title>Bot forwarder</title>
</head>
<body>
<%
Response.Cache.SetCacheability(HttpCacheability.NoCache);

var action = Request.Form["wctx"].Replace("ru=","");
if (string.IsNullOrWhiteSpace(action))
{
	Response.Write("Empty return url not allowed");
	Response.End();
}
if (!action.StartsWith("http://localhost:3979"))
{
	Response.Write("Not an allowed return url");
	Response.End();
}

Response.Write("<form id=\"botforward\" action=\"" + action + "\" method=\"POST\">");

foreach (string strKey in Request.Form.AllKeys)
	Response.Write("<input type=\"hidden\" name=\"" + strKey + "\" value=\"" + Server.HtmlEncode(Request.Form[strKey]) + "\" />\n");

Response.Write("</form>");
%>
<script type="text/javascript">
window.onload = function(){
  document.forms['botforward'].submit();
}
</script>
</form>
</body>
</html>