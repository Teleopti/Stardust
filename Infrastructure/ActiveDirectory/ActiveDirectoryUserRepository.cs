using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.Globalization;
using Teleopti.Ccc.Domain.Security.ActiveDirectory;

namespace Teleopti.Ccc.Infrastructure.ActiveDirectory
{
    /// <summary>
    /// ActiveDirectoryUser repository functions.
    /// </summary>
    public class ActiveDirectoryUserRepository :
        ActiveDirectoryRepository,
        IActiveDirectoryUserRepository
    {

        #region Variables

        /// <summary>
        /// Default Paging Size for the Search Methods.
        /// </summary>
        private const int DefaultPageSize = 2000;

        #endregion

        #region Interface

        /// <summary>
        /// Gets a user's Recursive Membership Groups by the specified Object SID String.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The Collection of Token Groups.</returns>
        public Collection<ActiveDirectoryGroup> FindTokenGroups(ActiveDirectoryUser user)
        {
            Collection<ActiveDirectoryGroup> TokenGroups = new Collection<ActiveDirectoryGroup>();

            using (DirectoryEntry deUser = FindDirectoryEntry(ActiveDirectoryUserMapper.OBJECTSID, user.ObjectSIDString))
            {
                deUser.RefreshCache(new string[] { ActiveDirectoryUserMapper.TOKENGROUPS });

                if (deUser.Properties.Contains(ActiveDirectoryUserMapper.TOKENGROUPS))
                {
                    if (deUser.Properties[ActiveDirectoryUserMapper.TOKENGROUPS] != null)
                    {
                        foreach (byte[] GroupSID in deUser.Properties[ActiveDirectoryUserMapper.TOKENGROUPS])
                        {
                            string sGroupSID = ActiveDirectoryHelper.ConvertBytesToStringSid(GroupSID);
                            ActiveDirectoryGroupRepository groupRepository = new ActiveDirectoryGroupRepository();
                            ActiveDirectoryGroup groupSearch = groupRepository.FindGroup(ActiveDirectoryGroupMapper.OBJECTSID, sGroupSID);
                            if (groupSearch != null)
                                TokenGroups.Add(groupSearch);
                        }
                    }
                }
            }

            return TokenGroups;
        }

        /// <summary>
        /// Search for a User Object from Active Directory.
        /// </summary>
        /// <param name="key">The Property Key.</param>
        /// <param name="value">The Property Value.</param>
        /// <returns>A User Object.</returns>
        public ActiveDirectoryUser FindUser(string key, string value)
        {
            SearchResult Result;
            ActiveDirectoryUser _User = null;
            List<string> PropertiesToLoad = ActiveDirectoryUserMapper.AllUserProperties();

            using (IDirectorySearcherChannel Searcher = CreateNewDirectorySearcher())
            {
                Searcher.Filter = "(&(&(objectClass=user)(!(objectClass=computer)))(" + key + "=" + value + "))";

                Searcher.PageSize = DefaultPageSize;

                foreach (string Property in PropertiesToLoad)
                    Searcher.PropertiesToLoad.Add(Property);

                Result = Searcher.FindOne();
            }

            if (Result != null)
            {
                _User = new ActiveDirectoryUser();
                PopulateUser(Result.Properties, _User, true);
            }


            return _User;
        }

        /// <summary>
        /// Search for a List of User Objects from Active Directory.
        /// </summary>
        /// <param name="key">The Property Key.</param>
        /// <param name="value">The Property Value. (use * for wildcard)</param>
        /// <returns>A List of User Objects.</returns>
        public Collection<ActiveDirectoryUser> FindUsers(string key, string value)
        {
            SearchResultCollection Results;
            Collection<ActiveDirectoryUser> Users = new Collection<ActiveDirectoryUser>();
            List<string> PropertiesToLoad = ActiveDirectoryUserMapper.AllUserProperties();

            using (IDirectorySearcherChannel Searcher = CreateNewDirectorySearcher())
            {
                Searcher.Filter = string.Format(CultureInfo.InvariantCulture, "(&(&(objectClass=user)(!(objectClass=computer)))({0}={1}))", key, value);
                Searcher.PageSize = DefaultPageSize;

                foreach (string Property in PropertiesToLoad)
                    Searcher.PropertiesToLoad.Add(Property);

                Results = Searcher.FindAll();
            }

            foreach (SearchResult Result in Results)
            {
                ActiveDirectoryUser newUser = new ActiveDirectoryUser();
                PopulateUser(Result.Properties, newUser, false);
                Users.Add(newUser);
            }
            return Users;
        }

        /// <summary>
        /// Search for a List of User Objects from Active Directory.
        /// </summary>
        /// <param name="filter">The Search Result Filter. [null for default]</param>
        /// <param name="searchRoot">The Search Root. [null for default]</param>
        /// <param name="sizeLimit">The Return Limit. [0 for no limit]</param>
        /// <param name="sort">The sort.</param>
        /// <returns>A List of User Objects.</returns>
        public Collection<ActiveDirectoryUser> FindUsers(string filter, DirectoryEntry searchRoot, int sizeLimit, SortOption sort)
        {
            SearchResultCollection Results;
            Collection<ActiveDirectoryUser> Users = new Collection<ActiveDirectoryUser>();
            List<string> PropertiesToLoad = ActiveDirectoryUserMapper.AllUserProperties();

            if (string.IsNullOrEmpty(filter))
                filter = "(&(objectClass=user)(!(objectClass=computer)))";

            if (sort == null)
                sort = new SortOption(ActiveDirectoryUserMapper.SAMACCOUNTNAME, SortDirection.Ascending);

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
                ActiveDirectoryUser newUser = new ActiveDirectoryUser();
                PopulateUser(Result.Properties, newUser, false);
                Users.Add(newUser);
            }

            return Users;
        }

