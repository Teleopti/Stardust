using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Helper
{
	public class NoFormatting : ITextFormatter
	{
		/// <summary>
		/// returns the string unformatted
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public string Format(string value)
		{
			return value;
		}
	}
}