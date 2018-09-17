using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Toggle
{
	public class FetchToggleOverrideFromDbTest
	{
		[Test]
		public void ShouldGetNullIfNotPresentInDb()
		{
			SetupFixtureForAssembly.RestoreCcc7Database();
			var configReader = new FakeConfigReader();
			configReader.FakeConnectionString("Toggle", InfraTestConfigReader.ConnectionString);
		
			var target = new FetchToggleOverrideFromDb(configReader);

			target.OverridenValue(Toggles.TestToggle).HasValue
				.Should().Be.False();
		}
		
		
		[TestCase(true)]
		[TestCase(false)]
		public void ShouldGetValueIfPresentInDb(bool dbValue)
		{
			SetupFixtureForAssembly.RestoreCcc7Database();
			var configReader = new FakeConfigReader();
			configReader.FakeConnectionString("Toggle", InfraTestConfigReader.ConnectionString);

			var saver = new SaveToggleOverride(configReader);
			saver.Save(Toggles.TestToggle, dbValue);
			
			var target = new FetchToggleOverrideFromDb(configReader);

			target.OverridenValue(Toggles.TestToggle).Value
				.Should().Be.EqualTo(dbValue);
		}
	}
}