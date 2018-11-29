using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using Autofac;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;

using Module = Autofac.Module;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Meetings.Overview
{
    

    public class MeetingOverviewModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            overviewMain(builder);
        }
        
        private static void overviewMain(ContainerBuilder builder)
        {
            builder.RegisterType<MeetingOverviewViewModel>().As<IMeetingOverviewViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<MeetingOverviewViewFactory>().As<IMeetingOverviewViewFactory>();
            builder.RegisterType<MeetingOverviewPresenter>().InstancePerLifetimeScope();
            builder.RegisterType<MeetingOverviewFilter>().As<IMeetingOverviewFilter>().InstancePerLifetimeScope();

            builder.RegisterType<MeetingOverviewView>()
                .As<IMeetingOverviewView>()
                .OnActivated(e => e.Instance.Presenter = e.Context.Resolve<MeetingOverviewPresenter>())
                .InstancePerLifetimeScope();
            builder.RegisterType<MeetingsScheduleProvider>().As<MeetingsScheduleProvider>().InstancePerLifetimeScope();

            //builder.RegisterType<PersonsFromSelectedTeamsLoader>().As<IPersonsFromSelectedTeamsLoader>().InstancePerLifetimeScope();

            builder.RegisterType<ScheduleAppointmentFromMeetingCreator>().As<IScheduleAppointmentFromMeetingCreator>().InstancePerLifetimeScope();
            builder.RegisterType<InfoWindowTextFormatter>().As<IInfoWindowTextFormatter>().InstancePerLifetimeScope();

            builder.RegisterType<CanModifyMeeting>().As<ICanModifyMeeting>().InstancePerLifetimeScope();
            builder.RegisterType<DeleteMeetingCommand>().As<IDeleteMeetingCommand>().InstancePerLifetimeScope();
            builder.RegisterType<AddMeetingCommand>().As<IAddMeetingCommand>().InstancePerLifetimeScope();
            builder.RegisterType<EditMeetingCommand>().As<IEditMeetingCommand>().InstancePerLifetimeScope();
            builder.RegisterType<CopyMeetingCommand>().As<ICopyMeetingCommand>().InstancePerLifetimeScope();
            builder.RegisterType<PasteMeetingCommand>().As<IPasteMeetingCommand>().InstancePerLifetimeScope();
            builder.RegisterType<CutMeetingCommand>().As<ICutMeetingCommand>().InstancePerLifetimeScope();
			builder.RegisterType<FetchMeetingForCurrentUserChangeCommand>().As<IFetchMeetingForCurrentUserChangeCommand>().InstancePerLifetimeScope();

            builder.RegisterType<ShowExportMeetingCommand>().As<IShowExportMeetingCommand>().InstancePerLifetimeScope();
            builder.RegisterType<ExportMeetingView>().As<IExportMeetingView>().InstancePerLifetimeScope();
            builder.RegisterType<ExportMeetingPresenter>().As<IExportMeetingPresenter>().InstancePerLifetimeScope();
            builder.RegisterType<ExportableScenarioProvider>().As<IExportableScenarioProvider>().InstancePerLifetimeScope();
            builder.RegisterType<ExportMeetingCommand>().As<IExportMeetingCommand>().InstancePerLifetimeScope();
            builder.RegisterType<ExportMeetingsProvider>().As<IExportMeetingsProvider>().InstancePerLifetimeScope();

            builder.RegisterType<MeetingClipboardHandler>().As<IMeetingClipboardHandler>().InstancePerLifetimeScope();
            builder.RegisterType<MeetingParticipantPermittedChecker>().As<IMeetingParticipantPermittedChecker>().InstancePerLifetimeScope();
            builder.RegisterType<MeetingChangerAndPersister>().As<IMeetingChangerAndPersister>().InstancePerLifetimeScope();
			builder.RegisterType<OverlappingAppointmentsHelper>().As<IOverlappingAppointmentsHelper>().InstancePerLifetimeScope();

        }

        private class MeetingOverviewViewFactory : IMeetingOverviewViewFactory
        {
            private readonly IComponentContext _container;
            private readonly IDictionary<IMeetingOverviewView, ILifetimeScope> _innerScopes;

            public MeetingOverviewViewFactory(IComponentContext container)
            {
                _container = container;
                _innerScopes = new Dictionary<IMeetingOverviewView, ILifetimeScope>();
            }

            public IMeetingOverviewView Create(IEnumerable<Guid> selectedPersons, DateOnlyPeriod selectedPeriod, IScenario scenario)
            {
                try
                {
                    var lifetimeScope = _container.Resolve<ILifetimeScope>();
                    var inner = lifetimeScope.BeginLifetimeScope();

                    var model = inner.Resolve<IMeetingOverviewViewModel>();
                    model.CurrentScenario = scenario;
                    model.SelectedPeriod = selectedPeriod;
                    model.AllSelectedPersonsId = selectedPersons;
                    model.FilteredPersonsId = new List<Guid>(model.AllSelectedPersonsId);
                    
                    var meetingOverview = inner.Resolve<IMeetingOverviewView>();

                    var form = (Form)meetingOverview;
                    form.Show();
                    //correct event?
                    form.FormClosed += mainFormClosed;
                    _innerScopes[meetingOverview] = inner;
                    return meetingOverview;
                }
                catch (TargetInvocationException exception)
                {
                    if (exception.InnerException != null && exception.InnerException is DataSourceException)
                        throw exception.InnerException;
                    throw;
                }
            }

            private void mainFormClosed(object sender, FormClosedEventArgs e)
            {
                var form = (Form)sender;
                form.FormClosed -= mainFormClosed;
                var overview = (MeetingOverviewView) sender;
                _innerScopes[overview].Dispose();
                _innerScopes.Remove(overview);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
            public void ShowMeetingComposerView(IPersonSelectorView parent, IMeetingViewModel meetingViewModel, bool viewSchedulesPermission)
            {
                var meetingComposerView = new MeetingComposerView(meetingViewModel, null, true, viewSchedulesPermission,
															   new EventAggregator(), _container.Resolve<IResourceCalculation>(),
															   _container.Resolve<ISkillPriorityProvider>(),
															   _container.Resolve<IScheduleStorageFactory>(),
															   _container.Resolve<IStaffingCalculatorServiceFacade>(),
															   _container.Resolve<CascadingResourceCalculationContextFactory>());
                meetingComposerView.Show((Control)parent);
            }
        }
    }
}