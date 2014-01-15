using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Teleopti.Ccc.Domain.Collection
{
    /// <summary>
    /// Wrapper for Bindinglist, used to solve Microsoft databinding issue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TypedBindingCollection<T> : BindingList<T>, ITypedList
    {

        #region ITypedList Implementation

        /// <summary>
        /// Returns the <see cref="T:System.ComponentModel.PropertyDescriptorCollection"/> that represents the properties on each item used to bind data.
        /// </summary>
        /// <param name="listAccessors">An array of <see cref="T:System.ComponentModel.PropertyDescriptor"/> objects to find in the collection as bindable. This can be null.</param>
        /// <returns>
        /// The <see cref="T:System.ComponentModel.PropertyDescriptorCollection"/> that represents the properties on each item used to bind data.
        /// </returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-06-10
        /// </remarks>
        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            if (listAccessors == null || listAccessors.Length <= 0)
            {
                // return the properties of items on
                // this list
                return GetTypeProperties(typeof(T));
            }
            else
            {
                // return the properties of the specified member
                // this is needed when the list is used in master/detail
                // structures to ensure the child grid can figure out
                // the types contained herein.
                string memberName = listAccessors[0].Name;
                PropertyInfo pinfo = typeof(T).GetProperty(memberName);
                if (pinfo != null)
                {
                    Type type = pinfo.PropertyType;
                    if (typeof(IList).IsAssignableFrom(type) && type.IsGenericType)
                    {
                        // if it is a generic list, find the first generic type and
                        // assume that's the type of the list contents
                        // a hack, but it could be worse :)
                        Type paramType = type.GetGenericArguments()[0];
                        return GetTypeProperties(paramType);
                    }
                    return GetTypeProperties(type);
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the name of the list.
        /// </summary>
        /// <param name="listAccessors">An array of <see cref="T:System.ComponentModel.PropertyDescriptor"/> objects, for which the list name is returned. This can be null.</param>
        /// <returns>The name of the list.</returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-06-10
        /// </remarks>
        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            return string.Format(CultureInfo.CurrentCulture, "List of {0}", typeof(T).Name);
        }

        #endregion // ITypedList Implementation

        private PropertyDescriptorCollection GetTypeProperties(Type type)
        {
            return GetAllProperties(type);
        }

        /// <summary>
        /// This is a fix for making the databinding work with Interfaces, 
        /// the method: provider.GetTypeDescriptor(type).GetProperties(attributes) only worked
        /// for class hiercheys on real classes (not interfaces) didnt take the base interface properties
        /// 
        /// This method Gets all the properties from baseclasses and baseinterfaces along with the inherited
        /// 
        /// Yes, Interface inheritance works different from class inheritence in reflecton ?!?!? 
        /// 
        /// /Peter
        /// 
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 4/23/2009
        /// </remarks>
        public PropertyDescriptorCollection GetAllProperties(Type type)
        {
            TypeDescriptionProvider provider = TypeDescriptor.GetProvider(type);
            PropertyDescriptorCollection props = provider.GetTypeDescriptor(type).GetProperties();

            PropertyDescriptorCollection newProps = new PropertyDescriptorCollection(null);

            Type[] inheritedInterfaces = type.GetInterfaces();
            foreach (Type intType in inheritedInterfaces)
            {
                PropertyDescriptorCollection inheritedProprs =
                    TypeDescriptor.GetProperties(intType);

                foreach (PropertyDescriptor p in inheritedProprs)
                    newProps.Add(p);
            }

            foreach (PropertyDescriptor p in props)
                newProps.Add(p);

            PropertyDescriptorCollection properties = newProps;

            return properties;
        }
    }
}