using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public interface IUserSetup<T>
	{
		void Apply(T spec, IPerson user, CultureInfo cultureInfo);
	}
	
	public interface IUserSetup
	{
		void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo);
	}
}