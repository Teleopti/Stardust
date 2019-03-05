using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest
{
	public interface ILogOnOffContext
	{
		void Login();
		void Logout();
	}

	public class DatabaseTestAttribute : InfrastructureTestAttribute, ILogOnOffContext
	{
		private IPerson person;
		private IDisposable _login;
		public IDataSourceForTenant DataSourceForTenant;
		
		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			extend.AddService(this);
		}

		protected override void BeforeTest()
		{
			base.BeforeTest();
			var dataSource = DataSourceForTenant.Tenant(InfraTestConfigReader.TenantName());
			_login = SetupFixtureForAssembly.CreatePersonAndLogin(dataSource, out person);
		}

		protected override void AfterTest()
		{
			base.AfterTest();

			_login?.Dispose();
			_login = null;
		}

		public void Login()
		{
			SetupFixtureForAssembly.Login(person);
		}

		public new void Logout()
		{
			SetupFixtureForAssembly.Logout();
		}
	}
}