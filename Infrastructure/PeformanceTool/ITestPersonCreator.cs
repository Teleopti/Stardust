namespace Teleopti.Ccc.Infrastructure.PeformanceTool
{
	public interface ITestPersonCreator
	{
		void CreatePersons(int numberOfPersons);
		void RemoveCreatedPersons();
	}
}