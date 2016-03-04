using System;
using System.Collections.Generic;
using Autofac;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class TestDoubles
	{
		public class Registration
		{
			public Action<ContainerBuilder> Action;
		}

		private readonly List<Registration> _registrations = new List<Registration>();

		public Registration Register(Action<ContainerBuilder> action)
		{
			var registration = new Registration
			{
				Action = action
			};
			_registrations.Add(registration);
			return registration;
		}

		public void RegisterFromPreviousContainer(ContainerBuilder builder)
		{
			_registrations.ForEach(r =>
			{
				if (r.Action != null)
					r.Action.Invoke(builder);
			});
		}
	}
}