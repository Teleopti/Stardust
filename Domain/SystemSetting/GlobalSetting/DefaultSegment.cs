using System;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
    [Serializable]
    public class DefaultSegment : SettingValue
    {
        private int _segmentLength = 15;

        public int SegmentLength
        {
            get { return _segmentLength; }
            set { _segmentLength = value; }
        }
    }
}