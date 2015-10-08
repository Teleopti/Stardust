namespace Teleopti.Interfaces.Domain
{
	public interface ISkillTypeProvider
	{
		ISkillType Outbound();
		ISkillType InboundTelephony();
	}
}