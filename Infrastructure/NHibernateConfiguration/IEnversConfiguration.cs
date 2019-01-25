using NHibernate.Cfg;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public interface IEnversConfiguration
	{
		void Configure(Configuration nhibConfiguration, IUpdatedBy updatedBy);
		IAuditSetter AuditSettingProvider { get; }
	}
}