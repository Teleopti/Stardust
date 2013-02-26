﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.DataProvider
{
	[TestFixture]
	public class PreferenceTemplateOptionsProviderTest
	{
		[Test]
		public void ShouldRetrievePreferenceTemplates()
		{
			var extendedPreferenceTemplateRepository = MockRepository.GenerateMock<IExtendedPreferenceTemplateRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = new Person();
			loggedOnUser.Stub(u => u.CurrentUser()).Return(person);
			var templates = new List<IExtendedPreferenceTemplate>();
			extendedPreferenceTemplateRepository.Stub(x => x.FindByUser(person)).Return(templates);

			var target = new PreferenceTemplateOptionsProvider(loggedOnUser, extendedPreferenceTemplateRepository);

			var result = target.RetrievePreferenceTemplateOptions();

			result.Should().Be.SameInstanceAs(templates);
		}
	}
}