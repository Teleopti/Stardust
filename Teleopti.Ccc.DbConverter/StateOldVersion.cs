using System.Collections.Generic;
using Infrastructure;

namespace Teleopti.Ccc.DBConverter
{
    /// <summary>
    /// The cache used with this tool
    /// </summary>
    internal class StateOldVersion : IState
    {
        private readonly IDictionary<string, object> _applicationCache;
        private readonly IDictionary<string, object> _sessionCache;
        private readonly IDictionary<string, object> _contextCache;
        private readonly IDictionary<string, object> _requestCache;


        /// <summary>
        /// Initializes a new instance of the <see cref="StateOldVersion"/> class.
        /// </summary>
        public StateOldVersion()
        {
            _applicationCache = new Dictionary<string, object>();
            _sessionCache = new Dictionary<string, object>();
            _contextCache = new Dictionary<string, object>();
            _requestCache = new Dictionary<string, object>();
        }

        #region IState Members

        /// <summary>
        /// Clears context cache
        /// </summary>
        /// <remarks></remarks>
        public void ClearContextCache()
        {
            _contextCache.Clear();
        }

        /// <summary>
        /// Get_s the application item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object get_ApplicationItem(IApplicationKey key)
        {
            object retVal;
            _applicationCache.TryGetValue(key.Key, out retVal);
            return retVal;
        }

        /// <summary>
        /// Get_s the context item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object get_ContextItem(string key)
        {
            object retVal;
            _contextCache.TryGetValue(key, out retVal);
            return retVal;
        }

        /// <summary>
        /// Get_s the request item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object get_RequestItem(ContextKey key)
        {
            object retVal;
            _requestCache.TryGetValue(key.ToString(), out retVal);
            return retVal;
        }

        /// <summary>
        /// Get_s the session item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object get_SessionItem(SessionKey key)
        {
            object retVal;
            _sessionCache.TryGetValue(key.ToString(), out retVal);
            return retVal;
        }

        /// <summary>
        /// Set_s the application item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="Value">The value.</param>
        public void set_ApplicationItem(IApplicationKey key, object Value)
        {
            _applicationCache[key.Key] = Value;
        }

        /// <summary>
        /// Set_s the context item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="Value">The value.</param>
        public void set_ContextItem(string key, object Value)
        {
            _contextCache[key] = Value;
        }

        /// <summary>
        /// Set_s the request item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="Value">The value.</param>
        public void set_RequestItem(ContextKey key, object Value)
        {
            _requestCache[key.ToString()] = Value;
        }

        /// <summary>
        /// Set_s the session item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="Value">The value.</param>
        public void set_SessionItem(SessionKey key, object Value)
        {
            _sessionCache[key.ToString()] = Value;
        }

        #endregion
    }
}