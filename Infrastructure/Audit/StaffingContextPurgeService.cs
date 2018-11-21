using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Infrastructure.Audit
{
	public class StaffingContextPurgeService : IPurgeAudit
	{
		private readonly IStaffingAuditRepository _staffingAuditRepository;
		private readonly IPurgeSettingRepository _purgeSettingRepository;
		private readonly INow _now;

		public StaffingContextPurgeService(IStaffingAuditRepository staffingAuditRepository, IPurgeSettingRepository purgeSettingRepository, INow now)
		{
			_staffingAuditRepository = staffingAuditRepository;
			_purgeSettingRepository = purgeSettingRepository;
			_now = now;
		}

		public void PurgeAudits()
		{
			var purgeSettings = _purgeSettingRepository.FindAllPurgeSettings();
			var monthsToKeepAuditEntry = purgeSettings.SingleOrDefault(p => p.Key == "MonthsToKeepAudit");
			var dateForPurging = _now.UtcDateTime().AddMonths(-(monthsToKeepAuditEntry?.Value ?? 3));
			_staffingAuditRepository.PurgeOldAudits(dateForPurging);
		}
	}
}