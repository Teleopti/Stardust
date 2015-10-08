using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class FromFactory : ICurrentUnitOfWork
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public FromFactory(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public IUnitOfWork Current()
		{
			return _unitOfWorkFactory.CurrentUnitOfWork();
		}
	}
}