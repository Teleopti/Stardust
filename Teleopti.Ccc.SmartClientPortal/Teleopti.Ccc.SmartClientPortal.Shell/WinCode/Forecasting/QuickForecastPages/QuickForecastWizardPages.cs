using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.QuickForecastPages
{
	public class QuickForecastWizardPages : AbstractWizardPagesNoRoot<QuickForecastModel>
	{
		private readonly QuickForecastModel _stateObj;

		public QuickForecastWizardPages(QuickForecastModel stateObj)
			: base(stateObj)
		{
			_stateObj = stateObj;
		}

		public override QuickForecastModel CreateNewStateObj()
		{
			return _stateObj;
		}

		public QuickForecastWorkloadsEvent CreateServiceBusMessage()
		{
			return new QuickForecastWorkloadsEvent
		{
			TargetPeriodStart = _stateObj.TargetPeriod.StartDate.Date,
            TargetPeriodEnd = _stateObj.TargetPeriod.EndDate.Date,
			ScenarioId = _stateObj.ScenarioId,
			SmoothingStyle = _stateObj.SmoothingStyle,
			TemplatePeriodStart = _stateObj.TemplatePeriod.StartDate.Date,
			TemplatePeriodEnd = _stateObj.TemplatePeriod.EndDate.Date,
			WorkloadIds = _stateObj.WorkloadIds,
			IncreaseWith = _stateObj.IncreaseWith,
			UseDayOfMonth = _stateObj.UseDayOfMonth,
			StatisticPeriodStart = _stateObj.StatisticPeriod.StartDate.Date,
			StatisticPeriodEnd = _stateObj.StatisticPeriod.EndDate.Date
		};
		}

		public override string Name
		{
			get { return Resources.QuickForecast; }
		}

		public override string WindowText
		{
			get { return Resources.QuickForecast; }
		}
	}

	public class QuickForecastModel
	{
		public QuickForecastModel()
		{
			WorkloadIds = new Collection<Guid>();
			SmoothingStyle = 5;
			StatisticPeriod = new DateOnlyPeriod
			(
				new DateOnly(DateTime.Today.AddYears(-2) ),
				new DateOnly(DateTime.Today.AddDays(-1) )
			);
			var start = DateTime.Today.AddMonths(1);
			start = new DateTime(start.Year, start.Month, 1);
			var end = start.AddMonths(3).AddDays(-1);
			TargetPeriod = new DateOnlyPeriod
			(
				new DateOnly(start),
				new DateOnly(end)
			);

			TemplatePeriod = new DateOnlyPeriod
			(
				new DateOnly(DateTime.Today.AddMonths(-3).AddDays(-1) ),
				new DateOnly (DateTime.Today.AddDays(-1) )
			);
		}

		public Guid ScenarioId { get; set; }

		public DateOnlyPeriod StatisticPeriod { get; set; }

		public DateOnlyPeriod TargetPeriod { get; set; }

		public bool UpdateStandardTemplates { get; set; }

		public int SmoothingStyle { get; set; }

		public DateOnlyPeriod TemplatePeriod { get; set; }

		public ICollection<Guid> WorkloadIds { get; set; }

		public int IncreaseWith { get; set; }

		public bool UseDayOfMonth { get; set; }
	}
}
