<%@ Page Language="C#" Inherits="AuthBridge.Web.xrds, Authbridge.Web" %>
<%
	var uri = new Uri(Request.Url, Response.ApplyAppPathModifier("~/response"));
	var baseuri = new Uri(Request.Url, Response.ApplyAppPathModifier("~/"));
 %><?xml version="1.0" encoding="UTF-8"?>
<xrds:XRDS
	xmlns:xrds="xri://$xrds"
	xmlns:openid="http://openid.net/xmlns/1.0"
	xmlns="xri://$xrd*($v*2.0)">
	<XRD>
		<Service priority="10">
			<Type>http://specs.openid.net/auth/2.0/return_to</Type>
			<URI priority="1"><%= uri %></URI>
			<URI priority="2"><%= baseuri %></URI>
		</Service>
	</XRD>
</xrds:XRDS>
