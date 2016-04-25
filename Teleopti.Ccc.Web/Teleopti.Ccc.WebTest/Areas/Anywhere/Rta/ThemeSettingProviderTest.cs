using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Rta
{
	public class ThemeSettingProviderTest
	{
		[Test]
		public void ShouldPersist()
		{
			var themeSetting = new ThemeSetting
			{
				Name = "dark"
			};
			var repository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var target = new ThemeSettingProvider(repository);
			repository.Stub(x => x.FindValueByKey(ThemeSettingProvider.Key, new ThemeSetting())).IgnoreArguments().Return(themeSetting);

			var result = target.Persist(themeSetting);

			result.Name.Should().Be(themeSetting.Name);
		}

		[Test]
		public void ShouldGet()
		{
			var themeSetting = new ThemeSetting
			{
				Name = "dark"
			};
			var repository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var target = new ThemeSettingProvider(repository);
			repository.Stub(x => x.FindValueByKey(ThemeSettingProvider.Key, new ThemeSetting())).IgnoreArguments().Return(themeSetting);

			target.Persist(themeSetting);
			var result = target.Get();

			result.Name.Should().Be(themeSetting.Name);
		}

		[Test]
		public void ShouldGetByOwner()
		{
			var themeSetting = new ThemeSetting
			{
				Name = "dark"
			};
			var person = new Person();
			var repository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var target = new ThemeSettingProvider(repository);
			repository.Stub(x => x.FindValueByKey(ThemeSettingProvider.Key, new ThemeSetting())).IgnoreArguments().Return(themeSetting);
			repository.Stub(x => x.FindValueByKeyAndOwnerPerson(ThemeSettingProvider.Key, person, new ThemeSetting())).IgnoreArguments().Return(themeSetting);

			target.Persist(themeSetting);
			var result = target.GetByOwner(person);

			result.Name.Should().Be(themeSetting.Name);
		}
	}
}
