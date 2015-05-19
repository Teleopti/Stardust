using System;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork
{
	public class MessageBrokerUnitOfWorkAspect : IMessageBrokerUnitOfWorkAspect
	{
		private readonly IConfigReader _configReader;
		private readonly MessageBrokerUnitOfWorkState _state;
		private ISessionFactory _sessionFactory;

		public MessageBrokerUnitOfWorkAspect(ICurrentHttpContext httpContext, IConfigReader configReader)
		{
			_configReader = configReader;
			_state = new MessageBrokerUnitOfWorkState(httpContext);
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			configure();
			_state.Set(new LiteUnitOfWork(_sessionFactory.OpenSession()));
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			var uow = _state.Get();
			_state.Set(null);
			if (exception == null)
				uow.Commit();
			uow.Dispose();
		}

		private void configure()
		{
			if (_sessionFactory != null) return;
			var configuration = new Configuration();
			configuration.SetProperty(NHibernate.Cfg.Environment.ConnectionString,
				_configReader.ConnectionStrings["MessageBroker"].ConnectionString);
			configuration.SetProperty(NHibernate.Cfg.Environment.Dialect, typeof(MsSql2008Dialect).AssemblyQualifiedName);
			_sessionFactory = configuration.BuildSessionFactory();
		}

	}
}