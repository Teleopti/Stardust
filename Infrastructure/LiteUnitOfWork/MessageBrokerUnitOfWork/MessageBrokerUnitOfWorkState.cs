using System;
using System.Collections;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork
{
	public class MessageBrokerUnitOfWorkState : ICurrentMessageBrokerUnitOfWork, IMessageBrokerUnitOfWorkScope
	{
		[ThreadStatic] 
		private static IDictionary _threadState;
		private const string itemsKey = "MessageBrokerUnitOfWork";

		private readonly ICurrentHttpContext _httpContext;
		private readonly IConfigReader _configReader;
		private ISessionFactory _sessionFactory;

		public MessageBrokerUnitOfWorkState(ICurrentHttpContext httpContext, IConfigReader configReader)
		{
			_httpContext = httpContext;
			_configReader = configReader;
		}

		private void configure()
		{
			if (_sessionFactory != null) return;
			var configuration = new Configuration()
				.DataBaseIntegration(db =>
				{
					db.ConnectionString = _configReader.ConnectionStrings_DontUse["MessageBroker"].ConnectionString;
					db.Dialect<MsSql2008Dialect>();
				});
			_sessionFactory = configuration.BuildSessionFactory();
		}

		public void Start()
		{
			configure();
			setItem(itemsKey + "Started", true);
		}

		public ILiteUnitOfWork Current()
		{
			if (!getItem<bool>(itemsKey + "Started"))
				return null;
			var uow = getItem<LiteUnitOfWork>(itemsKey);
			if (uow != null) return uow;
			uow = new LiteUnitOfWork(_sessionFactory.OpenSession());
			setItem(itemsKey, uow);
			return uow;
		}

		public void End(Exception exception)
		{
			setItem(itemsKey + "Started", null);
			var uow = getItem<LiteUnitOfWork>(itemsKey);
			if (uow == null)
				return;
			setItem(itemsKey, null);
			if (exception == null)
				uow.Commit();
			uow.Dispose();
		}

		private T getItem<T>(string key)
		{
			var items = state();
			if (items.Contains(key))
				return (T)items[key];
			return default(T);
		}

		private void setItem(string key, object o)
		{
			if (o == null)
			{
				state().Remove(key);
				return;
			}
			state()[key] = o;
		}

		private IDictionary state()
		{
			if (_httpContext.Current() != null)
				return _httpContext.Current().Items;
			return _threadState ?? (_threadState = new Dictionary<string, object>());
		}
	}

}