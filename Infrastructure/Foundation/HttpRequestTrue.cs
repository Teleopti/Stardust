using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class HttpRequestTrue : IBusinessUnitForRequest
	{
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public HttpRequestTrue(ICurrentHttpContext currentHttpContext, IBusinessUnitRepository businessUnitRepository, ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentHttpContext = currentHttpContext;
			_businessUnitRepository = businessUnitRepository;
			_currentUnitOfWork = currentUnitOfWork;
		}

		public bool IsHttpRequest()
		{
			return true;
		}

		public IBusinessUnit BusinessUnitForRequest()
		{
			var buid = UnitOfWorkAspect.BusinessUnitIdForRequest(_currentHttpContext);
			if (buid.HasValue)
			{
				return _currentUnitOfWork.Current()!=null ? 
					_businessUnitRepository.Load(buid.Value) : 
					null;
			}
			return null;
		}
	}
}