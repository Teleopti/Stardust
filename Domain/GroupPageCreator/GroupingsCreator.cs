using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
    public class GroupingsCreator : IGroupingsCreator
    {
        private readonly IGroupPageDataProvider _groupPageDataProvider;

        public GroupingsCreator(IGroupPageDataProvider groupPageDataProvider)
        {
            _groupPageDataProvider = groupPageDataProvider;
        }

        public IList<IGroupPage> CreateBuiltInGroupPages(bool includeNewHierarchyGrouping)
        {
            IList<IGroupPage> pages = new List<IGroupPage>();

            IGroupPageOptions options = new GroupPageOptions(_groupPageDataProvider.PersonCollection);
            options.SelectedPeriod = _groupPageDataProvider.SelectedPeriod;
            
            IGroupPage groupPage;

            if (includeNewHierarchyGrouping)
            {
                PersonGroupPage personGroupPage = new PersonGroupPage();
                options.CurrentGroupPageName = Resources.Main;
                options.CurrentGroupPageNameKey = "Main";
                groupPage = personGroupPage.CreateGroupPage(new[] { _groupPageDataProvider.BusinessUnit}, options);
                pages.Add(groupPage);
            }

            ContractGroupPage contractGroupPage = new ContractGroupPage();
            options.CurrentGroupPageName = Resources.Contracts;
            options.CurrentGroupPageNameKey = "Contracts";
            groupPage = contractGroupPage.CreateGroupPage(_groupPageDataProvider.ContractCollection, options);
            pages.Add(groupPage);

            ContractScheduleGroupPage contractScheduleGroupPage = new ContractScheduleGroupPage();
            options.CurrentGroupPageName = Resources.ContractSchedule;
            options.CurrentGroupPageNameKey = "ContractSchedule";
            groupPage = contractScheduleGroupPage.CreateGroupPage(_groupPageDataProvider.ContractScheduleCollection, options);
            pages.Add(groupPage);

            PartTimePercentageGroupPage partTimePercentageGroupPage = new PartTimePercentageGroupPage();
            options.CurrentGroupPageName = Resources.PartTimepercentages;
            options.CurrentGroupPageNameKey = "PartTimepercentages";
            groupPage = partTimePercentageGroupPage.CreateGroupPage(_groupPageDataProvider.PartTimePercentageCollection, options);
            pages.Add(groupPage);

            PersonNoteGroupPage personNoteGroupPage = new PersonNoteGroupPage();
            options.CurrentGroupPageName = Resources.Note;
            options.CurrentGroupPageNameKey = "Note";
            groupPage = personNoteGroupPage.CreateGroupPage(null, options);
            pages.Add(groupPage);

            RuleSetBagGroupPage ruleSetBagGroupPage = new RuleSetBagGroupPage();
            options.CurrentGroupPageName = Resources.RuleSetBag;
            options.CurrentGroupPageNameKey = "RuleSetBag";
            groupPage = ruleSetBagGroupPage.CreateGroupPage(_groupPageDataProvider.RuleSetBagCollection, options);
            pages.Add(groupPage);

            SkillGroupPage skillGroupPage = new SkillGroupPage();
            options.CurrentGroupPageName = Resources.Skill;
            options.CurrentGroupPageNameKey = "Skill";
            groupPage = skillGroupPage.CreateGroupPage(_groupPageDataProvider.SkillCollection, options);
            pages.Add(groupPage);

            return pages;
        }
    }
}