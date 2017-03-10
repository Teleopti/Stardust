﻿using System;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ForecastEventModule : Module
	{
		[ThreadStatic]
		private static IJobResultFeedback jobResultFeedback;

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<WorkloadDayHelper>();
			builder.RegisterType<ForecastsFileContentProvider>().As<IForecastsFileContentProvider>();
			builder.RegisterType<ForecastsAnalyzeQuery>().As<IForecastsAnalyzeQuery>();
			builder.RegisterType<ForecastsRowExtractor>().As<IForecastsRowExtractor>();
			builder.Register(getThreadJobResultFeedback).As<IJobResultFeedback>().SingleInstance();
			builder.RegisterType<SkillDayLoadHelper>().As<ISkillDayLoadHelper>().InstancePerLifetimeScope();
			builder.RegisterType<SaveForecastToSkill>().As<ISaveForecastToSkill>();
			builder.RegisterType<MultisiteForecastToSkillAnalyzer>().As<IMultisiteForecastToSkillCommand>();
			builder.RegisterType<OpenAndSplitSkillCommand>().As<IOpenAndSplitSkillCommand>();
			//builder.RegisterType<ImportForecastToSkillCommand>().As<IImportForecastToSkillCommand>();
			builder.RegisterType<SplitImportForecastMessage>().As<ISplitImportForecastMessage>();
			builder.RegisterType<ForecastClassesCreator>().As<IForecastClassesCreator>();
			builder.RegisterType<StatisticLoader>().As<IStatisticLoader>();
			builder.RegisterType<ReforecastPercentCalculator>().As<IReforecastPercentCalculator>();
			builder.RegisterType<Statistic>().As<IStatistic>();
			builder.RegisterType<ImportForecastProcessor>().As<IImportForecastProcessor>();
			builder.RegisterType<StatisticHelper>().As<IStatisticHelper>();
			builder.RegisterType<OpenAndSplitTargetSkill>().As<IOpenAndSplitTargetSkill>();
			builder.RegisterType<ExportMultisiteSkillProcessor>().As<IExportMultisiteSkillProcessor>();
		}

		private static IJobResultFeedback getThreadJobResultFeedback(IComponentContext componentContext)
		{
			return jobResultFeedback ?? (jobResultFeedback = new JobResultFeedback(componentContext.Resolve<ICurrentUnitOfWorkFactory>()));
		}
	}
}
