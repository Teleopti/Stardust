using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Multi;
using NHibernate.SqlCommand;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

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
			: base(currentUnitOfWork, null, null)
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
                            .Fetch("RootGroupCollection")
                            .AddOrder(Order.Asc("Description"));

            ICriteria rootGroupPages = Session.CreateCriteria(typeof (RootPersonGroup))
                                        .Fetch("ChildGroupCollection");

            ICriteria personColection = Session.CreateCriteria(typeof(RootPersonGroup))
                                        .Fetch("PersonCollection");

            ICriteria childGroupPages = Session.CreateCriteria(typeof(ChildPersonGroup))
                                        .Fetch("ChildGroupCollection");

            ICriteria childPersonColection = Session.CreateCriteria(typeof(ChildPersonGroup))
                                        .Fetch("PersonCollection");
            
            return CollectionHelper.ToDistinctGenericCollection<IGroupPage>(Session.CreateQueryBatch()
                                        .Add<GroupPage>(groupPages)
                                        .Add<RootPersonGroup>(rootGroupPages)
                                        .Add<RootPersonGroup>(personColection)
                                        .Add<ChildPersonGroup>(childGroupPages)
                                        .Add<ChildPersonGroup>(childPersonColection)
                                        .GetResult<GroupPage>(0)).ToList();
        }

		public IList<IGroupPage> LoadGroupPagesByIds(IEnumerable<Guid> groupPageIds)
		{
			ICriteria groupPages = Session.CreateCriteria(typeof (GroupPage))
				.Fetch("RootGroupCollection")
				.Add(Restrictions.In("Id", groupPageIds.ToArray()));

			ICriteria rootGroupPages = Session.CreateCriteria(typeof(RootPersonGroup))
										.Fetch("ChildGroupCollection");

			ICriteria personColection = Session.CreateCriteria(typeof(RootPersonGroup))
										.Fetch("PersonCollection");

			ICriteria childGroupPages = Session.CreateCriteria(typeof(ChildPersonGroup))
										.Fetch("ChildGroupCollection");

			ICriteria childPersonColection = Session.CreateCriteria(typeof(ChildPersonGroup))
										.Fetch("PersonCollection");

			return CollectionHelper.ToDistinctGenericCollection<IGroupPage>(Session.CreateQueryBatch()
										.Add<GroupPage>(groupPages)
										.Add<RootPersonGroup>(rootGroupPages)
										.Add<RootPersonGroup>(personColection)
										.Add<ChildPersonGroup>(childGroupPages)
										.Add<ChildPersonGroup>(childPersonColection)
										.GetResult<GroupPage>(0)).ToList();
		}

		public IList<IGroupPage> LoadAllGroupPageWhenPersonCollectionReAssociated()
		{
            try
            {
                ICriteria groupPages = Session.CreateCriteria(typeof (GroupPage))
                    .Fetch("RootGroupCollection")
                    .AddOrder(Order.Asc("Description"));

                ICriteria rootGroupPages = Session.CreateCriteria(typeof (RootPersonGroup))
                    .Fetch("ChildGroupCollection");

                ICriteria childGroupPages = Session.CreateCriteria(typeof (ChildPersonGroup))
                    .Fetch("ChildGroupCollection");

                ICriteria persons = Session.CreateCriteria<RootPersonGroup>()
                    .Fetch("PersonCollection");

				var retList = CollectionHelper.ToDistinctGenericCollection<IGroupPage>(Session.CreateQueryBatch()
                    .Add<GroupPage>(groupPages)
                    .Add<RootPersonGroup>(rootGroupPages)
                    .Add<ChildPersonGroup>(childGroupPages)
                    .Add<RootPersonGroup>(persons)
                    .GetResult<GroupPage>(0)).ToList();

                var multiCriteria = Session.CreateQueryBatch();
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
                if(hasQuery) multiCriteria.Execute();;
                
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
				.Fetch("Parent")
				.CreateCriteria("PersonCollection", JoinType.InnerJoin)
				.Add(Restrictions.Where<Person>(x => x.Id == personId));

		    var rootGroupQuery = DetachedCriteria.For<RootPersonGroup>()
				.Fetch("Parent")
			    .CreateCriteria("PersonCollection", JoinType.InnerJoin)
			    .Add(Restrictions.Where<Person>(x => x.Id == personId));

			var mainQuery = Session.CreateQueryBatch()
			    .Add<ChildPersonGroup>(childGroupQuery)
			    .Add<RootPersonGroup>(rootGroupQuery);

			var childGroups = CollectionHelper.ToDistinctGenericCollection<ChildPersonGroup>(mainQuery.GetResult<ChildPersonGroup>(0));
			var rootGroups = CollectionHelper.ToDistinctGenericCollection<RootPersonGroup>(mainQuery.GetResult<RootPersonGroup>(1)).ToArray();

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

		private void initializeChildGroups(IChildPersonGroup childPersonGroup, IQueryBatch multiCriteria)
        {
            foreach (IChildPersonGroup choldChildGroup in childPersonGroup.ChildGroupCollection)
            {
                initializeChildGroups(choldChildGroup, multiCriteria);
            }
            multiCriteria.Add<ChildPersonGroup>(Session.CreateCriteria<ChildPersonGroup>()
                                  .Add(Restrictions.Eq("Id", childPersonGroup.Id))
                                  .Fetch("PersonCollection"));
        }
    }
}
