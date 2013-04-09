using NUnit.Framework;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class AgentPreferenceCanCreateResultTest
	{
		private AgentPreferenceCanCreateResult _result;

		[SetUp]
		public void Setup()
		{
			_result = new AgentPreferenceCanCreateResult();		
		}

		[Test]
		public void ShouldGetSetProperties()
		{
			_result.StartTimeMinError = true;
			_result.StartTimeMaxError = true;
			_result.EndTimeMinError = true;
			_result.EndTimeMaxError = true;
			_result.LengthMinError = true;
			_result.LengthMaxError = true;
			_result.ConflictingTypeError = true;
			_result.Result = true;

			Assert.IsTrue(_result.StartTimeMinError);
			Assert.IsTrue(_result.StartTimeMaxError);
			Assert.IsTrue(_result.EndTimeMinError);
			Assert.IsTrue(_result.EndTimeMaxError);
			Assert.IsTrue(_result.LengthMinError);
			Assert.IsTrue(_result.LengthMaxError);
			Assert.IsTrue(_result.ConflictingTypeError);
			Assert.IsTrue(_result.Result);
		}
	}
}
