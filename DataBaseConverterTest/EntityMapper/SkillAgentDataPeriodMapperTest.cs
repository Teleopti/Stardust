using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class SkillAgentDataPeriodMapperTest : MapperTest<global::Domain.SkillData>
    {
        protected override int NumberOfPropertiesToConvert
        {
            get { return 10; }
        }

        [Test]
        public void CanMapSkillAgentDataPeriod()
        {
            
            int interval = 26; //06:30
            int intervalLength = 15;
            
            MinMax<int> agents = new MinMax<int>(3, 6);

            IDictionary<global::Domain.SkillData, global::Domain.Skill> parents = new Dictionary<global::Domain.SkillData, global::Domain.Skill>();
            MappedObjectPair mapped = new MappedObjectPair();
            
            Skill skill = new Skill("SkillName", "Desc", 12, 15, new SkillType("TEST", 12, "22"));

                //Create old Skill
            global::Domain.Skill oldSkill =
                new global::Domain.Skill(1, global::Domain.SkillType.InboundTelephony,
                    "Old Skill", "Desc", Color.DodgerBlue, null, null, null, false,
                          true, null);

            //Create old skill data
            global::Domain.SkillData oldSkillData = new 
                global::Domain.SkillData(interval,0,0,agents.Minimum,agents.Maximum,0,0,
                        0,true,0,0,3,2,null);

            parents[oldSkillData] = oldSkill;
            ObjectPairCollection<global::Domain.Skill, Skill> pair = new ObjectPairCollection<global::Domain.Skill, Skill>();
            pair.Add(oldSkill, skill);
            mapped.Skill = pair;

            //Create skill date
            DateTime skillDate = new DateTime(2006, 12, 15);

            //Create Expected date Intervall=26	= 06:30 Will be -> '2006-12-15 06:30'
            DateTime expectedIntervallDate = new DateTime(2006, 12, 15, 6, 30, 0, DateTimeKind.Utc);

            SkillAgentDataPeriodMapper target = new SkillAgentDataPeriodMapper(mapped, TimeZoneInfo.Utc, skillDate, intervalLength, parents);

            SkillPersonDataPeriod newSkillPersonDataPeriod = target.Map(oldSkillData);

            Assert.AreEqual(expectedIntervallDate, newSkillPersonDataPeriod.Period.StartDateTime);
            Assert.AreEqual(expectedIntervallDate.AddMinutes(15), newSkillPersonDataPeriod.Period.EndDateTime);

            Assert.AreEqual(agents, newSkillPersonDataPeriod.SkillPersonData.PersonCollection);
            
            Assert.AreSame(skill, newSkillPersonDataPeriod.Skill);
        }
    }
}