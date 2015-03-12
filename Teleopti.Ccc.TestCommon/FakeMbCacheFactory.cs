using System;
using System.Linq.Expressions;
using MbCache.Core;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeMbCacheFactory : IMbCacheFactory
	{
		public T Create<T>(params object[] parameters) where T : class
		{
			return default(T);
		}

		public T ToCachedComponent<T>(T uncachedComponent) where T : class
		{
			return uncachedComponent;
		}

		public void Invalidate<T>()
		{
		}

		public void Invalidate(object component)
		{
		}

		public void Invalidate<T>(T component, Expression<Func<T, object>> method, bool matchParameterValues)
		{
		}

		public bool IsKnownInstance(object component)
		{
			throw new NotImplementedException();
		}

		public Type ImplementationTypeFor(Type componentType)
		{
			throw new NotImplementedException();
		}

		public void DisableCache<T>(bool evictCacheEntries = true)
		{
			throw new NotImplementedException();
		}

		public void EnableCache<T>()
		{
			throw new NotImplementedException();
		}
	}
}