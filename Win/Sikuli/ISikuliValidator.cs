using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Win.Sikuli
{
	public interface ISikuliValidator
	{
		SikuliValidationResult Validate();
		string Description { get; }
	}
}
