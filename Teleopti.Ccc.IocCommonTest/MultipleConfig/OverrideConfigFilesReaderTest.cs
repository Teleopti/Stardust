using System.IO;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon.MultipleConfig;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.IocCommonTest.MultipleConfig
{
	public class OverrideConfigFilesReaderTest
	{
		[Test]
		public void ShouldFindOneFile()
		{
			var dir = Directory.GetCurrentDirectory();
			var displayName = RandomName.Make();
			var filename = Path.Combine(dir, displayName + ".override.config");
			try
			{
				File.WriteAllText(filename, "it doesn't matter");

				var target = new OverrideConfigFilesReader(dir);
				target.Overrides()[displayName]
					.Should().Be.EqualTo(filename);
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void ShouldNotFindFileWithWrongPatter()
		{
			var dir = Directory.GetCurrentDirectory();
			var displayName = RandomName.Make();
			var filename = Path.Combine(dir, displayName + ".config");
			try
			{
				File.WriteAllText(filename, "it doesn't matter");

				var target = new OverrideConfigFilesReader(dir);
				target.Overrides().ContainsKey(displayName)
					.Should().Be.False();
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void ShouldReadMultipleFiles()
		{
			var dir = Directory.GetCurrentDirectory();
			var filename1 = Path.Combine(dir, RandomName.Make() + ".override.config");
			var filename2 = Path.Combine(dir, RandomName.Make() + ".override.config");
			try
			{
				File.WriteAllText(filename1, "it doesn't matter");
				File.WriteAllText(filename2, "it doesn't matter");
				var target = new OverrideConfigFilesReader(dir);
				target.Overrides().Count()
					.Should().Be.EqualTo(2);
			}
			finally
			{
				File.Delete(filename1);
				File.Delete(filename2);
			}
		}
	}
}