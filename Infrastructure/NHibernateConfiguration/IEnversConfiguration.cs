using NHibernate.Cfg;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public interface IEnversConfiguration
	{
		void Configure(Configuration nhibConfiguration);
		IAuditSetter AuditSettingProvider { get; }
	}
}