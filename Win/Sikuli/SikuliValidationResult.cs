using System;
using System.Text;

namespace Teleopti.Ccc.Win.Sikuli
{
	public class SikuliValidationResult
	{
		public enum ResultValue
		{
			Pass,
			Fail,
			Warn
		}


		private readonly StringBuilder _stringBuilder;

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

		public void AppendLimitValueLineToDetails(string name, string limit, string value)
		{
			var line = String.Format("{0} : Limit = {1}; Value = {2}", name, limit, value);
			Details.AppendLine(line);
		}
	}
}