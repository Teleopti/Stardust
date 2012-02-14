using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
    public class QuickForecastPresenter
    {
        private readonly IQuickForecastView _view;
        private readonly QuickForecastModel _model;
        private readonly IQuickForecastScenarioProvider _quickForecastScenarioProvider;
        private readonly IWorkloadProvider _workloadProvider;
        private readonly IQuickForecastPeriodProvider _quickForecastPeriodProvider;
        private readonly QuickForecastCommand _quickForecastCommand;

        public QuickForecastPresenter(IQuickForecastView view, QuickForecastModel model, IQuickForecastScenarioProvider quickForecastScenarioProvider, IWorkloadProvider workloadProvider, IQuickForecastPeriodProvider quickForecastPeriodProvider, QuickForecastCommand quickForecastCommand)
        {
            _view = view;
            _model = model;
            _quickForecastScenarioProvider = quickForecastScenarioProvider;
            _workloadProvider = workloadProvider;
            _quickForecastPeriodProvider = quickForecastPeriodProvider;
            _quickForecastCommand = quickForecastCommand;
        }

        public void Initialize()
        {
            _view.SetWorkloadCollection(_workloadProvider.WorkloadCollection.Select(w => new WorkloadModel(w)));

            var scenarioList = _quickForecastScenarioProvider.AllScenarios.Select(s => new ScenarioModel(s));
            _view.SetScenarioCollection(scenarioList);
            _view.SetSelectedScenario(scenarioList.FirstOrDefault(s => s.Scenario.Id == _quickForecastScenarioProvider.DefaultScenario.Id));
            _view.SetStatisticPeriod(_quickForecastPeriodProvider.DefaultStatisticPeriod);
            _view.SetTargetPeriod(_quickForecastPeriodProvider.DefaultTargetPeriod);
        }

        public void AddWorkload(WorkloadModel workload)
        {
            _model.SelectedWorkloads.Add(workload);
        }

        public void RemoveWorkload(WorkloadModel workload)
        {
            _model.SelectedWorkloads.Remove(workload);
        }

        public void ClearWorkloads()
        {
            _model.SelectedWorkloads.Clear();
        }

        public void ToggleUpdateStandardTemplates(bool enable)
        {
            _model.UpdateStandardTemplates = enable;
        }

        public void SetStatisticPeriod(DateOnlyPeriod period)
        {
            _model.StatisticPeriod = period;
        }

        public void SetTargetPeriod(DateOnlyPeriod period)
        {
            _model.TargetPeriod = period;
        }

        public void ExecuteAutoForecast()
        {
            _quickForecastCommand.WorkloadForecasted += QuickForecastCommandWorkloadForecasted;
            _quickForecastCommand.WorkloadForecasting += QuickForecastCommandWorkloadForecasting;
            _quickForecastCommand.Execute();
            _quickForecastCommand.WorkloadForecasted -= QuickForecastCommandWorkloadForecasted;
            _quickForecastCommand.WorkloadForecasting -= QuickForecastCommandWorkloadForecasting;
        }

        private void QuickForecastCommandWorkloadForecasting(object sender, CustomEventArgs<WorkloadModel> e)
        {
            _view.AppendWorkInProgress(string.Format(CultureInfo.CurrentUICulture,
                                                     "Running auto forecasting for workload {0} (belongs to skill {1}).", e.Value.Name,e.Value.SkillName));
        }

        void QuickForecastCommandWorkloadForecasted(object sender, CustomEventArgs<WorkloadModel> e)
        {
            _view.AppendWorkInProgress(string.Format(CultureInfo.CurrentUICulture,
                                                     "Done with auto forecasting for workload {0} (belongs to skill {1}).", e.Value.Name, e.Value.SkillName));
        }

        public void SetScenario(ScenarioModel scenarioModel)
        {
            _model.Scenario = scenarioModel.Scenario;
        }
    }
}
