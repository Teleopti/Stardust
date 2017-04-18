using Autofac;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Grouping
{
	public class PersonSelectorModule : Module
	{
		private readonly IIocConfiguration _config;

		public PersonSelectorModule(IIocConfiguration config)
		{
			_config = config;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PersonSelectorView>().As<IPersonSelectorView>().InstancePerLifetimeScope();
			builder.RegisterType<PersonSelectorPresenter>().As<IPersonSelectorPresenter>().InstancePerLifetimeScope();
			builder.RegisterType<CommandProvider>().As<ICommandProvider>().InstancePerLifetimeScope();
			builder.RegisterType<OpenModuleCommand>().As<IOpenModuleCommand>().InstancePerLifetimeScope();
			builder.RegisterType<AddGroupPageCommand>().As<IAddGroupPageCommand>().InstancePerLifetimeScope();
			builder.RegisterType<ModifyGroupPageCommand>().As<IModifyGroupPageCommand>().InstancePerLifetimeScope();
			builder.RegisterType<DeleteGroupPageCommand>().As<IDeleteGroupPageCommand>().InstancePerLifetimeScope();
			builder.RegisterType<RenameGroupPageCommand>().As<IRenameGroupPageCommand>().InstancePerLifetimeScope();
			builder.RegisterType<SendInstantMessageCommand>().As<ISendInstantMessageCommand>().InstancePerLifetimeScope();
			builder.RegisterType<SendInstantMessageEnableCommand>().As<ISendInstantMessageEnableCommand>().InstancePerLifetimeScope();
			builder.RegisterType<OpenMeetingsOverviewCommand>().As<IOpenMeetingsOverviewCommand>().InstancePerLifetimeScope();
			builder.RegisterType<AddMeetingFromPanelCommand>().As<IAddMeetingFromPanelCommand>().InstancePerLifetimeScope();
			builder.RegisterType<OpenIntradayTodayCommand>().As<IOpenIntradayTodayCommand>().InstancePerLifetimeScope();

			builder.RegisterType<PeopleNavigatorPresenter>().As<IPeopleNavigatorPresenter>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleNavigatorPresenter>().As<IScheduleNavigatorPresenter>().InstancePerLifetimeScope();
			builder.RegisterType<GroupPageHelper>().As<IGroupPageHelper>().InstancePerLifetimeScope();
			builder.RegisterType<AddPersonEnableCommand>().As<IAddPersonEnableCommand>().InstancePerLifetimeScope();

			
		}

		
	}
}