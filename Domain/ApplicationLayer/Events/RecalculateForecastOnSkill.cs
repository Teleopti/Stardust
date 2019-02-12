using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	///<summary>
	/// Message with details to perform an recalc of forecasting data on a skill.
	///</summary>
	public class RecalculateForecastOnSkill 
	{
				
		///<summary>
		/// The skill to recalculate.
		///</summary>
		public Guid SkillId { get; set; }

        /// <summary>
        /// The workloads to recalculate
        /// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public IEnumerable<Guid> WorkloadIds { get; set; }

	}
}