namespace Teleopti.Ccc.Domain.Auditing
{
	public interface IPersonAccessPersister
	{
		void Persist(PersonAccess model);
	}
}
