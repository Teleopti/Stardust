using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
                T ret;
                try
                {
                    ret = deserializeAndSetParent<T>(formatter);
                }
                catch (SerializationException)
                {
                    SetValue(defaultValue);
                    return deserializeAndSetParent<T>(formatter);
                }

                if (ret != null)
                    return ret;
                SetValue(defaultValue);
                return deserializeAndSetParent<T>(formatter);
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
			//temp hack - remove if we get rid of "type dependency" in db
			formatter.Binder = new TypeConverter();
			//
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
