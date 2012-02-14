using System;
using System.Linq;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Grouping
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

            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                OptionalColumnRepository optionalColumnRepository = new OptionalColumnRepository(uow);
                IList<IOptionalColumn> optionalColumnCollection = optionalColumnRepository.GetOptionalColumnValues<Person>();

                if (optionalColumnCollection != null)
                {
                    foreach (IOptionalColumn column in optionalColumnCollection)
                    {
                        if (_optionalColumnId == column.Id)
                        {
                            ICollection<IOptionalColumnValue> columnValueCollection = column.ValueCollection;

                            var optionalColumnGroups = from optionalColumn in columnValueCollection
                                                                                  where string.IsNullOrEmpty(optionalColumn.Description) == false
                            group optionalColumn by optionalColumn.Description;

                            //Creates the GroupPage object
                            groupPage = new GroupPage(groupPageOptions.CurrentGroupPageName);

                            foreach (var optionalColumnGroup in optionalColumnGroups)
                            {
                                //Creates a root Group object & add into GroupPage
                                string groupName = optionalColumnGroup.Key.Length > 50
                                                       ? optionalColumnGroup.Key.Substring(0, 48) + ".."
                                                       : optionalColumnGroup.Key;
                                IRootPersonGroup rootGroup = new RootPersonGroup(groupName);
                                foreach (var optionalColumn in optionalColumnGroup)
                                {
                                    Guid? id = optionalColumn.ReferenceId;
                                    IPerson person = groupPageOptions.Persons.FirstOrDefault(p => p.Id == id);
                                    if (person != null)
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