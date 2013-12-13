using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class CurrentInitiatorIdentifier : ICurrentInitiatorIdentifier
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public CurrentInitiatorIdentifier(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IInitiatorIdentifier Current()
		{
			return _unitOfWork.Current().Initiator();
		}
	}
}