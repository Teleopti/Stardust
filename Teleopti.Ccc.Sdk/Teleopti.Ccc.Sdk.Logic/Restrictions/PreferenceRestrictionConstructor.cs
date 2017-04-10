using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.Restrictions
{
    public class PreferenceRestrictionConstructor : IDomainAndDtoConstructor<IPreferenceRestriction,PreferenceRestrictionDto>
    {
        public IPreferenceRestriction CreateNewDomainObject()
        {
            return new PreferenceRestriction();
        }

        public PreferenceRestrictionDto CreateNewDto()
        {
            return new PreferenceRestrictionDto();
        }
    }

    public class PreferenceRestrictionTemplateConstructor : IDomainAndDtoConstructor<IPreferenceRestrictionTemplate, ExtendedPreferenceTemplateDto>
    {
        public IPreferenceRestrictionTemplate CreateNewDomainObject()
        {
            return new PreferenceRestrictionTemplate();
        }

        public ExtendedPreferenceTemplateDto CreateNewDto()
        {
            return new ExtendedPreferenceTemplateDto();
        }
    }

    public class ActivityRestrictionDomainObjectCreator : IActivityRestrictionDomainObjectCreator<IActivityRestriction>
    {
        public IActivityRestriction CreateNewDomainObject(IActivity activity)
        {
            return new ActivityRestriction(activity);
        }
    }

    public class ActivityRestrictionTemplateDomainObjectCreator : IActivityRestrictionDomainObjectCreator<IActivityRestrictionTemplate>
    {
        public IActivityRestrictionTemplate CreateNewDomainObject(IActivity activity)
        {
            return new ActivityRestrictionTemplate(activity);
        }
    }
}
