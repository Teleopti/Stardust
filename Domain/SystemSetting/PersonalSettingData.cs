using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting
{
    public class PersonalSettingData : SettingData
    {
        private readonly IPerson _ownerPerson;

        public PersonalSettingData()
        {
        }

        public PersonalSettingData(string name, IPerson ownerPerson) : base(name)
        {
            _ownerPerson = ownerPerson;
        }

        public virtual IPerson OwnerPerson => _ownerPerson;
    }
}
