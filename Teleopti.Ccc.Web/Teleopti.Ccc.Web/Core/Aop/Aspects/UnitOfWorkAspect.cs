using System;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Web;
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

		public void OnBeforeInvocation()
		{
			_unitOfWork = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork();
			_businessUnitOverrideScope = overrideBusinessUnitFilter();
		}

		public void OnAfterInvocation(Exception exception)
		{
			persistWhenNoExpcetion(exception);
			diposeBusinessUnitFilterOverride();
			_unitOfWork.Dispose();
		}

		private void persistWhenNoExpcetion(Exception exception)
		{
			if (exception != null) return;

			_unitOfWork.PersistAll();
		}


		private IDisposable overrideBusinessUnitFilter()
		{
			var id = BusinessUnitIdForRequest(_context);
			return id.HasValue ? _overrider.OverrideWith(id.Value) : null;
		}

		public static Guid? BusinessUnitIdForRequest(ICurrentHttpContext context)
		{
			if (context.Current() == null) return null;
			var buId = string.Empty;
			var queryString = context.Current().Request.QueryString;
			if (queryString != null)
				buId = queryString["BusinessUnitId"];
			var headers = context.Current().Request.Headers;
			if (headers != null)
				buId = headers["X-Business-Unit-Filter"] ?? buId;
			if (string.IsNullOrEmpty(buId)) return null;
			return Guid.Parse(buId);
		}

		private void diposeBusinessUnitFilterOverride()
		{
			if (_businessUnitOverrideScope == null) return;
			_businessUnitOverrideScope.Dispose();
			_businessUnitOverrideScope = null;
		}
	}

}