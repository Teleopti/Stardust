﻿using System.Web.Mvc;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
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
			RegisterModelBindersTask.RegisterModelBinders(modelBinderDictionary);
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

	}
}
