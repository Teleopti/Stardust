using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.UnitOfWork
{
	public class ThisUnitOfWork : ICurrentUnitOfWork
	{
		private readonly IUnitOfWork _unitOfWork;

		public ThisUnitOfWork(IUnitOfWork unitOfWork)
		{
			InParameter.NotNull(nameof(unitOfWork), unitOfWork);
			_unitOfWork = unitOfWork;
		}

		public bool HasCurrent()
		{
			return true;
		}

		public IUnitOfWork Current()
		{
			return _unitOfWork;
		}
	}
}