namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
	public interface ICloneEventsAfterMerge
	{
		void CloneEventsAfterMerge(AggregateRoot_Events clone);
	}
}