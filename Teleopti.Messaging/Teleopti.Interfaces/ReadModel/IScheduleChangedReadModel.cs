using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.ReadModel
{
	/// <summary>
	/// 
	/// </summary>
	public interface IScheduleChangedReadModel
	{
		/// <summary>
		/// 
		/// </summary>
		Guid Person { get; set; }
		/// <summary>
		/// 
		/// </summary>
		DateOnly Date { get; set; } 
	}
}