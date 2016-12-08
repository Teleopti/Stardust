using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class BusinessUnitIdIdForRequest : IBusinessUnitIdForRequest
	{
		private readonly ICurrentHttpContext _context;

		public BusinessUnitIdIdForRequest(ICurrentHttpContext context)
		{
			_context = context;
		}

		public Guid? Get()
		{
			if (_context.Current() == null) return null;
			var buId = string.Empty;
			var queryString = _context.Current().Request.QueryString;
			if (queryString != null)
				buId = queryString["BusinessUnitId"];
			var headers = _context.Current().Request.Headers;
			if (headers != null)
				buId = headers["X-Business-Unit-Filter"] ?? buId;
			if (string.IsNullOrEmpty(buId)) return null;
			return Guid.Parse(buId);
		}

	}
}