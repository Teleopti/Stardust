using System;
using System.Collections.Generic;
using Autofac;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class TestDoubles : IDisposable
	{
		public class Registration
		{
			public Action<ContainerBuilder> Action;
			public object Instance;
		}

		private readonly List<Registration> _registrations = new List<Registration>();

		public Registration Register(Action<ContainerBuilder> action, object instance)
		{
			var registration = new Registration
			{
				Action = action,
				Instance = instance
			};
			_registrations.Add(registration);
			return registration;
		}

		public void RegisterFromPreviousContainer(ContainerBuilder builder)
		{
			_registrations.ForEach(r =>
			{
				r.Action?.Invoke(builder);
			});
		}

		public void Dispose()
		{
			_registrations.ForEach(r =>
			{
				var disposable = r.Instance as IDisposable;
				disposable?.Dispose();
			});
			_registrations.Clear();
		}
	}
}