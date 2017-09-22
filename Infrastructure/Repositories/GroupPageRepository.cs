using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;

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
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

		public GroupPageRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
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
            var retList = CollectionHelper.ToDistinctGenericCollection<IGroupPage>(mainQuery[0]).ToArray();
            
            return retList;

        }

		public IList<IGroupPage> LoadGroupPagesByIds(IEnumerable<Guid> groupPageIds)
		{
			ICriteria groupPages = Session.CreateCriteria(typeof (GroupPage))
				.SetFetchMode("RootGroupCollection", FetchMode.Join)
				.Add(Restrictions.In("Id", groupPageIds.ToArray()));

			ICriteria rootGroupPages = Session.CreateCriteria(typeof(RootPersonGroup))
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
			var retList = CollectionHelper.ToDistinctGenericCollection<IGroupPage>(mainQuery[0]).ToArray();

			return retList;
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
				var retList = CollectionHelper.ToDistinctGenericCollection<IGroupPage>(mainQuery[0]).ToArray();

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
                
                return retList;
            }
            catch (SqlException sqlException)
            {
                throw new DataSourceException(sqlException.Message, sqlException);
            }
        }

	    public IList<IGroupPage> GetGroupPagesForPerson(Guid personId)
	    {
		    var result = new List<IGroupPage>();
			var childGroupQuery = DetachedCriteria.For<ChildPersonGroup>()
				.SetFetchMode("Parent", FetchMode.Join)
				.CreateCriteria("PersonCollection", JoinType.InnerJoin)
				.Add(Restrictions.Where<Person>(x => x.Id == personId));

		    var rootGroupQuery = DetachedCriteria.For<RootPersonGroup>()
				.SetFetchMode("Parent", FetchMode.Join)
			    .CreateCriteria("PersonCollection", JoinType.InnerJoin)
			    .Add(Restrictions.Where<Person>(x => x.Id == personId));

			var mainQuery = Session.CreateMultiCriteria()
			    .Add(childGroupQuery)
			    .Add(rootGroupQuery)
			    .List();

			var childGroups = CollectionHelper.ToDistinctGenericCollection<ChildPersonGroup>(mainQuery[0]).ToArray();
			var rootGroups = CollectionHelper.ToDistinctGenericCollection<RootPersonGroup>(mainQuery[1]).ToArray();

			// Go through the recursive hierarchy until we find a parent that is not a persongroupbase, at which point we are at the grouppage level.
			foreach (var childGroup in childGroups)
		    {
			    PersonGroupBase cg = childGroup;
			    while (cg.Parent is PersonGroupBase)
			    {
				    cg = (PersonGroupBase)cg.Parent;
			    }
				result.Add((IGroupPage)cg.Parent);
		    }

		    result.AddRange(rootGroups.Select(rootGroup => (IGroupPage) rootGroup.Parent));

		    return CollectionHelper.ToDistinctGenericCollection<IGroupPage>(result).Where(x=> !((IDeleteTag)x).IsDeleted).ToArray();
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
