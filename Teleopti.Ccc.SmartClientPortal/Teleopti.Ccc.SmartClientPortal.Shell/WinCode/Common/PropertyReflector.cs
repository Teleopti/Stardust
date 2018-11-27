using System;
using System.Collections.Generic;
using System.Reflection;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    /// <summary> 
    /// Helps to get and set property values on objects through reflection. 
    /// Properties of underlying objects can be accessed directly by separating 
    /// the levels in the hierarchy by dots. 
    /// To get/set the name of an Ancestor, for objects that have a Parent property, 
    /// you could use "Parent.Parent.Parent.Name". 
    /// </summary> 
    public class PropertyReflector
    {
        private const char PropertyNameSeparator = '.';

        private static readonly object[] NoParams = new object[0];
        private readonly IDictionary<Type, PropertyInfoCache> _propertyCache = new Dictionary<Type, PropertyInfoCache>();

        /// <summary> 
        /// Gets the Type of the given property of the given targetType. 
        /// The targetType and propertyName parameters can't be null. 
        /// </summary> 
        /// <param name="targetType">the target type which contains the property</param> 
        /// <param name="propertyName">the property to get, can be a property on a nested object (eg. "Child.Name")</param>

        public Type GetType(Type targetType, string propertyName)
        {
            if (propertyName.IndexOf(PropertyNameSeparator) > -1)
            {
                string[] propertyList = propertyName.Split(PropertyNameSeparator);
                for (int i = 0; i < propertyList.Length; i++)
                {
                    string currentProperty = propertyList[i];
                    targetType = GetTypeImpl(targetType, currentProperty);
                }
                return targetType;
            }
            
            return GetTypeImpl(targetType, propertyName);
        }

        /// <summary> 
        /// Gets the value of the given property of the given target. 
        /// If objects within the property hierarchy are null references, null will be returned. 
        /// The target and propertyName parameters can't be null. 
        /// </summary> 
        /// <param name="target">the target object to get the value from</param> 
        /// <param name="propertyName">the property to get, can be a property on a nested object (eg. "Child.Name")</param>

        public object GetValue(object target, string propertyName)
        {
            if (propertyName.IndexOf(PropertyNameSeparator) > -1)
            {
                string[] propertyList = propertyName.Split(PropertyNameSeparator);
                for (int i = 0; i < propertyList.Length; i++)
                {
                    string currentProperty = propertyList[i];
                    target = GetValueImpl(target, currentProperty);
                    if (target == null)
                    {
                        return null;
                    }
                }
                return target;
            }
            
            return GetValueImpl(target, propertyName);
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">the new value of the property</param>
        public void SetValue(object target, string propertyName, object value)
        {
            if (propertyName.IndexOf(PropertyNameSeparator) > -1)
            {
                string[] propertyList = propertyName.Split(PropertyNameSeparator);
                for (int i = 0; i < propertyList.Length - 1; i++)
                {
                    propertyName = propertyList[i];
                    target = GetValueImpl(target, propertyName);
                }
                propertyName = propertyList[propertyList.Length - 1];
            }
            SetValueImpl(target, propertyName, value);
        }

        /// <summary> 
        /// Returns the type of the given property on the target instance. 
        /// The type and propertyName parameters can't be null. 
        /// </summary> 
        /// <param name="targetType">the type of the target instance</param> 
        /// <param name="propertyName">the property to retrieve the type for</param> 
        /// <returns>the typr of the given property on the target type</returns> 
        private Type GetTypeImpl(Type targetType, string propertyName)
        {
            return GetPropertyInfo(targetType, propertyName).PropertyType;
        }

        /// <summary> 
        /// Returns the value of the given property on the target instance. 
        /// The target instance and propertyName parameters can't be null. 
        /// </summary> 
        /// <param name="target">the instance on which to get the value</param> 
        /// <param name="propertyName">the property for which to get the value</param> 
        /// <returns>the value of the given property on the target instance</returns> 
        private object GetValueImpl(object target, string propertyName)
        {
            return GetPropertyInfo(target.GetType(), propertyName).GetValue(target, NoParams);
        }

        /// <summary> 
        /// Sets the given property of the target instance to the given value. 
        /// Type mismatches in the parameters of these methods will result in an exception. 
        /// Also, the target instance and propertyName parameters can't be null. 
        /// </summary> 
        /// <param name="target">the instance to set the value on</param> 
        /// <param name="propertyName">the property to set the value on</param> 
        /// <param name="value">the value to set on the target</param> 
        private void SetValueImpl(object target, string propertyName, object value)
        {
            var property = GetPropertyInfo(target.GetType(), propertyName);
            if(property.CanWrite)
                    property.SetValue(target, value, NoParams);
        }

        /// <summary> 
        /// Obtains the PropertyInfo for the given propertyName of the given type from the cache. 
        /// If it is not already in the cache, the PropertyInfo will be looked up and added to 
        /// the cache. 
        /// </summary> 
        /// <param name="type">the type to resolve the property on</param> 
        /// <param name="propertyName">the name of the property to return the PropertyInfo for</param> 
        /// <returns></returns> 
        private PropertyInfo GetPropertyInfo(Type type, string propertyName)
        {
            PropertyInfoCache propertyInfoCache = GetPropertyInfoCache(type);
            if (!propertyInfoCache.ContainsKey(propertyName))
            {
                PropertyInfo propertyInfo = GetBestMatchingProperty(propertyName, type);
                propertyInfoCache.Add(propertyName, propertyInfo);
            }
            return propertyInfoCache[propertyName];
        }

        /// <summary> 
        /// Gets the best matching property info for the given name on the given type if the same property is defined on 
        /// multiple levels in the object hierarchy. 
        /// </summary> 
        private static PropertyInfo GetBestMatchingProperty(string propertyName, Type type)
        {
            PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            PropertyInfo bestMatch = null;
            int bestMatchDistance = int.MaxValue;
            for (int i = 0; i < propertyInfos.Length; i++)
            {
                PropertyInfo info = propertyInfos[i];
                if (info.Name == propertyName)
                {
                    int distance = CalculateDistance(type, info.DeclaringType);
                    if (distance == 0)
                    {
                        // as close as we're gonna get... 
                        return info;
                    }
                    if (distance > 0 && distance < bestMatchDistance)
                    {
                        bestMatch = info;
                        bestMatchDistance = distance;
                    }
                }
            }
            return bestMatch;
        }

        /// <summary> 
        /// Calculates the hierarchy levels between two classes. 
        /// If the targetObjectType is the same as the baseType, the returned distance will be 0. 
        /// If the two types do not belong to the same hierarchy, -1 will be returned. 
        /// </summary> 
        private static int CalculateDistance(Type targetObjectType, Type baseType)
        {
            Type currType = targetObjectType;
            int level = 0;
            while (currType != null)
            {
                if (baseType == currType)
                {
                    return level;
                }
                currType = currType.BaseType;
                level++;
            }
            return -1;
        }

        /// <summary> 
        /// Returns the PropertyInfoCache for the given type. 
        /// If there isn't one available already, a new one will be created. 
        /// </summary> 
        /// <param name="type">the type to retrieve the PropertyInfoCache for</param> 
        /// <returns>the PropertyInfoCache for the given type</returns> 
        private PropertyInfoCache GetPropertyInfoCache(Type type)
        {
            if (!_propertyCache.ContainsKey(type))
            {
                lock (this)
                {
                    if (!_propertyCache.ContainsKey(type))
                    {
                        _propertyCache.Add(type, new PropertyInfoCache());
                    }
                }
            }
            return _propertyCache[type];
        }
    }

    /// <summary> 
    /// Keeps a mapping between a string and a PropertyInfo instance. 
    /// Simply wraps an IDictionary and exposes the relevant operations. 
    /// Putting all this in a separate class makes the calling code more 
    /// readable. 
    /// </summary> 
    internal class PropertyInfoCache
    {
        private readonly IDictionary<string, PropertyInfo> _propertyInfoCache;

        public PropertyInfoCache()
        {
            _propertyInfoCache = new Dictionary<string, PropertyInfo>();
        }

        public bool ContainsKey(string key)
        {
            return _propertyInfoCache.ContainsKey(key);
        }

        public void Add(string key, PropertyInfo value)
        {
            _propertyInfoCache.Add(key, value);
        }

        public PropertyInfo this[string key]
        {
            get { return _propertyInfoCache[key]; }
        }
    }
}