﻿using Autofac;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core.IoC
{
	public class AnywhereAreaModule : Module
	{
		private readonly IocConfiguration _config;

		public AnywhereAreaModule(IocConfiguration config)
		{
			_config = config;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ExceptionHandlerPipelineModule>().As<IHubPipelineModule>();

			if (!_config.Args().DisableWebSocketCors)
			{
				builder.RegisterType<OriginHandlerPipelineModule>().As<IHubPipelineModule>();
			}
			
			builder.RegisterType<PersonScheduleViewModelFactory>().As<IPersonScheduleViewModelFactory>().SingleInstance();
			builder.RegisterType<PersonScheduleViewModelMapper>().As<PersonScheduleViewModelMapper>().SingleInstance();

			builder.Register(c => new ReportUrlConstructor(_config.Args().ReportServer, c.Resolve<IConfigReader>()))
					.As<IReportUrl>()
					.SingleInstance();
			
			builder.RegisterType<SkillDayLoadHelper>().As<ISkillDayLoadHelper>().SingleInstance();
			
			builder.RegisterType<PeriodDistributionService>().As<IPeriodDistributionService>().SingleInstance();

			builder.RegisterType<NonBlendSkillCalculator>().As<INonBlendSkillCalculator>();
			builder.RegisterType<NonBlendSkillImpactOnPeriodForProjection>().As<INonBlendSkillImpactOnPeriodForProjection>();
			builder.RegisterType<OccupiedSeatCalculator>().As<IOccupiedSeatCalculator>();
			builder.RegisterType<SingleSkillDictionary>().As<ISingleSkillDictionary>().InstancePerLifetimeScope();

			builder.RegisterType<IntraIntervalFinderService>().As<IIntraIntervalFinderService>();
			
			builder.RegisterType<PersonAbsenceCreator>().As<IPersonAbsenceCreator>();
			builder.RegisterType<PersonAbsenceRemover>().As<IPersonAbsenceRemover>();
			builder.RegisterType<AbsenceCommandConverter>().As<IAbsenceCommandConverter>();
			builder.RegisterType<SaveSchedulePartService>().As<ISaveSchedulePartService>();	
			builder.RegisterType<BusinessRulesForPersonalAccountUpdate>().As<IBusinessRulesForPersonalAccountUpdate>();
			builder.RegisterType<ScheduleDifferenceSaver>().As<IScheduleDifferenceSaver>();
			builder.RegisterType<ScheduleDayDifferenceSaver>().As<IScheduleDayDifferenceSaver>().SingleInstance();
			
		}
	}
}