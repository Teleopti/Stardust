using System;
using System.Collections.Generic;
using Autofac;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class TestDoubles : IDisposable
	{
		public readonly List<TestDoubleRegistration> Registrations = new List<TestDoubleRegistration>();

		public void RegisterFromPreviousContainer(ContainerBuilder builder)
		{
			Registrations.ForEach(r =>
			{
				r.RegistrationAction?.Invoke(builder);
			});
		}

		public void Dispose()
		{
			Registrations.ForEach(r =>
			{
				var disposable = r.Instance as IDisposable;
				disposable?.Dispose();
			});
			Registrations.Clear();
		}
	}

	public class TestDoubleRegistration
	{
		public Action<ContainerBuilder> RegistrationAction;
		public object Instance;
	}
}