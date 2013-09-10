﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Win.Intraday
{
    public class IntradayModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            groupMain(builder);
            commandWiring(builder);
            intradayGlobalWiring(builder);
        }

        private static void intradayGlobalWiring(ContainerBuilder builder)
        {
            builder.RegisterType<SchedulingResultLoader>().As<ISchedulingResultLoader>().InstancePerLifetimeScope();
            builder.RegisterType<RtaStateHolder>().As<IRtaStateHolder>().InstancePerLifetimeScope();
            builder.RegisterType<RangeProjectionService>().As<IRangeProjectionService>().InstancePerLifetimeScope();
            builder.RegisterType<PeopleLoader>().As<IPeopleLoader>().InstancePerLifetimeScope();
            builder.RegisterType<IntradaySelectedEntitiesForPeriod>().As<ISelectedEntitiesForPeriod>().InstancePerLifetimeScope();
        }

        private static void commandWiring(ContainerBuilder builder)
        {
            builder.RegisterType<OnEventScheduleMessageCommand>().InstancePerLifetimeScope();
            builder.RegisterType<OnEventForecastDataMessageCommand>().InstancePerLifetimeScope();
            builder.RegisterType<OnEventStatisticMessageCommand>().InstancePerLifetimeScope();
            builder.RegisterType<OnEventMeetingMessageCommand>().InstancePerLifetimeScope();
            builder.RegisterType<LoadStatisticsAndActualHeadsCommand>().InstancePerLifetimeScope();
            builder.RegisterType<LoadScheduleByPersonSpecification>().InstancePerLifetimeScope();
        }

        private class DummyScheduleUpdated : IUpdateScheduleDataFromMessages
        {
            public IPersistableScheduleData DeleteScheduleData(IEventMessage eventMessage)
            {
                throw new NotImplementedException();
            }

            public IPersistableScheduleData UpdateInsertScheduleData(IEventMessage eventMessage)
            {
                throw new NotImplementedException();
            }

            public void FillReloadedScheduleData(IPersistableScheduleData databaseVersionOfEntity)
            {
                var changeInfo = databaseVersionOfEntity as IChangeInfo;
                if (changeInfo != null)
                    LazyLoadingManager.Initialize(changeInfo.UpdatedBy);
            }

            public void NotifyMessageQueueSizeChange()
            {
            }
        }

        private static void groupMain(ContainerBuilder builder)
        {
            builder.RegisterType<IntradayView>()
                   .As<IIntradayView>()
                   .OnActivated(
                       e =>
                       e.Instance.Presenter =
                       e.Context.Resolve<IntradayPresenter>(TypedParameter.From(e.Context.Resolve<OnEventScheduleMessageCommand>(TypedParameter.From(e.Context.Resolve<IScheduleRefresher>(TypedParameter.From<IUpdateScheduleDataFromMessages>(new DummyScheduleUpdated())))))))
                   .InstancePerLifetimeScope();
            builder.RegisterType<IntradayMainModel>().InstancePerLifetimeScope();
            builder.RegisterType<HandleIntradayScope>().As<IIntradayViewFactory>();
            builder.RegisterType<IntradayPresenter>().InstancePerLifetimeScope();
        }

        private class HandleIntradayScope : IIntradayViewFactory
        {
            private readonly IComponentContext _container;
            private readonly IDictionary<IIntradayView, ILifetimeScope> innerScopes;

            public HandleIntradayScope(IComponentContext container)
            {
                _container = container;
                innerScopes = new Dictionary<IIntradayView, ILifetimeScope>();
            }

            public IIntradayView Create(DateOnlyPeriod dateOnlyPeriod, IScenario scenario, IEnumerable<IEntity> selectedEntities)
            {
                var lifetimeScope = _container.Resolve<ILifetimeScope>();
                var inner = lifetimeScope.BeginLifetimeScope();

                var intradayMainModel = inner.Resolve<IntradayMainModel>();
                intradayMainModel.Period = dateOnlyPeriod;
                intradayMainModel.Scenario = scenario;
                intradayMainModel.EntityCollection = selectedEntities;

                var stateHolder = inner.Resolve<ISchedulerStateHolder>();
                stateHolder.SetRequestedScenario(intradayMainModel.Scenario);
                stateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(intradayMainModel.Period,TeleoptiPrincipal.Current.Regional.TimeZone);
            
                var intraday = inner.Resolve<IIntradayView>();
				intraday.Scenario = scenario;
				var form = (Form)intraday;
                form.Show();
                //correct event?
                form.FormClosed += mainFormClosed;
				innerScopes[intraday] = inner;
				return intraday;
            }

            private void mainFormClosed(object sender, FormClosedEventArgs e)
            {
                var form = (IIntradayView)sender;
                innerScopes[form].Dispose();
                innerScopes.Remove(form);
            }
        }
    }
}
