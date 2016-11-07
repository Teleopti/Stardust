using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.UnitOfWork
{
	public class ThisAnalyticsUnitOfWork : ICurrentAnalyticsUnitOfWork
	{
		private readonly IUnitOfWork _unitOfWork;

		public ThisAnalyticsUnitOfWork(IUnitOfWork unitOfWork)
		{
			InParameter.NotNull("unitOfWork", unitOfWork);
			_unitOfWork = unitOfWork;
		}

		public IUnitOfWork Current()
		{
			return _unitOfWork;
		}
	}
}