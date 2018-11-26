using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

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