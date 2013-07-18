using System;
using Autofac;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    /// <summary>
    /// This one is used to avoid creating all possible instances for every instance of the SDK.
    /// </summary>
    public interface IFactoryProvider
    {
        ScheduleFactory CreateScheduleFactory(ILifetimeScope lifetimeScope);
        IPersonRequestFactory CreatePersonRequestFactory(ILifetimeScope lifetimeScope);
        LicenseFactory CreateLicenseFactory();
        IAssembler<IPerson, PersonDto> CreatePersonAssembler();
        PersonsFromLoadOptionFactory CreatePersonsFromLoadOptionFactory(ILifetimeScope lifetimeScope);
        PublicNoteTypeFactory CreatePublicNoteTypeFactory();
        ScheduleMailFactory CreateScheduleMailFactory(ILifetimeScope lifetimeScope);
        IAssembler<IPreferenceDay, PreferenceRestrictionDto> CreatePreferenceDayAssembler();
        IAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto> CreateStudentAvailabilityDayAssembler();
        PlanningTimeBankFactory CreatePlanningTimeBankFactory(ILifetimeScope lifetimeScope);
        WriteProtectionFactory CreateWriteProtectionFactory(ILifetimeScope lifetimeScope);
    	IAssembler<IPushMessageDialogue, PushMessageDialogueDto> CreatePushMessageDialogueAssembler();
    }

    public class FactoryProvider : IFactoryProvider
    {
        private readonly IComponentContext _container;

        public FactoryProvider(IComponentContext container)
        {
            _container = container;
        }

        public ScheduleFactory CreateScheduleFactory(ILifetimeScope lifetimeScope)
        {
            return lifetimeScope.Resolve<ScheduleFactory>();
        }

        public IPersonRequestFactory CreatePersonRequestFactory(ILifetimeScope lifetimeScope)
        {
            return lifetimeScope.Resolve<IPersonRequestFactory>();
        }

        public LicenseFactory CreateLicenseFactory()
        {
            return _container.Resolve<LicenseFactory>();
        }

        public IAssembler<IPerson, PersonDto> CreatePersonAssembler()
        {
            return _container.Resolve<IAssembler<IPerson, PersonDto>>();
        }

		public IAssembler<IPushMessageDialogue, PushMessageDialogueDto> CreatePushMessageDialogueAssembler()
		{
			return _container.Resolve<IAssembler<IPushMessageDialogue, PushMessageDialogueDto>>();
		}

        public PersonsFromLoadOptionFactory CreatePersonsFromLoadOptionFactory(ILifetimeScope lifetimeScope)
        {
            return lifetimeScope.Resolve<PersonsFromLoadOptionFactory>();
        }

        public PublicNoteTypeFactory CreatePublicNoteTypeFactory()
        {
            return _container.Resolve<PublicNoteTypeFactory>();
        }

        public ScheduleMailFactory CreateScheduleMailFactory(ILifetimeScope lifetimeScope)
        {
            return _container.Resolve<ScheduleMailFactory>();
        }

        public IAssembler<IPreferenceDay, PreferenceRestrictionDto> CreatePreferenceDayAssembler()
        {
            return _container.Resolve<IAssembler<IPreferenceDay, PreferenceRestrictionDto>>();
        }

        public IAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto> CreateStudentAvailabilityDayAssembler()
        {
            return _container.Resolve<IAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto>>();
        }

        public PlanningTimeBankFactory CreatePlanningTimeBankFactory(ILifetimeScope lifetimeScope)
        {
            return _container.Resolve<PlanningTimeBankFactory>();
        }

        public WriteProtectionFactory CreateWriteProtectionFactory(ILifetimeScope lifetimeScope)
        {
            return _container.Resolve<WriteProtectionFactory>();
        }
    }
}