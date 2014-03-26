using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Represents GroupPageRepository
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-06-23
    /// </remarks>
    public class GroupPageRepository : Repository<IGroupPage>, IGroupPageRepository
    {
        /// <summary>
        /// Initilaze a new instance of the class 
        /// </summary>
        /// <param name="unitOfWork"></param>
        public GroupPageRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        /// <summary>
        /// Loads the name of all group page by sorted by.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-25
        /// </remarks>
        public IList<IGroupPage> LoadAllGroupPageBySortedByDescription()
        {

            ICriteria groupPages = Session.CreateCriteria(typeof(GroupPage))
                            .SetFetchMode("RootGroupCollection", FetchMode.Join)
                            .AddOrder(Order.Asc("Description"));

            ICriteria rootGroupPages = Session.CreateCriteria(typeof (RootPersonGroup))
                                        .SetFetchMode("ChildGroupCollection", FetchMode.Join);

            ICriteria personColection = Session.CreateCriteria(typeof(RootPersonGroup))
                                        .SetFetchMode("PersonCollection", FetchMode.Join);

            ICriteria childGroupPages = Session.CreateCriteria(typeof(ChildPersonGroup))
                                        .SetFetchMode("ChildGroupCollection", FetchMode.Join);

            ICriteria childPersonColection = Session.CreateCriteria(typeof(ChildPersonGroup))
                                        .SetFetchMode("PersonCollection", FetchMode.Join);
            
            IList mainQuery = Session.CreateMultiCriteria()
                                        .Add(groupPages)
                                        .Add(rootGroupPages)
                                        .Add(personColection)
                                        .Add(childGroupPages)
                                        .Add(childPersonColection)
                                        .List();
            ICollection<IGroupPage> retList = CollectionHelper.ToDistinctGenericCollection<IGroupPage>(mainQuery[0]);
            
            return (List<IGroupPage>) retList;

        }

		public IList<IGroupPage> LoadAllGroupPageWhenPersonCollectionReAssociated()
		{
            try
            {
                ICriteria groupPages = Session.CreateCriteria(typeof (GroupPage))
                    .SetFetchMode("RootGroupCollection", FetchMode.Join)
                    .AddOrder(Order.Asc("Description"));

                ICriteria rootGroupPages = Session.CreateCriteria(typeof (RootPersonGroup))
                    .SetFetchMode("ChildGroupCollection", FetchMode.Join);

                ICriteria childGroupPages = Session.CreateCriteria(typeof (ChildPersonGroup))
                    .SetFetchMode("ChildGroupCollection", FetchMode.Join);

                ICriteria persons = Session.CreateCriteria<RootPersonGroup>()
                    .SetFetchMode("PersonCollection", FetchMode.Join);

                IList mainQuery = Session.CreateMultiCriteria()
                    .Add(groupPages)
                    .Add(rootGroupPages)
                    .Add(childGroupPages)
                    .Add(persons)
                    .List();
                ICollection<IGroupPage> retList = CollectionHelper.ToDistinctGenericCollection<IGroupPage>(mainQuery[0]);

                var multiCriteria = Session.CreateMultiCriteria();
                bool hasQuery = false;
                foreach (IGroupPage groupPage in retList)
                {
                    foreach (IRootPersonGroup rootPersonGroup in groupPage.RootGroupCollection)
                    {
                        foreach (IChildPersonGroup childPersonGroup in rootPersonGroup.ChildGroupCollection)
                        {
                            initializeChildGroups(childPersonGroup, multiCriteria);
                            hasQuery = true;
                        }
                    }
                }
                if(hasQuery) multiCriteria.List();
                
                return (List<IGroupPage>)retList;
            }
            catch (SqlException sqlException)
            {
                throw new DataSourceException(sqlException.Message, sqlException);
            }
        }

        private void initializeChildGroups(IChildPersonGroup childPersonGroup, IMultiCriteria multiCriteria)
        {
            foreach (IChildPersonGroup choldChildGroup in childPersonGroup.ChildGroupCollection)
            {
                initializeChildGroups(choldChildGroup, multiCriteria);
            }
            multiCriteria.Add(Session.CreateCriteria<ChildPersonGroup>()
                                  .Add(Restrictions.Eq("Id", childPersonGroup.Id))
                                  .SetFetchMode("PersonCollection", FetchMode.Join));
        }
    }
}
