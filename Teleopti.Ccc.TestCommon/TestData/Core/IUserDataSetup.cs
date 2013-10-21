using System.Globalization;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public interface IUserDataSetup
	{
		void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo);
	}
}