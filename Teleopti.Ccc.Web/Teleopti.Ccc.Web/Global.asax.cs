﻿using System;
using System.Security.Policy;
using System.Security.Principal;
using System.Web;
using Autofac;
using Autofac.Integration.Wcf;
using log4net;
using log4net.Config;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server.Adherence;
using Teleopti.Ccc.Web.Areas.Rta;
using ContainerBuilder = Teleopti.Ccc.Rta.Server.ContainerBuilder;

namespace Teleopti.Ccc.Web
{
	public class Global : System.Web.HttpApplication
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Global));

		protected void Application_Start(object sender, EventArgs e)
		{
			XmlConfigurator.Configure();
			var container = buildIoc();
			AutofacHostFactory.Container = container;

			container.Resolve<IRtaDataHandler>();
			container.Resolve<AdherenceAggregatorInitializor>().Initialize();
			setDefaultGenericPrincipal();
		}

		private static IContainer buildIoc()
		{
			var builder = ContainerBuilder.CreateBuilder();
			builder.RegisterType<TeleoptiRtaService>().SingleInstance();
			builder.RegisterModule(new DateAndTimeModule());
			return builder.Build();
		}

		private static void setDefaultGenericPrincipal()
		{
			try
			{
				Logger.Debug("Trying to set default generic principal.");
				AppDomain.CurrentDomain.SetThreadPrincipal(new GenericPrincipal(new GenericIdentity("Anonymous"), new string[] { }));
			}
			catch (PolicyException policyException)
			{
				Logger.Debug("Failed to set thread principal for app domain, because it was already set.", policyException);
			}
		}

		protected void Session_Start(object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{
			HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
			if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
			{
				//These headers are handling the "pre-flight" OPTIONS call sent by the browser
				HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
				HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
				HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");
				HttpContext.Current.Response.End();
			}
		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{
		}

		protected void Application_Error(object sender, EventArgs e)
		{

		}

		protected void Session_End(object sender, EventArgs e)
		{

		}

		protected void Application_End(object sender, EventArgs e)
		{

		}
	}
}