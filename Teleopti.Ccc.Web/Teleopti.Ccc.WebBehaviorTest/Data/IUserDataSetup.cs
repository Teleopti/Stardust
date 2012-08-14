using System.Globalization;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public interface IUserDataSetup
	{
		void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo);
	}
}