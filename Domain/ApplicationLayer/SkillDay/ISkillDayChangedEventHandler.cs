using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SkillDay
{
	public interface ISkillDayChangedEventHandler
	{
		void Handle(SkillDayChangedEvent @event);
	}
}