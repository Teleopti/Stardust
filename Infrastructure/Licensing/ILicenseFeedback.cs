namespace Teleopti.Ccc.Infrastructure.Licensing
{
    public interface ILicenseFeedback
    {
        void Warning(string warning);
        void Error(string error);
    }
}