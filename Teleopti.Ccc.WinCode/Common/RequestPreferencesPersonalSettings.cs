using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.WinCode.Common
{
    [Serializable]
    public class RequestPreferencesPersonalSettings : SettingValue
    {
        private IList<FilterBoxAdvancedFilter> _list;
        public IList<FilterBoxAdvancedFilter> MapTo()
        {

            return _list;

        }

        public void MapFrom(IRequestPreferences source)
        {
            if (source != null) _list = source.RequestList;
        }

        public bool IsSettingExtracted()
        {
            if (_list == null)
                return false;
            return true;
        }
    }

    public interface IRequestPreferences
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        IList<FilterBoxAdvancedFilter> RequestList { get; set; }
    }

    public class RequestPreferences : IRequestPreferences
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<FilterBoxAdvancedFilter> RequestList { get; set; }
    }
}
