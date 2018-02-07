using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public interface IUserDataSetup<T>
	{
		void Apply(T spec, IPerson person, CultureInfo cultureInfo);
	}

	public interface IUserDataSetup
	{
		void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo);
	}
}