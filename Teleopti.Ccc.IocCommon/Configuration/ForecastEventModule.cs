using System;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.ApplicationLayer.SkillDay;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ForecastEventModule : Module
	{

		[ThreadStatic]
		private static IJobResultFeedback jobResultFeedback;
		private readonly IocConfiguration _config;

		public ForecastEventModule(IocConfiguration config)
		{
			_config = config;
		}


		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<WorkloadDayHelper>();
			
			builder.RegisterType<ForecastsRowExtractor>().As<IForecastsRowExtractor>();
			builder.RegisterType<ForecastsFileContentProvider>().As<IForecastsFileContentProvider>();
			builder.RegisterType<ForecastsAnalyzeQuery>().As<IForecastsAnalyzeQuery>();
			builder.Register(getThreadJobResultFeedback).As<IJobResultFeedback>().SingleInstance();
			builder.RegisterType<SkillDayLoadHelper>().As<ISkillDayLoadHelper>().SingleInstance();
			builder.RegisterType<SaveForecastToSkill>().As<ISaveForecastToSkill>();
			builder.RegisterType<MultisiteForecastToSkillAnalyzer>().As<IMultisiteForecastToSkillCommand>();
			builder.RegisterType<OpenAndSplitSkillCommand>().As<IOpenAndSplitSkillCommand>();
			builder.RegisterType<SplitImportForecastMessage>().As<ISplitImportForecastMessage>();
			builder.RegisterType<ForecastClassesCreator>().As<IForecastClassesCreator>();
			builder.RegisterType<StatisticLoader>().As<IStatisticLoader>();
			builder.RegisterType<ReforecastPercentCalculator>().As<IReforecastPercentCalculator>();
			builder.RegisterType<Statistic>().As<IStatistic>();
			builder.RegisterType<ImportForecastProcessor>().As<IImportForecastProcessor>();
			builder.RegisterType<StatisticHelper>().As<IStatisticHelper>();
			builder.RegisterType<OpenAndSplitTargetSkill>().As<IOpenAndSplitTargetSkill>();
			builder.RegisterType<ExportMultisiteSkillProcessor>().As<IExportMultisiteSkillProcessor>();
			builder.RegisterType<StaffingCalculatorServiceFacade>().As<IStaffingCalculatorServiceFacade>().SingleInstance();
			if (_config.IsToggleEnabled(Toggles.WFM_Forecast_Readmodel_80790))
			{
				builder.RegisterType<SkillForecastIntervalCalculator>().SingleInstance();
				builder.RegisterType<SkillForecastReadModelPeriodBuilder>().SingleInstance();
				builder.RegisterType<SkillForecastJobStartTimeRepository>().As<ISkillForecastJobStartTimeRepository>().SingleInstance();
				builder.RegisterType<SkillForecastSettingsReader>().SingleInstance();

			}
			

		}

		private static IJobResultFeedback getThreadJobResultFeedback(IComponentContext componentContext)
		{
			return jobResultFeedback ?? (jobResultFeedback = new JobResultFeedback(componentContext.Resolve<ICurrentUnitOfWorkFactory>()));
		}
	}
}
