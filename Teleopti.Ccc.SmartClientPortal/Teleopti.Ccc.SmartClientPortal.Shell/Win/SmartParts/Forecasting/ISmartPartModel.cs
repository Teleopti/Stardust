using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts.Forecasting
{
    public interface ISmartPartModel
    {
        IScenario DefaultScenario { get; }
        ISkill Skill { get; }
        Guid SkillId { get; set; }
        IList<NamedEntity> WorkloadNames { get; }
        IList<EntityUpdateInformation> WorkloadUpdatedInfo { get; }
        IScenario GetScenarioById(Guid scenarioId);
        IDictionary<Guid, IList<IForecastProcessReport>> ProcessValidations(DateOnlyPeriod period);
        IDictionary<Guid, IDictionary<Guid, IList<IForecastProcessReport>>> ProcessAllBudgetForecasting(DateOnlyPeriod period);
        IDictionary<Guid, IDictionary<Guid, IList<IForecastProcessReport>>> ProcessAllDetailedForecasting(DateOnlyPeriod period);
        void ProcessTemplates();
        IDictionary<IScenario, EntityUpdateInformation> SetLastUpdatedWorkloadDetailedValuesOfAllScenarios(bool longterm);
        EntityUpdateInformation SetLastUpdatedValidatedVolumnDayValues();
        IList<IScenario> FindLastUpdatedScenarios(
            IDictionary<IScenario, EntityUpdateInformation> allLastUpdatedScenarios);
    }
}