using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

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