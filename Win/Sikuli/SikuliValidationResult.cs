using System.Text;

namespace Teleopti.Ccc.Win.Sikuli
{
	public class SikuliValidationResult
	{
		private readonly StringBuilder _stringBuilder;

		public SikuliValidationResult(bool defaultResult)
		{
			Result = defaultResult;
			_stringBuilder = new StringBuilder();
		}

		public bool Result { get; set; }

		public StringBuilder Details
		{
			get { return _stringBuilder; }
		}

		public void AppendLimitValueLine(string name, string limit, string value)
		{
			var line = string.Format("{0} : Limit = {1}; Value = {2}", name, limit, value);
			Details.AppendLine(line);
		}
	}
}