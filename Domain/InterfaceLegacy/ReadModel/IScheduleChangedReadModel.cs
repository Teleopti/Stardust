﻿using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.ReadModel
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
		DateTime Date { get; set; } 
	}

	/// <summary>
	/// 
	/// </summary>
	public interface ILastChangedReadModel
	{
		/// <summary>
		/// 
		/// </summary>
		DateTime ThisTime { get; set; }
		/// <summary>
		/// 
		/// </summary>
		DateTime LastTime { get; set; } 
	}
}