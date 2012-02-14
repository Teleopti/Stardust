using NHibernate.Cfg;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public interface IEnversConfiguration
	{
		void Configure(Configuration nhibConfiguration);
		IAuditSetter AuditSettingProvider { get; }
	}
}