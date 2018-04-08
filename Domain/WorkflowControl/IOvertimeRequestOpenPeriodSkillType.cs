using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public interface IOvertimeRequestOpenPeriodSkillType : IAggregateEntity
	{
		ISkillType SkillType { get; set; }

		IOvertimeRequestOpenPeriodSkillType EntityClone();

		IOvertimeRequestOpenPeriodSkillType NoneEntityClone();
	}
}