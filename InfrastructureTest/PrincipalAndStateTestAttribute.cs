﻿using System;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class PrincipalAndStateTestAttribute : InfrastructureTestAttribute, IPrincipalAndStateContext
	{
		public FakeMessageSender MessageSender;
		private IPerson person;
		private IDisposable _login;

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			system.AddService(this);
		}

		protected override void BeforeTest()
		{
			base.BeforeTest();

			_login = SetupFixtureForAssembly.CreatePersonAndLogin(out person);
			MessageSender.AllNotifications.Clear();
		}

		protected override void AfterTest()
		{
			base.AfterTest();

			_login.Dispose();
			_login = null;
		}

		public void Login()
		{
			SetupFixtureForAssembly.Login(person);
		}

		public void Logout()
		{
			SetupFixtureForAssembly.Logout();
		}
	}

	public interface IPrincipalAndStateContext
	{
		void Login();
		void Logout();
	}
}