using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
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