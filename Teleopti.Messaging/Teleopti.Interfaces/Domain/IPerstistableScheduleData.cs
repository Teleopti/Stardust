using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Interfaces.Domain
{
	public interface INonversionedPersistableScheduleData : IScheduleData, IAggregateRoot, IChangeInfo, IMainReference
	{
		/// <summary>
		/// Gets the function path.
		/// </summary>
		/// <value>The function path.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2009-03-10
		/// </remarks>
		string FunctionPath { get; }

		/// <summary>
		/// Makes this aggregate transient.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2009-11-30
		/// </remarks>
		INonversionedPersistableScheduleData CreateTransient();
	}

	/// <summary>
	/// Data in a schedule that also exists in db and is persistable
	/// </summary>
	/// <remarks>
	/// Created by: rogerkr
	/// Created date: 2008-05-29
	/// </remarks>
	public interface IPersistableScheduleData : IVersionedAggregateRoot, INonversionedPersistableScheduleData
	{
	}
}