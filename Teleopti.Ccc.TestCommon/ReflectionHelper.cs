using System;
using System.Reflection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon
{
    /// <summary>
    /// Helper for reflection
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// Checks for private or protected default constructor.
        /// (Default: Private or protected only)
        /// </summary>
        /// <param name="myType">My type.</param>
        /// <returns></returns>
        public static bool HasDefaultConstructor(Type myType)
        {
            return HasDefaultConstructor(myType, false);
        }

        /// <summary>
        /// Determines whether [has default constructor] [the specified my type].
        /// </summary>
        /// <param name="myType">My type.</param>
        /// <param name="includePublic">if set to <c>true</c> [include public].</param>
        /// <returns>
        /// 	<c>true</c> if [has default constructor] [the specified my type]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-29
        /// </remarks>
        public static bool HasDefaultConstructor(Type myType, bool includePublic)
        {
            Type[] types = new Type[0];
            bool retval = false;

            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            if (includePublic) flags |= BindingFlags.Public;
            ConstructorInfo constructorInfoObj = myType.GetConstructor(
                flags, null,
                CallingConventions.HasThis, types, null);
            if (constructorInfoObj != null)
            {
                object tempObj = constructorInfoObj.Invoke(null);
                if (tempObj != null)
                    retval = true;
            }

            return retval;
        }

        public static void SetUpdatedOn(IChangeInfo aggregateRoot, DateTime date)
        {
            typeof(AggregateRoot_Events_ChangeInfo).GetField("_updatedOn", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(
                aggregateRoot, date);
        }

        public static void SetCreatedOn(ICreateInfo aggregateRoot, DateTime date)
        {
					aggregateRoot.GetType().GetProperty("CreatedOn", BindingFlags.Instance | BindingFlags.Public).SetValue(aggregateRoot, date, null);
        }
        public static void SetUpdatedBy(IChangeInfo aggregateRoot, IPerson person)
        {
            typeof(AggregateRoot_Events_ChangeInfo).GetField("_updatedBy", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(
                aggregateRoot, person);
        }

        public static void SetCreatedBy(ICreateInfo aggregateRoot, IPerson person)
        {
					aggregateRoot.GetType().GetProperty("CreatedBy", BindingFlags.Instance | BindingFlags.Public).SetValue(aggregateRoot, person, null);
        }
    }
}