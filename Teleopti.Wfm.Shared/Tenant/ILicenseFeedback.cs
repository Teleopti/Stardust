namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public interface ILicenseFeedback
	{
		void Warning(string warning);
		void Warning(string warning, string caption);
		void Error(string error);
	}
}