using Teleopti.Ccc.Infrastructure.MultiTenancy;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public interface IPersonInfoPersister
	{
		void Persist(PersonInfo personInfo);
	}
}