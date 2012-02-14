using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationSteps
{
    /// <summary>
    /// Helper functions to AuthorizationEntity.
    /// </summary>
    public static class AuthorizationEntityExtender 
    {
        /// <summary>
        /// Converts to specific IAuthorization based list. Gives back an empty list even if the input is null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceList">The source list.</param>
        /// <returns></returns>
        public static IList<T> ConvertToSpecificList<T>(ICollection<IAuthorizationEntity> sourceList) where T : IAuthorizationEntity
        {
            IList<T> resultList = new List<T>();
            if (sourceList != null)
            {
                resultList = new List<T>(sourceList.OfType<T>());
            }
            return resultList;
        }

        /// <summary>
        /// Converts to general IAuthorizationEntity list. Gives back an empty list even if the input is null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceList">The source list.</param>
        /// <returns></returns>
        public static IList<IAuthorizationEntity> ConvertToBaseList<T>(ICollection<T> sourceList) where T : IAuthorizationEntity
        {
            IList<IAuthorizationEntity> resultList = new List<IAuthorizationEntity>();
            if (sourceList != null)
            {
                resultList = new List<IAuthorizationEntity>(sourceList.OfType<IAuthorizationEntity>());
            }
            return resultList;
        }

        /// <summary>
        /// Determines whether the collection contains an item whose key equals with the specified searchkey param.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="searchKey">The key.</param>
        /// <returns>
        /// 	<c>true</c> if the searchString param is member of the collection; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAnyAuthorizationKeyEquals(IEnumerable<IAuthorizationEntity> list, string searchKey)
        {
            foreach (IAuthorizationEntity entity in list)
            {
                if (entity.AuthorizationKey == searchKey)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether the collection contains an item whose name equals with the specified searchName param.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="searchKey">The key.</param>
        /// <returns>
        /// 	<c>true</c> if the searchString param is member of the collection; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAnyAuthorizationNameEquals(IEnumerable<IAuthorizationEntity> list, string searchKey)
        {
            foreach (IAuthorizationEntity entity in list)
            {
                if (entity.AuthorizationName == searchKey)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Determines whether the collection contains an item that starts with the specified searchName param.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="searchName">The key.</param>
        /// <returns>
        /// 	<c>true</c> if the collection contains an item whose name starts with the specified searchName; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAnyAuthorizationNameStartsWith(IEnumerable<IAuthorizationEntity> list, string searchName)
        {
            foreach (IAuthorizationEntity entity in list)
            {
                if (entity.AuthorizationName.StartsWith(searchName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether the collection contains an item whose name ends with the specified searchName param.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="searchName">Name of the search.</param>
        /// <returns>
        /// 	<c>true</c> if the collection contains an item that ends with the specified searchName; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAnyAuthorizationNameEndsWith(IEnumerable<IAuthorizationEntity> list, string searchName)
        {
            foreach (IAuthorizationEntity entity in list)
            {
                if (entity.AuthorizationName.EndsWith(searchName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether the collection contains an item that contains the specified searchKey param.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="searchKey">Name of the search.</param>
        /// <returns>
        /// 	<c>true</c> if the collection contains an item that contains the specified searchKey; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAnyAuthorizationNameContains(IEnumerable<IAuthorizationEntity> list, string searchKey)
        {
            foreach (IAuthorizationEntity entity in list)
            {
                if (entity.AuthorizationKey.Contains(searchKey))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Unions the two param lists.
        /// </summary>
        /// <param name="masterList">The master list.</param>
        /// <param name="servantList">The servant list.</param>
        /// <remarks>
        /// NOTE: The new items in the servant list will be added to the master list.
        /// </remarks>
        public static IList<T> UnionTwoLists<T>(IList<T> masterList, IEnumerable<T> servantList) where T : IAuthorizationEntity
        {
            foreach (T servantItem in servantList)
            {
                if (!IsEntityListContainsItem<T>(masterList, servantItem))
                {
                    masterList.Add(servantItem);
                }
            }
            return masterList;
        }

        /// <summary>
        /// Subtract the two param lists.
        /// </summary>
        /// <param name="masterList">The first list.</param>
        /// <param name="servantList">The second list.</param>
        /// <remarks>
        /// NOTE: The result list will be created from the first list which is a ref param.
        /// </remarks>
        public static IList<T> SubtractTwoLists<T>(IList<T> masterList, IEnumerable<T> servantList) where T : IAuthorizationEntity
        {
            for (int counter = masterList.Count - 1; counter >= 0; counter--)
            {
                T item = masterList[counter];
                if (!IsEntityListContainsItem<T>(servantList, item))
                {
                    masterList.Remove(item);
                }
            }
            return masterList;
        }

        /// <summary>
        /// Determines whether the param list contains the param item.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="item">The item.</param>
        /// <returns>
        /// 	<c>true</c> if is list contains the item; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2007-10-30
        /// </remarks>
        public static bool IsEntityListContainsItem<T>(IEnumerable<T> list, T item) where T : IAuthorizationEntity
        {
            foreach (T entity in list)
            {
                if (entity.AuthorizationKey == item.AuthorizationKey)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the list divided by the separation character param.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public static string ListToSqlString(IList<IAuthorizationEntity> list)
        {
            if (list != null && list.Count > 0)
                return ListToString(list, ",", "'", "'");
            return "''";
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the list divided by the separation character param.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="itemSeparator">The separation string between items.</param>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public static string ListToString(IList<IAuthorizationEntity> list, string itemSeparator)
        {
            return ListToString(list, itemSeparator, string.Empty, string.Empty);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the list divided by the separation character param.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="itemSeparator">The separation string between items.</param>
        /// <param name="itemPrefix">The prefix before each item.</param>
        /// <param name="itemSuffix">The suffix after each item.</param>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public static string ListToString(IList<IAuthorizationEntity> list, string itemSeparator, string itemPrefix, string itemSuffix)
        {
            if (list == null || list.Count == 0)
                return string.Empty;
            StringBuilder builder = new StringBuilder();
            foreach (IAuthorizationEntity entity in list)
            {
                builder.Append(itemPrefix);
                builder.Append(entity.AuthorizationKey);
                builder.Append(itemSuffix);
                builder.Append(itemSeparator);
            }
            if (builder.Length > 0)
                builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }

    }
}