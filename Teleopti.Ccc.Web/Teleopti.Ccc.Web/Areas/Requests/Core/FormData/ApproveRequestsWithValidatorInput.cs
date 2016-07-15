using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.FormData
{
	public class ApproveRequestsWithValidatorInput
	{
		public IEnumerable<Guid> RequestIds { get; set; }
		public RequestValidatorsFlag Validators { get; set; }
	}
}