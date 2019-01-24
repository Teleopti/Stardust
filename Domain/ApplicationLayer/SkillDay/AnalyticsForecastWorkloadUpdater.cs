using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SkillDay
{
	[DisabledBy(Toggles.WFM_AnalyticsForecastUpdater_80798)]
	public class AnalyticsForecastWorkloadUpdater :
		IHandleEvent<SkillDayChangedEvent>,
		IRunOnHangfire
	{
		private readonly ISkillDayChangedEventHandler _skillDayChangedEventHandler;

		public AnalyticsForecastWorkloadUpdater(ISkillDayChangedEventHandler skillDayChangedEventHandler)
		{
			_skillDayChangedEventHandler = skillDayChangedEventHandler;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[Attempts(5)]
		public virtual void Handle(SkillDayChangedEvent @event)
		{
			_skillDayChangedEventHandler.Handle(@event);
		}
	}
}