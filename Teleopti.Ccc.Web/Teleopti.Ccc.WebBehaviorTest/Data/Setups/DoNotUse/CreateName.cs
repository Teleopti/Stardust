using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class CreateName
	{
		public Name FromString(string name)
		{
			if (name.Contains(" "))
			{
				var splitted = name.Split(' ');
				return new Name(splitted[0], splitted[1]);
			}
			return new Name("", name);
		}
	}
}