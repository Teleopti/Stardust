using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.FormData
{
	public class ApproveRequestsWithValidatorInput
	{
		public IEnumerable<Guid> RequestIds { get; set; }
		public RequestValidatorsFlag Validators { get; set; }
	}
}