﻿using System;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ForecastEventModule : Module
	{
		private readonly IIocConfiguration _configuration;

		[ThreadStatic]
		private static IJobResultFeedback jobResultFeedback;

		public ForecastEventModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
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

			
			if (_configuration.Toggle(Toggles.ResourcePlanner_UseErlangAWithInfinitePatience_45845))
			{
				if (_configuration.Toggle(Toggles.ResourcePlanner_UseErlangAWithInfinitePatienceEsl_74899))
				{
					if (_configuration.Toggle(Toggles.ResourcePlanner_UseErlangAWithFinitePatience_47738))
					{
						builder.RegisterType<StaffingCalculatorServiceFacadeErlangAAbandon>().As<IStaffingCalculatorServiceFacade>().SingleInstance();
					}
					else
					{
						builder.RegisterType<StaffingCalculatorServiceFacadeErlangAWithEsl>().As<IStaffingCalculatorServiceFacade>().SingleInstance();
					}
				}
				else
				{
					builder.RegisterType<StaffingCalculatorServiceFacadeErlangA>().As<IStaffingCalculatorServiceFacade>().SingleInstance();
				}		
			}
			else
			{
				builder.RegisterType<StaffingCalculatorServiceFacade>().As<IStaffingCalculatorServiceFacade>().SingleInstance();
			}
		}

		private static IJobResultFeedback getThreadJobResultFeedback(IComponentContext componentContext)
		{
			return jobResultFeedback ?? (jobResultFeedback = new JobResultFeedback(componentContext.Resolve<ICurrentUnitOfWorkFactory>()));
		}
	}
}
