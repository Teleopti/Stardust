using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class UnitOfWorkAspect : IUnitOfWorkAspect
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IBusinessUnitFilterOverrider _overrider;
		private readonly IBusinessUnitIdForRequest _businessUnitIdForRequest;
		private IUnitOfWork _unitOfWork;
		private IDisposable _businessUnitOverrideScope;

		public UnitOfWorkAspect(
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, 
			IBusinessUnitFilterOverrider overrider, 
			IBusinessUnitIdForRequest businessUnitIdForRequest)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_overrider = overrider;
			_businessUnitIdForRequest = businessUnitIdForRequest;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			_unitOfWork = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork();
			try
			{
				var id = _businessUnitIdForRequest.Get();
				if (id.HasValue)
					_businessUnitOverrideScope = _overrider.OverrideWith(id.Value);
			}
			catch (Exception)
			{
				_unitOfWork.Dispose();
				throw;
			}
		}

		public virtual void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			try
			{
				if (exception != null) return;
				_unitOfWork.PersistAll();
			}
			finally
			{
				try
				{
					_businessUnitOverrideScope?.Dispose();
					_businessUnitOverrideScope = null;
				}
				finally
				{
					_unitOfWork.Dispose();
				}
			}
		}
		
	}

}