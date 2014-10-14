using System;

namespace Teleopti.Interfaces.Domain
{

	/// <summary>
	/// 
	/// </summary>
	public interface IActualAgentState : IEquatable<IActualAgentState>
	{
		/// <summary>
		/// 
		/// </summary>
		Guid PersonId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		string State { get; set; }
		/// <summary>
		/// 
		/// </summary>
		Guid StateId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		string Scheduled { get; set; }
		/// <summary>
		/// 
		/// </summary>
		Guid ScheduledId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		DateTime StateStart { get; set; }
		/// <summary>
		/// 
		/// </summary>
		string ScheduledNext { get; set; }
		/// <summary>
		/// 
		/// </summary>
		Guid ScheduledNextId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		DateTime NextStart { get; set; }
		/// <summary>
		/// 
		/// </summary>
		string AlarmName { get; set; }
		/// <summary>
		/// 
		/// </summary>
		Guid AlarmId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		int Color { get; set; }

		/// <summary>
		/// 
		/// </summary>
		DateTime AlarmStart { get; set; }

		/// <summary>
		/// 
		/// </summary>
		double StaffingEffect { get; set; }

		/// <summary>
		/// 
		/// </summary>
		string StateCode { get; set; }

		/// <summary>
		/// 
		/// </summary>
		Guid PlatformTypeId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		DateTime ReceivedTime { get; set; }

		/// <summary>
		/// 
		/// </summary>
		TimeSpan TimeInState { get; set; }

		/// <summary>
		/// 
		/// </summary>
		DateTime? BatchId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		string OriginalDataSourceId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		Guid BusinessUnit { get; set; }

		/// <summary>
		/// 
		/// </summary>
		bool SendOverMessageBroker { get; set; }
	}
}