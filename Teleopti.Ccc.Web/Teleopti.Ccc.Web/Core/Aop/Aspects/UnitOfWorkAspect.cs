using Teleopti.Ccc.Web.Core.Aop.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Core.Aop.Aspects
{
	public class UnitOfWorkAspect : IAspect
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private IUnitOfWork _unitOfWork;

		public UnitOfWorkAspect(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public void OnBeforeInvokation()
		{
			_unitOfWork = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork();
		}

		public void OnAfterInvokation()
		{
			_unitOfWork.PersistAll();
			_unitOfWork.Dispose();
		}
	}
}