using Teleopti.Ccc.Web.Core.Aop.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Core.Aop.Aspects
{
	public class UnitOfWorkAspect : IAspect
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private IUnitOfWork _unitOfWork;

		public UnitOfWorkAspect(IUnitOfWorkFactory unitOfWorkFactory) {
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public void OnBeforeInvokation()
		{
			_unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork();
		}

		public void OnAfterInvokation()
		{
			_unitOfWork.PersistAll();
			_unitOfWork.Dispose();
		}
	}
}