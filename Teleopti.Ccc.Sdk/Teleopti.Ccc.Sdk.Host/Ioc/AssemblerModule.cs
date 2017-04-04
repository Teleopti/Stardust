using System;
using Autofac;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.WcfService.Factory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfHost.Ioc
{
	//in i "logic" assembly + tests!
	public class AssemblerModule : Module
	{
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterAssemblyTypes(typeof(PersonAssembler).Assembly)
				.Where(isAssembler)
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
				.AsImplementedInterfaces()
                .InstancePerLifetimeScope();

		    builder.RegisterType<RestrictionAssembler<IPreferenceRestriction, PreferenceRestrictionDto, IActivityRestriction>>
		        ().As<IAssembler<IPreferenceRestriction, PreferenceRestrictionDto>>().InstancePerLifetimeScope();
		    builder.RegisterType<PreferenceRestrictionConstructor>().As
                <IDomainAndDtoConstructor<IPreferenceRestriction, PreferenceRestrictionDto>>().InstancePerLifetimeScope();

            builder.RegisterType<RestrictionAssembler<IPreferenceRestrictionTemplate, ExtendedPreferenceTemplateDto, IActivityRestriction>>
                ().As<IAssembler<IPreferenceRestrictionTemplate, ExtendedPreferenceTemplateDto>>().InstancePerLifetimeScope();
            builder.RegisterType<PreferenceRestrictionTemplateConstructor>().As
                <IDomainAndDtoConstructor<IPreferenceRestrictionTemplate, ExtendedPreferenceTemplateDto>>().InstancePerLifetimeScope();

		    builder.RegisterType<ActivityRestrictionDomainObjectCreator>().As
                <IActivityRestrictionDomainObjectCreator<IActivityRestriction>>().InstancePerLifetimeScope();
            builder.RegisterType<ActivityRestrictionTemplateDomainObjectCreator>().As
                <IActivityRestrictionDomainObjectCreator<IActivityRestrictionTemplate>>().InstancePerLifetimeScope();

            builder.RegisterType<ActivityRestrictionAssembler<IActivityRestriction>>().As
                <IAssembler<IActivityRestriction, ActivityRestrictionDto>>().InstancePerLifetimeScope();

            builder.RegisterType<ActivityRestrictionAssembler<IActivityRestrictionTemplate>>().As
                <IAssembler<IActivityRestrictionTemplate, ActivityRestrictionDto>>().InstancePerLifetimeScope();

            builder.RegisterType<ActivityLayerAssembler<MainShiftLayer>>().As<IActivityLayerAssembler<MainShiftLayer>>().InstancePerLifetimeScope();
            builder.RegisterType<ActivityLayerAssembler<PersonalShiftLayer>>().As<IActivityLayerAssembler<PersonalShiftLayer>>().InstancePerLifetimeScope();

        	builder.RegisterType<SdkProjectionServiceFactory>().As<ISdkProjectionServiceFactory>();

		}

		private static bool isAssembler(Type infrastructureType)
		{
			return infrastructureType.Name.EndsWith("Assembler", StringComparison.Ordinal);
		}
	}
}