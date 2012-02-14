using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting
{
    [Serializable]
    public abstract class SettingValue : ISettingValue
    {
        [NonSerialized]
        private ISettingData _owner;


        ISettingData ISettingValue.BelongsTo
        {
            get { return BelongsTo; }
        }

        protected ISettingData BelongsTo
        {
            get
            {
                return _owner;
            }
        }

        void ISettingValue.SetOwner(ISettingData owner)
        {
            SetOwner(owner);
        }


        protected void SetOwner(ISettingData owner)
        {
            _owner = owner;
        }

    }
}
