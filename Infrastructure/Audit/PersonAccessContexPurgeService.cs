using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Infrastructure.Audit
{
	public class PersonAccessContexPurgeService : IPurgeAudit
	{
		private readonly IPurgeSettingRepository _purgeSettingRepository;
		private readonly INow _now;
		private readonly IPersonAccessAuditRepository _personAccessAuditRepository;

		public PersonAccessContexPurgeService(IPurgeSettingRepository purgeSettingRepository, INow now, IPersonAccessAuditRepository personAccessAuditRepository)
		{
			_purgeSettingRepository = purgeSettingRepository;
			_now = now;
			_personAccessAuditRepository = personAccessAuditRepository;
		}

		public void PurgeAudits()
		{
			var purgeSettings = _purgeSettingRepository.FindAllPurgeSettings();
			var monthsToKeepAuditEntry = purgeSettings.SingleOrDefault(p => p.Key == "MonthsToKeepAudit");
			var dateForPurging = _now.UtcDateTime().AddMonths(-(monthsToKeepAuditEntry?.Value ?? 3));
			_personAccessAuditRepository.PurgeOldAudits(dateForPurging);
		}
	}
}