<%@ Page Language="C#" AutoEventWireup="true" EnableTheming="false"%>
<head runat=server /> 
<html>
<body>
<p>
<b>HttpContext.Current.User.Identity.Name:</b>
<%Response.Write(HttpContext.Current.User.Identity.Name);%>
<p>
<b>Request.ServerVariables["AUTH_USER"]:</b>
<%Response.Write(Request.ServerVariables["AUTH_USER"]);%>
</p>
</body>
</html> 