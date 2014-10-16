﻿using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	public class ForecasterModuleTest
	{
		[Test, Ignore("Will fix when ValidatedVolumeDayRepositor.MatchDays is moved out of the repo.")]
		public void ShouldResolveQuickForecast()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule<CommonModule>();

			using (var container = containerBuilder.Build())
			{
				container.Resolve<IQuickForecaster>()
					.Should().Not.Be.Null();
			}
		}
	}
}