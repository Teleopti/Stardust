

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

		bool StartTimeMinErrorActivity { get; set; }
		bool StartTimeMaxErrorActivity { get; set; }
		bool EndTimeMinErrorActivity { get; set; }
		bool EndTimeMaxErrorActivity { get; set; }
		bool LengthMinErrorActivity { get; set; }
		bool LengthMaxErrorActivity { get; set; }

		bool EmptyError { get; set; }
	
		bool ConflictingTypeError { get; set; }
		bool Result { get; set; }
		bool ActivityTimesError { get; }
		bool ExtendedTimesError { get; }
	}
	public class AgentPreferenceCanCreateResult : IAgentPreferenceCanCreateResult
	{
		private bool _startTimeMinError;
		private bool _startTimeMaxError;
		private bool _endTimeMinError;
		private bool _endTimeMaxError;
		private bool _lengthMinError;
		private bool _lengthMaxError;

		private bool _startTimeMinErrorActivity;
		private bool _startTimeMaxErrorActivity;
		private bool _endTimeMinErrorActivity;
		private bool _endTimeMaxErrorActivity;
		private bool _lengthMinErrorActivity;
		private bool _lengthMaxErrorActivity;

		private bool _result;
		private bool _conflictingTypeError;
		private bool _emptyError;


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

		public bool StartTimeMinErrorActivity
		{
			get { return _startTimeMinErrorActivity; }
			set { _startTimeMinErrorActivity = value; }
		}

		public bool StartTimeMaxErrorActivity
		{
			get { return _startTimeMaxErrorActivity; }
			set { _startTimeMaxErrorActivity = value; }
		}

		public bool EndTimeMinErrorActivity
		{
			get { return _endTimeMinErrorActivity; }
			set { _endTimeMinErrorActivity = value; }
		}

		public bool EndTimeMaxErrorActivity
		{
			get { return _endTimeMaxErrorActivity; }
			set { _endTimeMaxErrorActivity = value; }
		}

		public bool LengthMinErrorActivity
		{
			get { return _lengthMinErrorActivity; }
			set { _lengthMinErrorActivity = value; }
		}

		public bool LengthMaxErrorActivity
		{
			get { return _lengthMaxErrorActivity; }
			set { _lengthMaxErrorActivity = value; }
		}

		public bool EmptyError
		{
			get { return _emptyError; }
			set { _emptyError = value; }
		}

		public bool ConflictingTypeError
		{
			get { return _conflictingTypeError; }
			set { _conflictingTypeError = value; }
		}

		public bool Result
		{
			get { return _result; }
			set { _result = value; }
		}

		public bool ActivityTimesError
		{
			get
			{
				return _startTimeMinErrorActivity || _startTimeMaxErrorActivity || _endTimeMinErrorActivity || _endTimeMaxErrorActivity || _lengthMinErrorActivity || _lengthMaxErrorActivity;		
			}
		}
		public bool ExtendedTimesError
		{
			get
			{
				return _startTimeMinError || _startTimeMaxError || _endTimeMinError || _endTimeMaxError || _lengthMinError || _lengthMaxError;			
			}
		}
	}
}
