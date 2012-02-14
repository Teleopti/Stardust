using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Logging;

namespace Teleopti.Caching.Core
{
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    public class Cache : ICache
    {
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MegaByte")]
        public const int MegaByte = 1048576;
        private readonly string _strCacheShortName;
        private readonly string _strCacheLongName;
        private string _curpath;
        private int _streamLength;
        private MemoryMappedFileStream _memoryMappedFileStream;

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public Cache(string cacheName)
        {
            if(String.IsNullOrEmpty(cacheName))
                throw new ArgumentNullException("cacheName", "Cache Name cannot be null.");
            GetCurrentPath();
            if (AppDomain.CurrentDomain.GetData("streamLength") != null)
            {
                _streamLength = Convert.ToInt32(AppDomain.CurrentDomain.GetData("streamLength"), CultureInfo.InvariantCulture);
            }
            else
            {
                _streamLength = MegaByte;
                AppDomain.CurrentDomain.SetData("streamLength", MegaByte);
            }
            _strCacheShortName = cacheName;
            _strCacheLongName = String.Format(CultureInfo.InvariantCulture, "{0}{1}", cacheName, ".dat");
            FillCacheFromMemoryMappedFile();
            Debug.Assert(!Equals(cacheName, null), "cacheName is null.");
            Debug.Assert(cacheName.Length > 0, "cacheName is empty.");
        }

        private void GetCurrentPath()
        {
            _curpath = Path.GetTempPath();
        }

        private void FillCacheFromMemoryMappedFile()
        {
            _memoryMappedFileStream = new MemoryMappedFileStream(_curpath + _strCacheLongName, FileAccess.ReadWrite, 0, _streamLength, _strCacheShortName);
            byte[] byteArray = new byte[(int)_memoryMappedFileStream.Capacity];
            _memoryMappedFileStream.Read(byteArray, 0, (int)_memoryMappedFileStream.Capacity);
            if (byteArray.Length != _streamLength)
                UpdateStreamLength(byteArray.Length);
            try { DeserializeByteArray(byteArray); }
            catch (SerializationException) { CreateNewCache(); }
        }

        public void Add(string key, object data)
        {
            if (String.IsNullOrEmpty(key))
                throw new ArgumentNullException("key", "Key cannot be null.");
            if (data == null)
                throw new ArgumentNullException("data", "Data cannot be null.");
            if (!(data is Hashtable))
                throw new ArgumentException("Data needs to be of type Hashtable.", "data");
            Hashtable htCache = GetHashtable();
            if (htCache != null && htCache.ContainsKey(key))
            {
                htCache[key] = data;
                SerializeAndUpdateCache(htCache);
            }
            else if (htCache != null)
            {
                htCache.Add(key, data);
                SerializeAndUpdateCache(htCache);
            }
        }

        public object Items(string key)
        {
            Hashtable htCache = GetHashtable();
            if (htCache != null && htCache.ContainsKey(key))
            {
                return htCache[key];
            }
            return null;
        }

        private Hashtable GetHashtable()
        {
            byte[] byteArray = new byte[_streamLength];
            ReadCache(byteArray);
            if (byteArray.Length != _streamLength)
                UpdateStreamLength(byteArray.Length);
            Hashtable htCache = (Hashtable)DeserializeByteArray(byteArray);
            return htCache;
        }

        public void Remove(string key)
        {
            if (String.IsNullOrEmpty(key))
                throw new ArgumentNullException("key", "Key cannot be null.");
            Hashtable htCache = GetHashtable();
            if (htCache != null && htCache.ContainsKey(key))
            {
                htCache.Remove(key);
                byte[] newByteArray = SerializeObject(htCache);
                if (newByteArray.Length != _streamLength)
                    UpdateStreamLength(newByteArray.Length);
                WriteCache(newByteArray);
            }
        }

        private void CreateNewCache()
        {
            BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), "Cache::CreateNewCache : Memory Mapped File is empty.");
            Hashtable htCache = new Hashtable();
            byte[] newByteArray = SerializeObject(htCache);
            if (newByteArray.Length != _streamLength)
                UpdateStreamLength(newByteArray.Length);
            WriteCache(newByteArray);
        }

        private void SerializeAndUpdateCache(Hashtable htCache)
        {
            byte[] byteArray = SerializeObject(htCache);
            UpdateStreamLength(byteArray.Length);
            WriteCache(byteArray);
        }

        private void ReadCache(byte[] byteArray)
        {
            if (!_memoryMappedFileStream.CanRead)
            {
                _memoryMappedFileStream = new MemoryMappedFileStream(_curpath + _strCacheLongName, FileAccess.ReadWrite, 0, _streamLength, _strCacheShortName);    
            }
            _memoryMappedFileStream.Read(byteArray, 0, _streamLength);
            _memoryMappedFileStream.Flush();
            _memoryMappedFileStream.Close();
        }

        private void WriteCache(byte[] newByteArray)
        {
            if (!_memoryMappedFileStream.CanWrite)
            {
                _memoryMappedFileStream = new MemoryMappedFileStream(_curpath + _strCacheLongName, FileAccess.ReadWrite,
                                                                     0, _streamLength, _strCacheShortName);
            }
            _memoryMappedFileStream.Write(newByteArray, 0, newByteArray.Length);
            _memoryMappedFileStream.Flush();
            _memoryMappedFileStream.Close();
        }

        #pragma warning disable 1692

        public byte[] SerializeObject(object value)
        {
            BinaryFormatter b = new BinaryFormatter();
            MemoryStream m = new MemoryStream();
            b.Serialize(m, value);
            byte[] by = m.ToArray();
            m.Close();
            return by;
        }

        
        public object DeserializeByteArray(byte[] value)
        {
            try
            {
                object o;
                MemoryStream m = new MemoryStream(value);
                BinaryFormatter bf = new BinaryFormatter();
                o = bf.Deserialize(m);
                m.Close();
                return o;
            }
            catch (SerializationException)
            {
                return new Hashtable();
            }
        }

        #pragma warning restore 1692

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public void UpdateStreamLength(int newValue)
        {
            if (newValue == 0) 
                newValue = MegaByte;
            _streamLength = newValue;
            AppDomain.CurrentDomain.SetData("streamLength", newValue);
        }

        public object this[string key]
        {
            get
            {
                return Items(key);
            }
            set
            {
                Add(key, value);
            }
        }

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _memoryMappedFileStream.Dispose();
            }
        }

        ~Cache()
        {
            Dispose(false);
        }

        #endregion

    }
}