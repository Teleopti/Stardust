using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.Globalization;
using Teleopti.Ccc.Domain.Security.ActiveDirectory;

namespace Teleopti.Ccc.Infrastructure.ActiveDirectory
{
    /// <summary>
    /// ActiveDirectoryGroup repository functions
    /// </summary>
    public class ActiveDirectoryGroupRepository :
        ActiveDirectoryRepository,
        IActiveDirectoryGroupRepository
    {

        #region Variables

        #endregion

        #region Constants

        /// <summary>
        /// Default Paging Size for the Search Methods.
        /// </summary>
        private const int DefaultPageSize = 2000;
        /// <summary>
        /// The Ranged PageSize Attribute. (Win2000 = 1000, Win2003 = 1500)
        /// </summary>
        private const int RangedPageSize = 1500;    // Win2003
        //private const int RangedPageSize = 1000;  // Win2000

        #endregion

        #region Interface

        /// <summary>
        /// Search for a Group Object from Active Directory.
        /// </summary>
        /// <param name="key">The Property Key.</param>
        /// <param name="value">The Property Value.</param>
        /// <returns>A Group Object.</returns>
        public ActiveDirectoryGroup FindGroup(string key, string value)
        {
            SearchResult Result;
            ActiveDirectoryGroup group = null;
            List<string> PropertiesToLoad = ActiveDirectoryGroupMapper.AllGroupProperties();

            using (DirectorySearcher Searcher = new DirectorySearcher())
            {
                Searcher.Filter = string.Format(CultureInfo.InvariantCulture, "(&(objectClass=group)({0}={1}))", key, value);
                Searcher.PageSize = DefaultPageSize;

                foreach (string Property in PropertiesToLoad)
                    Searcher.PropertiesToLoad.Add(Property);

                Result = Searcher.FindOne();
            }

            if (Result != null)
            {
                group = new ActiveDirectoryGroup();
                PopulateFields(Result.Properties, group);
            }

            return group;
        }

        /// <summary>
        /// Search for a List of Group Objects from Active Directory.
        /// </summary>
        /// <param name="key">The Property Key.</param>
        /// <param name="value">The Property Value.</param>
        /// <returns>A Group Object.</returns>
        public Collection<ActiveDirectoryGroup> FindGroups(string key, string value)
        {
            SearchResultCollection Results;
            Collection<ActiveDirectoryGroup> Groups = new Collection<ActiveDirectoryGroup>();
            List<string> PropertiesToLoad = ActiveDirectoryGroupMapper.AllGroupProperties();
            using (IDirectorySearcherChannel Searcher = CreateNewDirectorySearcher())
            {
                Searcher.Filter = string.Format(CultureInfo.InvariantCulture, "(&(objectClass=group)({0}={1}))", key, value);
                Searcher.PageSize = DefaultPageSize;

                foreach (string Property in PropertiesToLoad)
                    Searcher.PropertiesToLoad.Add(Property);

                Results = Searcher.FindAll();
            }

            foreach (SearchResult Result in Results)
            {
                ActiveDirectoryGroup group = new ActiveDirectoryGroup();
                PopulateFields(Result.Properties, group);
                Groups.Add(group);
            }
                

            return Groups;
        }

        /// <summary>
        /// Search for a List of Group Objects from Active Directory.
        /// </summary>
        /// <param name="filter">The Search Result Filter. [null for default]</param>
        /// <param name="searchRoot">The Search Root. [null for default]</param>
        /// <param name="sizeLimit">The Return Limit. [0 for no limit]</param>
        /// <param name="sort">The sort.</param>
        /// <returns>A List of User Objects.</returns>
        public Collection<ActiveDirectoryGroup> FindGroups(string filter, DirectoryEntry searchRoot, int sizeLimit, SortOption sort)
        {
            SearchResultCollection Results;
            Collection<ActiveDirectoryGroup> groups = new Collection<ActiveDirectoryGroup>();
            List<string> PropertiesToLoad = ActiveDirectoryUserMapper.AllUserProperties();

            if (string.IsNullOrEmpty(filter))
                filter = "(&(objectClass=group)(!(objectClass=computer)))";

            if (sort == null)
                sort = new SortOption(ActiveDirectoryGroupMapper.COMMONNAME, SortDirection.Ascending);

            using (IDirectorySearcherChannel Searcher = CreateNewDirectorySearcher())
            {
                Searcher.Filter = filter;
                Searcher.Sort = sort;
                Searcher.PageSize = DefaultPageSize;

                if (searchRoot != null)
                    Searcher.SearchRoot = searchRoot;

                if (sizeLimit > 0)
                    Searcher.SizeLimit = sizeLimit;

                foreach (string Property in PropertiesToLoad)
                    Searcher.PropertiesToLoad.Add(Property);

                Results = Searcher.FindAll();
            }

            foreach (SearchResult Result in Results)
            {
                ActiveDirectoryGroup group = new ActiveDirectoryGroup();
                PopulateFields(Result.Properties, group);
                groups.Add(group);            
            }

            return groups;
        }

        #endregion

        #region Local methods

        /// <summary>
        /// Retrieve Ranged Values.
        /// </summary>
        private void GetRangedValues(ActiveDirectoryGroup groupToFill)
        {
            int PageSize = RangedPageSize;
            int Page = 0;
            int StartRecord = 0;
            int RecordCount = 0;

            using (DirectoryEntry directoryEntry = FindDirectoryEntry(ActiveDirectoryGroupMapper.OBJECTSID, groupToFill.ObjectSIDString))
            {
                Page = 0;
                do
                {
                    RecordCount = 0;
                    StartRecord = (Page * PageSize);

                    directoryEntry.RefreshCache(new string[] { ActiveDirectoryGroupMapper.MEMBER + ";range=" + StartRecord + "-*" });

                    if (directoryEntry.Properties[ActiveDirectoryGroupMapper.MEMBER] != null)
                    {
                        RecordCount = directoryEntry.Properties[ActiveDirectoryGroupMapper.MEMBER].Count;

                        foreach (string _Member in directoryEntry.Properties[ActiveDirectoryGroupMapper.MEMBER])
                            groupToFill.MemberActions.Add(_Member, MemberAction.None);
                    }

                    Page++;
                }
                while (RecordCount == PageSize);

                Page = 0;
                do
                {
                    RecordCount = 0;
                    StartRecord = (Page * PageSize);

                    directoryEntry.RefreshCache(new string[] { ActiveDirectoryGroupMapper.MEMBEROF + ";range=" + StartRecord + "-*" });

                    if (directoryEntry.Properties[ActiveDirectoryGroupMapper.MEMBEROF] != null)
                    {
                        RecordCount = directoryEntry.Properties[ActiveDirectoryGroupMapper.MEMBEROF].Count;

                        foreach (string _MemberOf in directoryEntry.Properties[ActiveDirectoryGroupMapper.MEMBEROF])
                            groupToFill.MemberOf.Add(_MemberOf);
                    }

                    Page++;
                }
                while (RecordCount == PageSize);
            }

            groupToFill.MemberOf.Sort();
        }

        /// <summary>
        /// Populates the internal fields using a SearchResult Property Collection.
        /// </summary>
        /// <param name="Collection">A SearchResult Property Collection.</param>
        /// <param name="groupToFill">The group to fill.</param>
        private void PopulateFields(ResultPropertyCollection Collection, ActiveDirectoryGroup groupToFill)
        {
            if (Collection.Contains(ActiveDirectoryGroupMapper.CHANGED))
                groupToFill.Changed = Convert.ToDateTime(Collection[ActiveDirectoryGroupMapper.CHANGED][0], CultureInfo.CurrentCulture).ToLocalTime();

            if (Collection.Contains(ActiveDirectoryGroupMapper.COMMONNAME))
                groupToFill.CommonName = Collection[ActiveDirectoryGroupMapper.COMMONNAME][0] as string;

            if (Collection.Contains(ActiveDirectoryGroupMapper.CREATED))
                groupToFill.Created = Convert.ToDateTime(Collection[ActiveDirectoryGroupMapper.CREATED][0], CultureInfo.CurrentCulture).ToLocalTime();

            if (Collection.Contains(ActiveDirectoryGroupMapper.DISTINGUISHEDNAME))
                groupToFill.DistinguishedName = Collection[ActiveDirectoryGroupMapper.DISTINGUISHEDNAME][0] as string;

            if (Collection.Contains(ActiveDirectoryGroupMapper.OBJECTGUID))
            {
                groupToFill.ObjectGUID = Collection[ActiveDirectoryGroupMapper.OBJECTGUID][0] as byte[];
            }

            if (Collection.Contains(ActiveDirectoryGroupMapper.OBJECTSID))
            {
                groupToFill.ObjectSID = Collection[ActiveDirectoryGroupMapper.OBJECTSID][0] as byte[];
            }

            if (Collection.Contains(ActiveDirectoryGroupMapper.SAMACCOUNTNAME))
                groupToFill.SAMAccountName = Collection[ActiveDirectoryGroupMapper.SAMACCOUNTNAME][0] as string;

            GetRangedValues(groupToFill);
        }

        #endregion

    }
}
