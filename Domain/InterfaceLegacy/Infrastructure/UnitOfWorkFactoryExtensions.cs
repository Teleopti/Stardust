namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public static class UnitOfWorkFactoryExtensions
	{
		public static bool HasCurrentUnitOfWork(this IUnitOfWorkFactory unitOfWorkFactory)
		{
			return unitOfWorkFactory.CurrentUnitOfWork() != null;
		}
	}
}