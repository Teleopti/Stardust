using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting
{
    public abstract class SettingData : Entity, ISettingData
    {
        private string _key;
        private byte[] serializedValue = {1}; 
        protected SettingData(){}
        private readonly object _serializeLock = new object();
        
        protected SettingData(string key)
        {
            _key = key;
        }

        public virtual string Key
        {
            get { return _key; }
        }


        public virtual T GetValue<T>(T defaultValue) where T : class, ISettingValue
        {
            lock (_serializeLock)
            {
                IFormatter formatter = new BinaryFormatter();
                T ret = null;
                try
                {
                    ret = deserializeAndSetParent<T>(formatter);
                }
                catch (SerializationException)
                {
                    SetValue(defaultValue);
                    ret = deserializeAndSetParent<T>(formatter);
                    return ret;
                }

                if (ret != null)
                    return ret;
                SetValue(defaultValue);
                ret = deserializeAndSetParent<T>(formatter);

                return ret;
            }
        }


        public virtual void SetValue<T>(T value) where T : class, ISettingValue
        {
            IFormatter formatter = new BinaryFormatter();
            using(MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, value);
                serializedValue = stream.GetBuffer();                
            }
        }

        private T deserializeAndSetParent<T>(IFormatter formatter) where T : class, ISettingValue
        {
            using(MemoryStream stream = new MemoryStream(serializedValue))
            {
                object tempObj = formatter.Deserialize(stream);
                T ret = tempObj as T;
                if (ret != null)
                    ret.SetOwner(this);
                return ret;                
            }
        }
    }
}
