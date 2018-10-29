using System;

namespace Teleopti.Wfm.Api.Query.Request
{
	public class PermissionByPersonDto : IQueryDto
	{
		public Guid PersonId { get; set; }
	}
}