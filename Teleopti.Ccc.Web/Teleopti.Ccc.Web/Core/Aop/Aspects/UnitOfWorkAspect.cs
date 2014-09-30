using System;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Core.Aop.Core;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Core.Aop.Aspects
{
	public class UnitOfWorkAspect : IAspect
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IBusinessUnitFilterOverrider _overrider;
		private readonly ICurrentHttpContext _context;
		private IUnitOfWork _unitOfWork;
		private IDisposable _businessUnitOverrideScope;

		public UnitOfWorkAspect(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IBusinessUnitFilterOverrider overrider, ICurrentHttpContext context)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_overrider = overrider;
			_context = context;
		}

		public void OnBeforeInvokation()
		{
			_unitOfWork = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork();
			var buId = string.Empty;
			var queryString = _context.Current().Request.QueryString;
			if (queryString != null)
				buId = queryString["BusinessUnitId"];
			var headers = _context.Current().Request.Headers;
			if (headers != null)
			{
				buId = headers["X-Business-Unit-Filter"] ?? buId;
			}

			if (string.IsNullOrEmpty(buId)) return;
			var id = Guid.Parse(buId);
			_businessUnitOverrideScope = _overrider.OverrideWith(id);
		}

		public void OnAfterInvokation()
		{
			_unitOfWork.PersistAll();

			if (_businessUnitOverrideScope != null)
			{
				_businessUnitOverrideScope.Dispose();
				_businessUnitOverrideScope = null;
			}

			_unitOfWork.Dispose();
		}
	}
}