        #endregion

        #region Local methods

        /// <summary>
        /// Populates the internal fields using a SearchResult Property Collection.
        /// </summary>
        private void PopulateUser(ResultPropertyCollection Collection, ActiveDirectoryUser userToPopulate, bool loadGroups)
        {
            if (Collection.Contains(ActiveDirectoryUserMapper.ACCOUNTCONTROL))
                userToPopulate.AccountControl = (int?)Collection[ActiveDirectoryUserMapper.ACCOUNTCONTROL][0] ?? 0;

            if (Collection.Contains(ActiveDirectoryUserMapper.ASSISTANT))
                userToPopulate.Assistant = Collection[ActiveDirectoryUserMapper.ASSISTANT][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.CELLPHONE))
                userToPopulate.CellPhone = Collection[ActiveDirectoryUserMapper.CELLPHONE][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.CHANGED))
                userToPopulate.Changed = Convert.ToDateTime(Collection[ActiveDirectoryUserMapper.CHANGED][0], CultureInfo.CurrentCulture).ToLocalTime();

            if (Collection.Contains(ActiveDirectoryUserMapper.CITY))
                userToPopulate.City = Collection[ActiveDirectoryUserMapper.CITY][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.COMMONNAME))
                userToPopulate.CommonName = Collection[ActiveDirectoryUserMapper.COMMONNAME][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.COMPANY))
                userToPopulate.Company = Collection[ActiveDirectoryUserMapper.COMPANY][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.CREATED))
                userToPopulate.Created = Convert.ToDateTime(Collection[ActiveDirectoryUserMapper.CREATED][0], CultureInfo.CurrentCulture).ToLocalTime();

            if (Collection.Contains(ActiveDirectoryUserMapper.DEPARTMENT))
                userToPopulate.Department = Collection[ActiveDirectoryUserMapper.DEPARTMENT][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.DESCRIPTION))
                userToPopulate.Description = Collection[ActiveDirectoryUserMapper.DESCRIPTION][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.DISTINGUISHEDNAME))
            {
                userToPopulate.DistinguishedName = Collection[ActiveDirectoryUserMapper.DISTINGUISHEDNAME][0] as string;
            }

            if (Collection.Contains(ActiveDirectoryUserMapper.EMAILADDRESS))
                userToPopulate.EmailAddress = Collection[ActiveDirectoryUserMapper.EMAILADDRESS][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.EMPLOYEEID))
                userToPopulate.EmployeeID = Collection[ActiveDirectoryUserMapper.EMPLOYEEID][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.FAXNUMBER))
                userToPopulate.FaxNumber = Collection[ActiveDirectoryUserMapper.FAXNUMBER][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.FIRSTNAME))
                userToPopulate.FirstName = Collection[ActiveDirectoryUserMapper.FIRSTNAME][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.FULLDISPLAYNAME))
                userToPopulate.FullDisplayName = Collection[ActiveDirectoryUserMapper.FULLDISPLAYNAME][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.HOMEDIRECTORY))
                userToPopulate.HomeDirectory = Collection[ActiveDirectoryUserMapper.HOMEDIRECTORY][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.HOMEDRIVE))
                userToPopulate.HomeDrive = Collection[ActiveDirectoryUserMapper.HOMEDRIVE][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.HOMEPHONE))
                userToPopulate.HomePhone = Collection[ActiveDirectoryUserMapper.HOMEPHONE][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.LASTNAME))
                userToPopulate.LastName = Collection[ActiveDirectoryUserMapper.LASTNAME][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.LOGONCOUNT))
                userToPopulate.LogonCount = (int?)Collection[ActiveDirectoryUserMapper.LOGONCOUNT][0] ?? -1;

            if (Collection.Contains(ActiveDirectoryUserMapper.MEMBEROF))
            {
                foreach (string Value in Collection[ActiveDirectoryUserMapper.MEMBEROF])
                {
                    int Index = Value.IndexOf("=", 1, StringComparison.OrdinalIgnoreCase) + 1;
                    int Length = Value.IndexOf(",", 1, StringComparison.OrdinalIgnoreCase) - Index;
                    userToPopulate.MemberOf.Add(Value.Substring(Index, Length));
                }
            }

            if (Collection.Contains(ActiveDirectoryUserMapper.MIDDLEINITAL))
                userToPopulate.MiddleInitial = Collection[ActiveDirectoryUserMapper.MIDDLEINITAL][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.NOTES))
                userToPopulate.Notes = Collection[ActiveDirectoryUserMapper.NOTES][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.OBJECTGUID))
            {
                userToPopulate.ObjectGUID = Collection[ActiveDirectoryUserMapper.OBJECTGUID][0] as byte[];
            }

            if (Collection.Contains(ActiveDirectoryUserMapper.OBJECTSID))
            {
                userToPopulate.ObjectSID = Collection[ActiveDirectoryUserMapper.OBJECTSID][0] as byte[];
            }

            if (Collection.Contains(ActiveDirectoryUserMapper.OFFICE))
                userToPopulate.Office = Collection[ActiveDirectoryUserMapper.OFFICE][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.PASSWORDLASTSET))
                userToPopulate.PasswordLastSet = (Collection[ActiveDirectoryUserMapper.PASSWORDLASTSET][0].Equals(0) ? 0 : -1);

            if (Collection.Contains(ActiveDirectoryUserMapper.POBOX))
                userToPopulate.POBox = Collection[ActiveDirectoryUserMapper.POBOX][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.PROFILEPATH))
                userToPopulate.ProfilePath = Collection[ActiveDirectoryUserMapper.PROFILEPATH][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.SAMACCOUNTNAME))
                userToPopulate.SAMAccountName = Collection[ActiveDirectoryUserMapper.SAMACCOUNTNAME][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.SCRIPTPATH))
                userToPopulate.ScriptPath = Collection[ActiveDirectoryUserMapper.SCRIPTPATH][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.STATE))
                userToPopulate.State = Collection[ActiveDirectoryUserMapper.STATE][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.STREETADDRESS))
                userToPopulate.StreetAddress = Collection[ActiveDirectoryUserMapper.STREETADDRESS][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.USERPRINCIPALNAME))
                userToPopulate.UserPrincipalName = Collection[ActiveDirectoryUserMapper.USERPRINCIPALNAME][0] as string;

            if (Collection.Contains(ActiveDirectoryUserMapper.ZIPCODE))
                userToPopulate.ZipCode = Collection[ActiveDirectoryUserMapper.ZIPCODE][0] as string;

            if (loadGroups)
            {
                IList<ActiveDirectoryGroup> groups = FindTokenGroups(userToPopulate);
                foreach (ActiveDirectoryGroup group in groups)
                {
                    userToPopulate.TokenGroups.Add(group);
                }
            }
        }

        #endregion

    }
}
