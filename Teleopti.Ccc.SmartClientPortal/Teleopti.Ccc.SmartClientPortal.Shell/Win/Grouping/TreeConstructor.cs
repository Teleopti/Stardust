using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Grouping
{
    /// <summary>
    /// This will create GroupPages based on dynamic option types
    /// </summary>
    /// <remarks>
    /// Created by: Madhuranga Pinnagoda
    /// Created date: 2008-06-25
    /// </remarks>
    public class TreeConstructor
    {
        private readonly IGroupPageDataProvider _groupPageDataProvider;

        public TreeConstructor(IGroupPageDataProvider groupPageDataProvider)
        {
            _groupPageDataProvider = groupPageDataProvider;
        }

        /// <summary>
        /// Creates the group page.
        /// </summary>
        /// <param name="pageName">The name for the page.</param>
        /// <param name="optionType">Type of the option.</param>
        /// <param name="optionalColumnId">The optional column id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-06-25
        /// </remarks>
        public IGroupPage CreateGroupPage(string pageName, DynamicOptionType optionType, Guid? optionalColumnId)
        {
            IGroupPage returnVal;

            IGroupPageOptions groupPageOptions = new GroupPageOptions(_groupPageDataProvider.PersonCollection);
            groupPageOptions.CurrentGroupPageName = pageName;
            IGroupPageCreator<IPerson> option;
            //Check whether user select no grouping.
            if (!optionType.Equals(DynamicOptionType.DoNotGroup))
            {
                switch (optionType)
                {
                    case DynamicOptionType.Contract:
                        var contractGroupPage = new ContractGroupPage();
                        returnVal = contractGroupPage.CreateGroupPage(_groupPageDataProvider.ContractCollection, groupPageOptions);
                        return returnVal;
                    case DynamicOptionType.ContractSchedule:
                        var contractScheduleGroupPage = new ContractScheduleGroupPage();
                        returnVal = contractScheduleGroupPage.CreateGroupPage(_groupPageDataProvider.ContractScheduleCollection, groupPageOptions);
                        return returnVal;
                    case DynamicOptionType.PartTimePercentage:
                        var partTimePercentageGroupPage = new PartTimePercentageGroupPage();
                        returnVal = partTimePercentageGroupPage.CreateGroupPage(_groupPageDataProvider.PartTimePercentageCollection, groupPageOptions);
                        return returnVal;
                    case DynamicOptionType.PersonNote:
                        var personNoteGroupPage = new PersonNoteGroupPage();
                        returnVal = personNoteGroupPage.CreateGroupPage(_groupPageDataProvider.PersonCollection, groupPageOptions);
                        return returnVal;
                    case DynamicOptionType.RuleSetBag:
                        var ruleSetBagGroupPage = new RuleSetBagGroupPage();
                        returnVal = ruleSetBagGroupPage.CreateGroupPage(_groupPageDataProvider.RuleSetBagCollection, groupPageOptions);
                        return returnVal;
                    case DynamicOptionType.OptionalPage:
                        option = new OptionalColumnGroupPage(optionalColumnId);
                        break;
                    default:
                        return null;
                }

                returnVal = option.CreateGroupPage(null,
                                                   new GroupPageOptions(_groupPageDataProvider.PersonCollection)
                                                       {
                                                           CurrentGroupPageName =pageName
                                                       });
            }
            else
            {
                //If manual options selected.
                returnVal = new GroupPage(pageName);
            }

            return returnVal;
        }
    }
}