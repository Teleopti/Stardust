using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Intraday;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
	[TestFixture]
	public class OpenIntradayTodayCommandTest
	{
		private MockRepository _mock;
		private OpenIntradayTodayCommand _target;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			var personSelector = _mock.DynamicMock<IPersonSelectorView>();
			_target = new OpenIntradayTodayCommand(personSelector);
		}

		[Test]
		public void VerifyConstructorAndCanExecute()
		{
			Assert.That(_target, Is.Not.Null);
			Assert.IsFalse(_target.CanExecute());
		}
	}
}
