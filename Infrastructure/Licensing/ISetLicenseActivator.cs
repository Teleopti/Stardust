using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public interface ISetLicenseActivator : ILicenseFeedback
	{
		void Execute(IDataSource dataSource);
	}
}