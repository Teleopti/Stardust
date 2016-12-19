﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Settings.Mapping
{
	[TestFixture]
	public class SettingsViewModelMappingTest
	{

		[SetUp]
		public void Setup()
		{
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new SettingsMappingProfile()));
		}

		[Test]
		public void ShouldMapCultureInfo()
		{
			var culture = CultureInfo.CurrentCulture;
			var result = Mapper.Map<CultureInfo, CultureViewModel>(culture);
			result.text.Should().Be.EqualTo(culture.DisplayName);
			result.id.Should().Be.EqualTo(culture.LCID);
		}

		[Test]
		public void ShouldFetchAllCulturesOnComputer()
		{
			var result = Mapper.Map<IPerson, SettingsViewModel>(new Person());
			var cInfo = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
			var validCultures = new List<CultureInfo>();
			for (int i = 0; i < cInfo.Length - 1; i++)
			{
				try
				{
					CultureInfo.GetCultureInfo(cInfo[i].LCID);
				}
				catch (Exception)
				{
					continue;
				}
				validCultures.Add(cInfo[i]);
			}
			result.Cultures.Should().Have.Count.GreaterThanOrEqualTo(validCultures.Count);
			result.Cultures.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldIncludeBrowserDefault()
		{
			var result = Mapper.Map<IPerson, SettingsViewModel>(new Person());
			var defaultBrowser = result.Cultures.First();
			defaultBrowser.id.Should().Be.EqualTo(-1);
			defaultBrowser.text.Should().Be.EqualTo(Resources.BrowserDefault);
		}

		[Test]
		public void ShouldUseCorrectCulture()
		{
			var person = new Person();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo(3082));
			var result = Mapper.Map<IPerson, SettingsViewModel>(person);
			result.ChoosenCulture.id.Should().Be.EqualTo(3082);
		}

		[Test]
		public void ShouldUseCorrectCultureIfNull()
		{
			var person = new Person();
			person.PermissionInformation.SetUICulture(CultureInfo.GetCultureInfo(3082));
			var result = Mapper.Map<IPerson, SettingsViewModel>(person);
			result.ChoosenCulture.id.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldUseCorrectUiCulture()
		{
			var person = new Person();
			person.PermissionInformation.SetUICulture(CultureInfo.GetCultureInfo(3082));
			var result = Mapper.Map<IPerson, SettingsViewModel>(person);
			result.ChoosenUiCulture.id.Should().Be.EqualTo(3082);
		}

		[Test]
		public void ShouldUseCorrectUiCultureIfNull()
		{
			var person = new Person();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo(3082));
			var result = Mapper.Map<IPerson, SettingsViewModel>(person);
			result.ChoosenUiCulture.id.Should().Be.EqualTo(-1);
		}

		[Test]
		public void CulturesShouldBeSorted()
		{
			var result = Mapper.Map<IPerson, SettingsViewModel>(new Person());
			//remove browser default
			result.Cultures.RemoveAt(0);
			var sortedCultures = result.Cultures.OrderBy(c => c.text);
			result.Cultures.Should().Have.SameSequenceAs(sortedCultures);
		}




	}
}