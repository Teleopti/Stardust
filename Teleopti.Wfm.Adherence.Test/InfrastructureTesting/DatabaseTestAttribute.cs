using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.InfrastructureTesting
{
	public interface ILogOnOffContext
	{
		void Login();
		void Logout();
	}

	public class DatabaseTestAttribute : InfrastructureTestAttribute, ILogOnOffContext
	{
		private (IPerson Person, IBusinessUnit BusinessUnit) _data;

		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			extend.AddService(this);
		}

		protected override void BeforeTest()
		{
			_data = InfrastructureTestSetup.Before();
			base.BeforeTest();
			base.Login(_data.Person, _data.BusinessUnit);
		}

		protected override void AfterTest()
		{
			base.AfterTest();
			base.Logout();
			InfrastructureTestSetup.After();
		}

		public void Login()
		{
			base.Login(_data.Person, _data.BusinessUnit);
		}

		public new void Logout()
		{
			base.Logout();
		}
	}
}