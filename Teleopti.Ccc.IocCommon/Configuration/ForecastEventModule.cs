using System;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Interfaces.Infrastructure;

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
			builder.RegisterType<SaveForecastToSkillCommand>().As<ISaveForecastToSkillCommand>();
			builder.RegisterType<MultisiteForecastToSkillCommand>().As<IMultisiteForecastToSkillCommand>();
			builder.RegisterType<OpenAndSplitSkillCommand>().As<IOpenAndSplitSkillCommand>();
			builder.RegisterType<ImportForecastToSkillCommand>().As<IImportForecastToSkillCommand>();
			builder.RegisterType<SendImportForecastBusMessage>().As<ISendBusMessage>();
		}

		private static IJobResultFeedback getThreadJobResultFeedback(IComponentContext componentContext)
		{
			return jobResultFeedback ?? (jobResultFeedback = new JobResultFeedback(componentContext.Resolve<ICurrentUnitOfWorkFactory>()));
		}
	}
}
