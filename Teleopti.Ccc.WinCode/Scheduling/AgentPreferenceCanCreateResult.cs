

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IAgentPreferenceCanCreateResult
	{
		bool StartTimeMinError { get; set; }
		bool StartTimeMaxError { get; set; }
		bool EndTimeMinError { get; set; }
		bool EndTimeMaxError { get; set; }
		bool LengthMinError { get; set; }
		bool LengthMaxError { get; set; }
		bool Result { get; set; }
	}
	public class AgentPreferenceCanCreateResult : IAgentPreferenceCanCreateResult
	{
		private bool _startTimeMinError;
		private bool _startTimeMaxError;
		private bool _endTimeMinError;
		private bool _endTimeMaxError;
		private bool _lengthMinError;
		private bool _lengthMaxError;
		private bool _result;


		public bool StartTimeMinError
		{
			get { return _startTimeMinError; }
			set { _startTimeMinError = value; }
		}

		public bool StartTimeMaxError
		{
			get { return _startTimeMaxError; }
			set { _startTimeMaxError = value; }
		}

		public bool EndTimeMinError
		{
			get { return _endTimeMinError; }
			set { _endTimeMinError = value; }
		}

		public bool EndTimeMaxError
		{
			get { return _endTimeMaxError; }
			set { _endTimeMaxError = value; }
		}

		public bool LengthMinError
		{
			get { return _lengthMinError; }
			set { _lengthMinError = value; }
		}

		public bool LengthMaxError
		{
			get { return _lengthMaxError; }
			set { _lengthMaxError = value; }
		}

		public bool Result
		{
			get { return _result; }
			set { _result = value; }
		}
	}
}
