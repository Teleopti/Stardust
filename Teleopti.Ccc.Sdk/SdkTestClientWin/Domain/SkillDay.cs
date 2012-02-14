using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SdkTestClientWin.Sdk;

namespace SdkTestClientWin.Domain
{
    public class SkillDay
    {
        private SkillDayDto _dto;

        public SkillDay(SkillDayDto dto)
        {
            _dto = dto;
        }

        public SkillDayDto Dto
        {
            get { return _dto; }
        }
    }
}
