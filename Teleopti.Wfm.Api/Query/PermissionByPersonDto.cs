using System;

namespace Teleopti.Wfm.Api.Query
{
	public class PermissionByPersonDto : IQueryDto
	{
		public Guid PersonId { get; set; }
	}
}