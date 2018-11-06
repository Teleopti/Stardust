namespace Teleopti.Wfm.Adherence.Test.InfrastructureTesting
{
	public abstract class DatabaseTestWithoutTransaction : DatabaseTest
	{
		protected override sealed void SetupForRepositoryTest()
		{
			//CleanUpAfterTest();
			UnitOfWork.PersistAll();
			SetupForRepositoryTestWithoutTransaction();
		}

		protected virtual void SetupForRepositoryTestWithoutTransaction()
		{
		}
	}
}