namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
	public interface IEventsRoot
	{
		void CloneEventsAfterMerge(AggregateRoot clone);
	}
}