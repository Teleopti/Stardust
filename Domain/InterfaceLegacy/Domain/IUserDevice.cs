namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IUserDevice:IAggregateRoot
	{
		IPerson Owner { get; set; }
		string Token { get; set; }
	}
}