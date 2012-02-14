#region Imports

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;

#endregion

namespace Teleopti.Ccc.AgentPortal.Proxy
{

    /// <summary>
    /// Represents a ProxyInvocationHelper
    /// </summary>
    public static class ProxyInvocationHelper
    {

        #region Fields - Instance Member

        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - ProxyInvocationHelper Members

        #endregion

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - ProxyInvocationHelper Members

        /// <summary>
        /// Calls the method.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        /// <param name="method">The method.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/27/2008
        /// </remarks>
        public static object CallMethod(object proxy, string method, object[] args)
        {
            Type proxyType = proxy.GetType();
            MethodInfo proxyMethod = proxyType.GetMethod(method, BindingFlags.Public | BindingFlags.Instance);

            ParameterInfo[] methodParams = proxyMethod.GetParameters();

            int defaultSize = args.Length;
            for (int j = 0; j <= (defaultSize - 1); j++)
            {
                if(!methodParams[j].ParameterType.IsPrimitive)
                {
                    if(args[j] != null)
                    {
                        object val = Convert.ChangeType(args[j], methodParams[j].ParameterType, CultureInfo.CurrentCulture);
                        args[j] = val;
                    }
                }
            }

            bool checkForOutParameters = false;
            if (methodParams.Length > args.Length)
            {
                int difference = (methodParams.Length - args.Length);
                Array.Resize(ref args, (args.Length + difference));

                for (int i = defaultSize; i <= args.Length - 1; i++)
                {
                    object paramTypeValue;

                    if (!methodParams[i].IsOut)
                        paramTypeValue = Activator.CreateInstance(methodParams[i].ParameterType);
                    else
                        paramTypeValue = Activator.CreateInstance(Type.GetType(methodParams[i].ParameterType.FullName.Replace("&", "")));

                    if (methodParams[i].ParameterType.Equals(typeof(bool)))
                        paramTypeValue = true;
                    args[i] = paramTypeValue;
                }
                if (methodParams[defaultSize].IsOut)
                    checkForOutParameters = true;
            }

            object returnValue = proxyMethod.Invoke(proxy, args);
            if (checkForOutParameters)
                returnValue = args[defaultSize];

            Type returnValueType = returnValue.GetType();
            if (returnValueType.IsAssignableFrom(returnValue.GetType()))
            {
                return returnValue;
            }
            else
            {
                if (returnValueType.Namespace.Contains("System.Collections.Generic"))
                {
                    Type t = returnValue.GetType().GetGenericArguments()[0];

                    Type targetReturnType = typeof(List<>).MakeGenericType(t);
                    object targetReturnTypeInstance = Activator.CreateInstance(targetReturnType);

                    object[] returnValueArray;
                    
                    int count = (int) returnValueType.GetProperty("Count").GetValue(returnValue, null);

                    returnValueArray = (object[])returnValueType.GetMethod("ToArray", BindingFlags.Public | BindingFlags.Instance).
                        Invoke(returnValue, null);

                    MethodInfo addMethod = targetReturnTypeInstance.GetType().GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);

                    for(int i = 0; i <= (count - 1); i++)
                    {
                        object item = returnValueArray[i];
                        addMethod.Invoke(targetReturnTypeInstance, new object[] { item });
                    }

                    return targetReturnTypeInstance;
                }
                return returnValue;
            }
        }

        /// <summary>
        /// Does the convert.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="sourceList">The source list.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/27/2008
        /// </remarks>
        public static Collection<TSource> DoConvert<TSource, TTarget>(TTarget[] sourceList)
        {
            IList<TSource> targetList = new List<TSource>();
            Collection<TSource> target = new Collection<TSource>(targetList);

            foreach (object source in sourceList)
            {
                targetList.Add((TSource)source);
            }
            return target;
        }

        /// <summary>
        /// Does the convert.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="sourceList">The source list.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/27/2008
        /// </remarks>
        /// TODO: For the moment TSource value is ignored. But it is used in the code generation.
        public static object[] DoConvert<TSource, TTarget>(Collection<TTarget> sourceList)
        {
            object[] target = new object[sourceList.Count];

            int index = 0;
            foreach (TTarget source in sourceList)
            {
                target[index] = source;
                index++;
            }
            return target;
        }

        /// <summary>
        /// Copies the array.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/27/2008
        /// </remarks>
        public static TTarget[] CopyArray<TTarget>(object[] source)
        {
            TTarget[] target = new TTarget[source.Length];
            source.CopyTo(target, 0);
            return target;
        }

        #endregion

        #endregion

    }

}
