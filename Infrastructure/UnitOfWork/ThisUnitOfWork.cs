using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class ThisUnitOfWork : ICurrentUnitOfWork
	{
		private readonly IUnitOfWork _unitOfWork;

		public ThisUnitOfWork(IUnitOfWork unitOfWork)
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