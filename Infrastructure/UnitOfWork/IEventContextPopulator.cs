namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IEventContextPopulator
	{
		void PopulateEventContext(object @event);
	}
}