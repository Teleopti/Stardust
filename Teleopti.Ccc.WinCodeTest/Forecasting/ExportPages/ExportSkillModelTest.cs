using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.ExportPages
{
	[TestFixture]
	public class ExportSkillModelTest
	{
		private ExportSkillModel _target;
		
		[SetUp]
		public void Setup()
		{
			_target = new ExportSkillModel(true,true);
		}

		[Test]
		public void ShouldGetExportToFile()
		{
			Assert.IsTrue(_target.ExportToFile);
			Assert.IsTrue(_target.DirectExportPermitted);
			Assert.IsTrue(_target.FileExportPermitted);
		}

		[Test]
		public void ShouldNotSetFileExportAsDefaultIfNoPermissionForThatOption()
		{
			_target = new ExportSkillModel(true,false);

			Assert.IsFalse(_target.ExportToFile);
			Assert.IsFalse(_target.FileExportPermitted);
			Assert.IsTrue(_target.DirectExportPermitted);
		}

		[Test]
		public void ShouldChangeExportType()
		{
			_target.ChangeExportType(false);
			Assert.IsFalse(_target.ExportToFile);
		}
	}
}
