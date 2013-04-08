using Teleopti.Ccc.Web.Core.Aop.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Core.Aop.Aspects
{
	public class UnitOfWorkAspect : IAspect
	{
		private readonly IUnitOfWorkFactoryProvider _unitOfWorkFactoryProvider;
		private IUnitOfWork _unitOfWork;

		public UnitOfWorkAspect(IUnitOfWorkFactoryProvider unitOfWorkFactoryProvider)
		{
			_unitOfWorkFactoryProvider = unitOfWorkFactoryProvider;
		}

		public void OnBeforeInvokation()
		{
			_unitOfWork = _unitOfWorkFactoryProvider.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork();
		}

		public void OnAfterInvokation()
		{
			_unitOfWork.PersistAll();
			_unitOfWork.Dispose();
		}
	}
}