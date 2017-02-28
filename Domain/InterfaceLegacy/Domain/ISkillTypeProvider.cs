namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISkillTypeProvider
	{
		ISkillType Outbound();
		ISkillType InboundTelephony();
	}
}