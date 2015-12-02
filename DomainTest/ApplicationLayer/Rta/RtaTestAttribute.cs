using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[LoggedOff]
	[Toggle(Domain.FeatureFlags.Toggles.RTA_DeletedPersons_36041)]
	public class RtaTestAttribute : DomainTestAttribute
	{
	}
}