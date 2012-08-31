﻿using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Test;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class TestController : Controller
	{
		private readonly IModifyNow _modifyNow;
		private readonly ISessionSpecificDataProvider _sessionSpecificDataProvider;
		private readonly IAuthenticator _authenticator;
		private readonly IWebLogOn _logon;
		private readonly IBusinessUnitProvider _businessUnitProvider;

		public TestController(IModifyNow modifyNow, ISessionSpecificDataProvider sessionSpecificDataProvider, IAuthenticator authenticator, IWebLogOn logon, IBusinessUnitProvider businessUnitProvider)
		{
			_modifyNow = modifyNow;
			_sessionSpecificDataProvider = sessionSpecificDataProvider;
			_authenticator = authenticator;
			_logon = logon;
			_businessUnitProvider = businessUnitProvider;
		}

		public ViewResult BeforeScenario()
		{
			_sessionSpecificDataProvider.RemoveCookie();
			updateIocNow(null);
			var viewModel = new TestMessageViewModel
								{
									Title = "Setting up for scenario",
									Message = "Setting up for scenario",
									ListItems = new[]
									            	{
									            		"Restoring Ccc7 database",
															"Clearing Analytics database",
															"Removing browser cookie",
															"Setting default implementation for INow"
									            	}
								};
			return View("Message", viewModel);
		}

		public ViewResult Logon(string dataSourceName, string businessUnitName, string userName, string password)
		{
			var result = _authenticator.AuthenticateApplicationUser(dataSourceName, userName, password);
			var businessUnits = _businessUnitProvider.RetrieveBusinessUnitsForPerson(result.DataSource, result.Person);
			var businessUnit = (from b in businessUnits where b.Name == businessUnitName select b).Single();
			_logon.LogOn(businessUnit.Id.Value, dataSourceName, result.Person.Id.Value, AuthenticationTypeOption.Application);
			var viewModel = new TestMessageViewModel
			                	{
			                		Title = "Quick logon",
			                		Message = "Signed in as " + result.Person.Name
			                	};
			return View("Message", viewModel);
		}

		public EmptyResult ExpireMyCookie()
		{
			_sessionSpecificDataProvider.ExpireTicket();
			return new EmptyResult();
		}

		public ViewResult CorruptMyCookie()
		{
			var wrong = Convert.ToBase64String(Convert.FromBase64String("Totally wrong"));
			_sessionSpecificDataProvider.MakeCookie("UserName", DateTime.Now, wrong);
			var viewModel = new TestMessageViewModel
			                	{
			                		Title = "Corrup my cookie",
			                		Message = "Cookie has been corrupted on your command!"
			                	};
			return View("Message", viewModel);
		}

		public ViewResult NonExistingDatasourceCookie()
		{
			var data = new SessionSpecificData(Guid.NewGuid(), "datasource", Guid.NewGuid(), AuthenticationTypeOption.Windows);
			_sessionSpecificDataProvider.StoreInCookie(data);
			var viewModel = new TestMessageViewModel
			                	{
			                		Title = "Incorrect datasource in my cookie",
			                		Message = "Cookie has an invalid datasource on your command!"
			                	};
			return View("Message", viewModel);
		}

		public ViewResult WidgetStylingSample()
		{
			return View();
		}

		public ViewResult SetCurrentTime(DateTime dateSet)
		{
			var utcDate = new DateTime(dateSet.Ticks, DateTimeKind.Utc);
			updateIocNow(utcDate);

			var viewModel = new TestMessageViewModel
			{
				Title = "Time changed on server!",
				Message = "INow component now thinks time is " + utcDate
			};
			ViewBag.SetTime = "hello";

			return View("Message", viewModel);
		}

		private void updateIocNow(DateTime? dateTimeSet)
		{
			_modifyNow.SetNow(dateTimeSet);
		}
	}
}
