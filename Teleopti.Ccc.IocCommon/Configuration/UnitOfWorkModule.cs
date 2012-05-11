using System;
using System.Collections.Generic;
using System.Threading;
using Autofac;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Module = Autofac.Module;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class UnitOfWorkModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.Register(c =>
			{
				var principal = c.Resolve<ICurrentTeleoptiPrincipal>().Current();
				if (principal != null && principal.Identity.IsAuthenticated)
					return UnitOfWorkFactory.Current;

				return new NotInitializedUnitOfWorkFactory();
			})
				 .As<IUnitOfWorkFactory>()
				 .InstancePerDependency()
				 .ExternallyOwned();
		}

		private class NotInitializedUnitOfWorkFactory : IUnitOfWorkFactory
		{
			public void Dispose()
			{
				throw new NotImplementedException();
			}

			public IUnitOfWork CreateAndOpenUnitOfWork()
			{
				throw new NotImplementedException();
			}

			public IUnitOfWork CreateAndOpenUnitOfWork(params IEnumerable<IAggregateRoot>[] rootCollectionsCollection)
			{
				throw new NotImplementedException();
			}

			public IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork()
			{
				throw new NotImplementedException();
			}

			public string Name
			{
				get { throw new NotImplementedException(); }
			}

			public void Close()
			{
				throw new NotImplementedException();
			}

			public long? NumberOfLiveUnitOfWorks
			{
				get { throw new NotImplementedException(); }
			}

			public IUnitOfWork CurrentUnitOfWork()
			{
				throw new NotImplementedException();
			}

			public bool HasCurrentUnitOfWork()
			{
				throw new NotImplementedException();
			}

			public IAuditSetter AuditSetting
			{
				get { throw new NotImplementedException(); }
			}
		}
	}
}