namespace Teleopti.Ccc.Sdk.Client.SdkServiceReference
{
    public interface ITeleoptiOrganizationService
    {
        PersonAccountDto[] GetPersonAccounts(PersonDto person, DateOnlyDto containingDate);
    }
}