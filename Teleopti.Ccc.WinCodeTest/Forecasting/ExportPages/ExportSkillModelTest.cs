using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Forecasting.ExportPages;


namespace Teleopti.Ccc.WinCodeTest.Forecasting.ExportPages
{
	[TestFixture]
	public class ExportSkillModelTest
	{
		private ExportSkillModel _target;
		//private MockRepository _mocks;

		[SetUp]
		public void Setup()
		{
			_target = new ExportSkillModel();
		   //	_mocks = new MockRepository();
		}

        //[Test]
        //public void ShouldDisposeObject()
        //{
        //    using (_mocks.Record())
        //    {
			
        //    }
        //    using (_mocks.Playback())
        //    {
        //        _target.Dispose();
        //    }
        //}

		[Test]
		public void ShouldGetExportToFile()
		{
			Assert.IsTrue(_target.ExportToFile);
		}

		[Test]
		public void ShouldChangeExportType()
		{
			_target.ChangeExportType(false);
			Assert.IsFalse(_target.ExportToFile);
		}
	}
}
