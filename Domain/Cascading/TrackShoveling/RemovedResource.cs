using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading.TrackShoveling
{
	public class RemovedResource
	{
		public RemovedResource()
		{
			ToSubskills = new List<ISkill>();
		}

		public double ResourcesMoved { get; set; }
		public ICollection<ISkill> ToSubskills { get; }
	}
}