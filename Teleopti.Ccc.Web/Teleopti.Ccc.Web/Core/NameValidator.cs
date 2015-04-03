namespace Teleopti.Ccc.Web.Core
{
	public class NameValidator
	{
		public static bool DescriptionIsInvalid(string newDescription)
		{
			return string.IsNullOrEmpty(newDescription) || newDescription.Length > 255;
		}
	}
}