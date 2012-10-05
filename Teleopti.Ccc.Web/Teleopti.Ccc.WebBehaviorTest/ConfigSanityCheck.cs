using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Teleopti.Ccc.WebBehaviorTest
{

	[TestFixture]
	public class ConfigSanityCheck
	{
		[Test]
		public void ShouldLoadConfig()
		{
			Assert.That(ConfigurationManager.AppSettings["configSanityCheck"], 
				Is.EqualTo("Yes, configuration is loaded!"), 
				"No! Configuration is not loaded!!!");
		}

		[Test]
		public void ShouldHaveThreadApartmentStateSTAWithoutAttribute()
		{
			Assert.That(Thread.CurrentThread.GetApartmentState(), Is.EqualTo(ApartmentState.STA));
		}

		[Test, RequiresSTA]
		public void ShouldHaveThreadApartmentStateSTAWith()
		{
			Assert.That(Thread.CurrentThread.GetApartmentState(), Is.EqualTo(ApartmentState.STA));
		}
	}
}
