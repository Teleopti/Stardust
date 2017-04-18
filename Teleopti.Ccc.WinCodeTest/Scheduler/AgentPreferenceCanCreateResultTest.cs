using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;

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

			_result.StartTimeMinErrorActivity = true;
			_result.StartTimeMaxErrorActivity = true;
			_result.EndTimeMinErrorActivity = true;
			_result.EndTimeMaxErrorActivity = true;
			_result.LengthMinErrorActivity = true;
			_result.LengthMaxErrorActivity = true;

			_result.EmptyError = true;

			_result.ConflictingTypeError = true;
			_result.Result = true;

			Assert.IsTrue(_result.StartTimeMinError);
			Assert.IsTrue(_result.StartTimeMaxError);
			Assert.IsTrue(_result.EndTimeMinError);
			Assert.IsTrue(_result.EndTimeMaxError);
			Assert.IsTrue(_result.LengthMinError);
			Assert.IsTrue(_result.LengthMaxError);

			Assert.IsTrue(_result.StartTimeMinErrorActivity);
			Assert.IsTrue(_result.StartTimeMaxErrorActivity);
			Assert.IsTrue(_result.EndTimeMinErrorActivity);
			Assert.IsTrue(_result.EndTimeMaxErrorActivity);
			Assert.IsTrue(_result.LengthMinErrorActivity);
			Assert.IsTrue(_result.LengthMaxErrorActivity);

			Assert.IsTrue(_result.EmptyError);

			Assert.IsTrue(_result.ConflictingTypeError);
			Assert.IsTrue(_result.Result);
		}

		[Test]
		public void ShouldReturnActivityTimesErrorIfAny()
		{
			_result.StartTimeMinErrorActivity = false;
			_result.StartTimeMaxErrorActivity = false;
			_result.EndTimeMinErrorActivity = true;
			_result.EndTimeMaxErrorActivity = false;
			_result.LengthMinErrorActivity = false;
			_result.LengthMaxErrorActivity = true;

			Assert.IsTrue(_result.ActivityTimesError);	
		}

		[Test]
		public void ShouldReturnExtendedTimesErrorIfAny()
		{
			_result.StartTimeMinError = false;
			_result.StartTimeMaxError = false;
			_result.EndTimeMinError = true;
			_result.EndTimeMaxError = false;
			_result.LengthMinError = false;
			_result.LengthMaxError = true;

			Assert.IsTrue(_result.ExtendedTimesError);
		}
	}
}
