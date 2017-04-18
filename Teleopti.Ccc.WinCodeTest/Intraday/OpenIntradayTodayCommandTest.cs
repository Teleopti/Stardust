using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
	public class OpenIntradayTodayCommandTest
	{
		private OpenIntradayTodayCommand _target;
		private MockRepository _mocks;
		private IPersonSelectorView _personSelectorView;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_personSelectorView = _mocks.StrictMock<IPersonSelectorView>();
			_target = new OpenIntradayTodayCommand(_personSelectorView);
		}


		[Test]
		public void VerifyCanExecuteReturnsFalse()
		{
			using (_mocks.Record())
			{
				Expect.Call(_personSelectorView.SelectedNodes)
				      .Return(new List<TreeNodeAdv>());
			}
			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.CanExecute());
			}
		}

		[Test]
		public void VerifyCanExecuteReturnsTrue()
		{
			using (_mocks.Record())
			{
				Expect.Call(_personSelectorView.SelectedNodes)
					  .Return(new List<TreeNodeAdv>{ new TreeNodeAdv() });
			}
			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.CanExecute());
			}
		}
	}
}
