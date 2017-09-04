using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Grouping
{
    /// <summary>
    /// Class for OptionalColumnGroupPage 
    /// </summary>
    /// <remarks>
    /// Created by: Muhamad Risath
    /// Created date: 2008-07-31
    /// </remarks>
    public class OptionalColumnGroupPage : IGroupPageCreator<IPerson>
    {
        private readonly Guid? _optionalColumnId;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionalColumnGroupPage"/> class.
        /// </summary>
        /// <param name="optionalColumnId">Id of the optional column.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-31
        /// </remarks>
        public OptionalColumnGroupPage(Guid? optionalColumnId)
        {
            _optionalColumnId = optionalColumnId;
        }

        public IGroupPage CreateGroupPage(IEnumerable<IPerson> entityCollection, IGroupPageOptions groupPageOptions)
        {
            if (groupPageOptions == null) throw new ArgumentNullException("groupPageOptions");
            IGroupPage groupPage = null;
			var cultureInfo = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;

            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var optionalColumnRepository = new OptionalColumnRepository(uow);
                IList<IOptionalColumn> optionalColumnCollection = optionalColumnRepository.GetOptionalColumns<Person>();

                if (optionalColumnCollection != null)
                {
                    foreach (IOptionalColumn column in optionalColumnCollection)
                    {
                        if (_optionalColumnId == column.Id)
                        {
                        	var optionalColumnGroups = optionalColumnRepository.UniqueValuesOnColumn(column.Id.Value);
							 
                            //Creates the GroupPage object
                            groupPage = new GroupPage(groupPageOptions.CurrentGroupPageName);

                            foreach (var optionalColumnGroup in optionalColumnGroups)
                            {
								
                                //Creates a root Group object & add into GroupPage
                                string groupName = optionalColumnGroup.Description.Length > 100
                                                       ? optionalColumnGroup.Description.Substring(0, 98) + ".."
                                                       : optionalColumnGroup.Description;
                                IRootPersonGroup rootGroup = new RootPersonGroup(groupName);

	                            var trimmedDescription = optionalColumnGroup.Description.Trim().ToLower(cultureInfo);

								foreach (var person in groupPageOptions.Persons)
								{
									var val = person.GetColumnValue(column);
									if (val == null) continue;

									if(val.Description.Trim().ToLower(cultureInfo).Equals(trimmedDescription))
										rootGroup.AddPerson(person);
								}
                                
                                //Add into GroupPage
                                if(rootGroup.PersonCollection.Count > 0)
                                    groupPage.AddRootPersonGroup(rootGroup);
                            }
                        }
                    }
                }
            }

            return groupPage;
        }
    }
}