using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public static class RtaExtensions
	{
		public static void CheckForActivityChange(this IRta rta, Guid personId, Guid businessUnitId)
		{
			rta.CheckForActivityChange(new CheckForActivityChangeInputModel
			{
				PersonId = personId,
				BusinessUnitId = businessUnitId
			});
		}
	}
}