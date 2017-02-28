using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.UnitOfWork
{
	public class ThisAnalyticsUnitOfWork : ICurrentAnalyticsUnitOfWork
	{
		private readonly IUnitOfWork _unitOfWork;

		public ThisAnalyticsUnitOfWork(IUnitOfWork unitOfWork)
		{
			InParameter.NotNull(nameof(unitOfWork), unitOfWork);
			_unitOfWork = unitOfWork;
		}

		public IUnitOfWork Current()
		{
			return _unitOfWork;
		}
	}
}