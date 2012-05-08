using System;
using System.Reflection;
using NHibernate.Bytecode;
using NHibernate.Engine;
using NHibernate.Proxy;
using NHibernate.Proxy.DynamicProxy;
using NHibernate.Proxy.Poco;
using NHibernate.Type;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	/// <summary>
	/// Just a temporarly solution until
	/// https://github.com/nhibernate/nhibernate-core/pull/98
	/// has been pulled into nh core and we have upgraded
	/// </summary>
	public class FasterProxyFactoryFactory : IProxyFactoryFactory
	{
		public IProxyFactory BuildProxyFactory()
		{
			return new FasterProxyFactory();
		}

		public IProxyValidator ProxyValidator
		{
			get { return new DynProxyTypeValidator(); }
		}

		public bool IsInstrumented(Type entityClass)
		{
			return true;
		}

		public bool IsProxy(object entity)
		{
			return entity is INHibernateProxy;
		}
	}

	public class FasterProxyFactory : DefaultProxyFactory
	{
		private readonly ProxyFactory _factory = new ProxyFactory();

		public override INHibernateProxy GetProxy(object id, ISessionImplementor session)
		{
			var initializer = new FasterLazyInitializer(EntityName, PersistentClass, id, GetIdentifierMethod, SetIdentifierMethod, ComponentIdType, session);
			return (INHibernateProxy) _factory.CreateProxy(PersistentClass, initializer, Interfaces);
		}
	}

	public class FasterLazyInitializer : BasicLazyInitializer, IInterceptor
	{
		[NonSerialized]
		private static readonly MethodInfo exceptionInternalPreserveStackTrace =
			typeof (Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);

		public FasterLazyInitializer(string entityName, Type persistentClass, object id, MethodInfo getIdentifierMethod,
		                       MethodInfo setIdentifierMethod, IAbstractComponentType componentIdType,
		                       ISessionImplementor session)
			: base(entityName, persistentClass, id, getIdentifierMethod, setIdentifierMethod, componentIdType, session)
		{
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public object Intercept(InvocationInfo info)
		{
			object returnValue;
			try
			{
				//////////////////////added this////////////////////////////
				if (info.TargetMethod.GetParameters().Length == 0 && isEqualToIdentifierMethod(info.TargetMethod))
				{
					return Identifier;
				}
				/////////////////////////////////////////////////////////////

				returnValue = base.Invoke(info.TargetMethod, info.Arguments, info.Target);
				if (returnValue != InvokeImplementation)
				{
					return returnValue;
				}
				returnValue = info.TargetMethod.Invoke(GetImplementation(), info.Arguments);
			}
			catch (TargetInvocationException ex)
			{
				exceptionInternalPreserveStackTrace.Invoke(ex.InnerException, new Object[] {});
				throw ex.InnerException;
			}

			return returnValue;
		}

		private bool isEqualToIdentifierMethod(MethodInfo method)
		{
			return getIdentifierMethod != null &&
			       (method.Name.Equals(getIdentifierMethod.Name) && method.ReturnType == getIdentifierMethod.ReturnType);
		}
	}
}
