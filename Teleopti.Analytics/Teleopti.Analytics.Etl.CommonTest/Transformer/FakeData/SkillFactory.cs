using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData
{
    public static class SkillFactory
    {
        public static IList<ISkill> CreateSkillList()
        {
            ISkillType skillType = SkillTypeFactory.CreateSkillTypePhone();
            skillType.SetId(Guid.NewGuid());

            ISkill skill1 = new Skill("skill 1", "desc skill 1", Color.FromArgb(0), 15, skillType);
            skill1.SetId(Guid.NewGuid());
            skill1.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            RaptorTransformerHelper.SetUpdatedOn(skill1, DateTime.Now);

            ISkill skill2 = new Skill("skill 2", "desc skill 2 [deleted]", Color.FromArgb(255), 15, skillType);
            skill2.SetId(Guid.NewGuid());
            // Set IsDeleted = True
            ((IDeleteTag)skill2).SetDeleted();
            skill2.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
            RaptorTransformerHelper.SetUpdatedOn(skill2, DateTime.Now);

            return new List<ISkill> {skill1, skill2 };
        }
    }
}