using System;
using System.Web.Mvc;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core.Startup;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Startup
{
	[TestFixture]
	public class RegisterModelBindersTaskTest
	{
		private ModelBinderDictionary modelBinderDictionary;

		[SetUp]
		public void Setup()
		{
			modelBinderDictionary = new ModelBinderDictionary();
			// Must be called in Task.Execute 
			var target = new RegisterModelBindersTask(null,null);
			target.BindersGetter = () => modelBinderDictionary;
			target.Execute();
		}

		[Test]
		public void ShouldRegisterDateOnlyModelBinder()
		{
			modelBinderDictionary.ContainsKey(typeof(DateOnly)).Should().Be.True();
			modelBinderDictionary.ContainsKey(typeof(DateOnly?)).Should().Be.True();
		}

		[Test]
		public void ShouldRegisterTimeOfDayModelBinder()
		{
			modelBinderDictionary.ContainsKey(typeof(TimeOfDay)).Should().Be.True();
			modelBinderDictionary.ContainsKey(typeof(TimeOfDay?)).Should().Be.True();
		}

		[Test]
		public void ShouldRegisterTimeSpanModelBinder()
		{
			modelBinderDictionary.ContainsKey(typeof(TimeSpan)).Should().Be.True();
			modelBinderDictionary.ContainsKey(typeof(TimeSpan?)).Should().Be.True();
		}

		[Test]
		public void ShouldRegisterAuthenticationModelBinder()
		{
			modelBinderDictionary.ContainsKey(typeof(IAuthenticationModel)).Should().Be.True();
		}

	}
}
