namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IEventInfrastructureInfoPopulator
	{
		void PopulateEventContext(params object[] events);
	}
}