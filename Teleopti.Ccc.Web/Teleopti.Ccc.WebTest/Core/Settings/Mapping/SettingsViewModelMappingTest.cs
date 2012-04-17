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

namespace Teleopti.Ccc.WebTest.Core.Settings.Mapping
{
	[TestFixture]
	public class SettingsViewModelMappingTest
	{
		[SetUp]
		public void Setup()
		{
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile<SettingsMappingProfile>());
		}

		[Test]
		public void ShouldMapCultureInfo()
		{
			var culture = CultureInfo.CurrentCulture;
			var result = Mapper.Map<CultureInfo, CultureViewModel>(culture);
			result.DisplayName.Should().Be.EqualTo(culture.DisplayName);
			result.LCID.Should().Be.EqualTo(culture.LCID);
		}

		[Test]
		public void ShouldFetchAllCulturesOnComputer()
		{
			var result = Mapper.Map<IPerson, SettingsViewModel>(new Person());
			result.Cultures.Should().Have.Count.GreaterThanOrEqualTo(CultureInfo.GetCultures(CultureTypes.SpecificCultures).Length);
			result.Cultures.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldIncludeBrowserDefault()
		{
			var result = Mapper.Map<IPerson, SettingsViewModel>(new Person());
			var defaultBrowser = result.Cultures.First();
			defaultBrowser.LCID.Should().Be.EqualTo(-1);
			defaultBrowser.DisplayName.Should().Be.EqualTo(Resources.BrowserDefault);
		}

		[Test]
		public void ShouldUseCorrectCulture()
		{
			var person = new Person();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo(3082));
			var result = Mapper.Map<IPerson, SettingsViewModel>(person);
			result.ChoosenCulture.LCID.Should().Be.EqualTo(3082);
		}

		[Test]
		public void ShouldUseCorrectCultureIfNull()
		{
			var person = new Person();
			person.PermissionInformation.SetUICulture(CultureInfo.GetCultureInfo(3082));
			var result = Mapper.Map<IPerson, SettingsViewModel>(person);
			result.ChoosenCulture.LCID.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldUseCorrectUiCulture()
		{
			var person = new Person();
			person.PermissionInformation.SetUICulture(CultureInfo.GetCultureInfo(3082));
			var result = Mapper.Map<IPerson, SettingsViewModel>(person);
			result.ChoosenUiCulture.LCID.Should().Be.EqualTo(3082);
		}

		[Test]
		public void ShouldUseCorrectUiCultureIfNull()
		{
			var person = new Person();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo(3082));
			var result = Mapper.Map<IPerson, SettingsViewModel>(person);
			result.ChoosenUiCulture.LCID.Should().Be.EqualTo(-1);
		}

		[Test]
		public void CulturesShouldBeSorted()
		{
			var result = Mapper.Map<IPerson, SettingsViewModel>(new Person());
			//remove browser default
			result.Cultures.RemoveAt(0);
			var sortedCultures = result.Cultures.OrderBy(c => c.DisplayName);
			result.Cultures.Should().Have.SameSequenceAs(sortedCultures);
		}
	}
}