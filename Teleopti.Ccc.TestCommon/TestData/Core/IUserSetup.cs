using System.Globalization;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public interface IUserSetup
	{
		void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo);
	}
}