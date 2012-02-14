using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.Win.Common
{
    [Serializable]
    public class VirtualSkillSetting : SettingValue
    {
        private readonly IDictionary<string, VirtualSkill> _virtualSkills = new Dictionary<string, VirtualSkill>();


        public IDictionary<string, VirtualSkill> VirtualSkills
        {
            get { return _virtualSkills; }
        }
    }
}