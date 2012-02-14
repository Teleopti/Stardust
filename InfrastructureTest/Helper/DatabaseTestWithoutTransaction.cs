namespace Teleopti.Ccc.InfrastructureTest.Helper
{
	public abstract class DatabaseTestWithoutTransaction : DatabaseTest
	{
		protected override sealed void SetupForRepositoryTest()
		{
			SkipRollback();
			UnitOfWork.PersistAll();
			SetupForRepositoryTestWithoutTransaction();
		}

		protected virtual void SetupForRepositoryTestWithoutTransaction()
		{
		}
	}
}