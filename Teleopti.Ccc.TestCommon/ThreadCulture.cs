using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.TestCommon
{
	public class ThreadCulture : IUserCulture
	{
		public CultureInfo GetCulture()
		{
			return CultureInfo.CurrentCulture;
		}
	}
}