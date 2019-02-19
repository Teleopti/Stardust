using System.Collections.Generic;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.InfrastructureTesting
{
	public class ReadModelUnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		private (IPerson Person, IBusinessUnit BusinessUnit) _data;

		protected override void BeforeInject(IComponentContext container)
		{
			_data = InfrastructureTestSetup.Before();
			base.BeforeInject(container);
		}

		protected override void BeforeTest()
		{
			base.BeforeTest();
			base.Login(_data.Person, _data.BusinessUnit);
			Resolve<IEnumerable<IAspect>>()
				.OfType<IReadModelUnitOfWorkAspect>()
				.Single()
				.OnBeforeInvocation(null);
		}

		protected override void AfterTest()
		{
			Resolve<IEnumerable<IAspect>>()
				.OfType<IReadModelUnitOfWorkAspect>()
				.Single()
				.OnAfterInvocation(null, null);
			base.Logout();
			base.AfterTest();
			InfrastructureTestSetup.After();
		}
	}
}