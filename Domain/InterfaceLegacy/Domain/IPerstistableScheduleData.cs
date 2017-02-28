using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Interfaces.Domain
{
	public interface IPersistableScheduleData : IScheduleData, IAggregateRoot, IChangeInfo, IMainReference
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
		IPersistableScheduleData CreateTransient();
	}
}