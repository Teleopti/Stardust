using System;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Common
{
    public class FakeNameFormatSettingProvider : ISettingsPersisterAndProvider<NameFormatSettings>
    {
        private int _nameFormatId;

        public NameFormatSettings Persist(NameFormatSettings isActive)
        {
            throw new NotImplementedException();
        }

        public void SetNameFormat(NameFormatSetting format)
        {
            _nameFormatId = (int) format;
        }

        public NameFormatSettings Get()
        {
            return new NameFormatSettings {NameFormatId = _nameFormatId };
        }

        public NameFormatSettings GetByOwner(IPerson person)
        {
            throw new NotImplementedException();
        }
    }
}