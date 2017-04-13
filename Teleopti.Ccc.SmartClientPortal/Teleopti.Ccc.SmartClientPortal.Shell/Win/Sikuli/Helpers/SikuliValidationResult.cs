using System;
using System.Text;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers
{
	public class SikuliValidationResult
	{
		public enum ResultValue
		{
			Pass = 0,
			Warn = 1,
			Fail = 2
		}

		public ResultValue CombineResultValue(SikuliValidationResult other)
		{
			return getHigherEnum(this.Result, other.Result);
		}

		private static ResultValue getHigherEnum(ResultValue first, ResultValue second)
		{
			return first.CompareTo(second) >= 0 ? first : second;
		}

		public StringBuilder CombineDetails(SikuliValidationResult other)
		{
			var otherDetails = other.Details.ToString();
			if (!string.IsNullOrWhiteSpace(otherDetails))
				Details.Append(otherDetails);
			return Details;
		}

		private readonly StringBuilder _stringBuilder;

		public SikuliValidationResult() : this(ResultValue.Pass){}

		public SikuliValidationResult(ResultValue defaultResult)
		{
			Result = defaultResult;
			_stringBuilder = new StringBuilder();
		}

		public ResultValue Result { get; set; }

		public StringBuilder Details
		{
			get { return _stringBuilder; }
		}

		public void AppendResultLine(string name, string limit, string value, ResultValue resultValue)
		{
			var line = String.Format("{0} : Limit = {1}; Value = {2}; {3}", name, limit, value, resultValue);
			Details.AppendLine(line);
		}
	}
}