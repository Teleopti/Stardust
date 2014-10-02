using System;
using System.Linq.Expressions;
using Rhino.Mocks;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest
{
	public class RhinoMocksStubProxy<T> : IMockProxy<T> where T : class
	{
		private readonly T _mock;

		public RhinoMocksStubProxy(T mock) { _mock = mock; }

		public T Object { get { return _mock; } }

		public void ReturnFor<TResult>(Expression<Func<T, TResult>> expression, TResult result) { _mock.Stub(new Function<T, TResult>(expression.Compile())).Return(result); }

		public void CallbackFor<TResult>(Expression<Func<T, TResult>> expression, Func<TResult> callback) { _mock.Stub(new Function<T, TResult>(expression.Compile())).Do(callback); }

		public void SetupProperty<TProperty>(Expression<Func<T, TProperty>> expression) { }
	}
}