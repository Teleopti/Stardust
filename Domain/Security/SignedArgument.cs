namespace Teleopti.Ccc.Domain.Security
{
	public class SignedArgument<T>
	{
		public T Body { get; set; }
		public string Signature { get; set; }
	}
}