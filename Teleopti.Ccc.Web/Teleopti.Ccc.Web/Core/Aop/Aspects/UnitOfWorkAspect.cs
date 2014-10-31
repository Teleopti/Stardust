using System;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Web;
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
			_businessUnitOverrideScope = overrideBusinessUnitFilter();
		}

		public void OnAfterInvokation(Exception exception)
		{
			_unitOfWork.PersistAll();
			diposeBusinessUnitFilterOverride();
			_unitOfWork.Dispose();
		}



		private IDisposable overrideBusinessUnitFilter()
		{
			if (_context.Current() == null) return null;
			var buId = string.Empty;
			var queryString = _context.Current().Request.QueryString;
			if (queryString != null)
				buId = queryString["BusinessUnitId"];
			var headers = _context.Current().Request.Headers;
			if (headers != null)
			{
				buId = headers["X-Business-Unit-Filter"] ?? buId;
			}

			if (string.IsNullOrEmpty(buId)) return null;
			var id = Guid.Parse(buId);
			return _overrider.OverrideWith(id);
		}

		private void diposeBusinessUnitFilterOverride()
		{
			if (_businessUnitOverrideScope == null) return;
			_businessUnitOverrideScope.Dispose();
			_businessUnitOverrideScope = null;
		}
	}
}