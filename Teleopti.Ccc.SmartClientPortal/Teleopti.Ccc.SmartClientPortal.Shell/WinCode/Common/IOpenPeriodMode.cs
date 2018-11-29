using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public interface IOpenPeriodMode
    {
        string SettingName {get;}
        bool ConsiderRestrictedScenarios { get; }
        ISpecification<DateOnlyPeriod> Specification { get; }
        string AliasOfMaxNumberOfDays { get; }
        bool ForecasterStyle { get; }
    }
}