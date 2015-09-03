using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class HttpRequestTrue : IIsHttpRequest
	{
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly IBusinessUnitRepository _businessUnitRepository;

		public HttpRequestTrue(ICurrentHttpContext currentHttpContext, IBusinessUnitRepository businessUnitRepository)
		{
			_currentHttpContext = currentHttpContext;
			_businessUnitRepository = businessUnitRepository;
		}

		public bool IsHttpRequest()
		{
			return true;
		}

		public IBusinessUnit BusinessUnitForRequest()
		{
			var buid = UnitOfWorkAspect.BusinessUnitIdForRequest(_currentHttpContext);
			return buid.HasValue ? _businessUnitRepository.Load(buid.Value) : null;
		}
	}
}