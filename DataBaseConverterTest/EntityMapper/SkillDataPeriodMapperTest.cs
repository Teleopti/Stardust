using System;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class SkillDataPeriodMapperTest : MapperTest<global::Domain.SkillData>
    {
        private Percent _percent;

        protected override int NumberOfPropertiesToConvert
        {
            get { return 11; }
        }

        [Test]
        public void CanMapSkillDataPeriod()
        {
            int interval = 26; //06:30
            int intervalLength = 15;
            
            Percent percent = new Percent(0.14);
            double seconds = 12;
            ServiceLevel serviceLevel = new ServiceLevel(percent, seconds);
            Percent minOccupancy = new Percent(0.10);
            Percent maxOccupancy = new Percent(0.80);
            MappedObjectPair mapped = new MappedObjectPair();
            
            //CreateProjection old skill data
            global::Domain.SkillData oldSkillData = new 
                global::Domain.SkillData(interval,0,0,1,0,0,(int)seconds,
                        (int)(percent.Value*100),true,0,0,(int)(minOccupancy.Value*100),
                            (int)(maxOccupancy.Value*100),null);


            //CreateProjection Expected date Intervall=26	= 06:30 Will be -> '2006-12-15 06:30'
            DateTime expectedIntervallDate = new DateTime(2006, 12, 15, 6, 30, 0, DateTimeKind.Utc);

            SkillDataPeriodMapper target = new SkillDataPeriodMapper(mapped, new CccTimeZoneInfo(TimeZoneInfo.Utc), expectedIntervallDate.Date, intervalLength);

            SkillDataPeriod newSkillDataPeriod = target.Map(oldSkillData);

            Assert.AreEqual(expectedIntervallDate, newSkillDataPeriod.Period.StartDateTime);
            Assert.AreEqual(expectedIntervallDate.Add(TimeSpan.FromMinutes(15)), newSkillDataPeriod.Period.EndDateTime);

            Assert.AreEqual(minOccupancy, newSkillDataPeriod.ServiceAgreement.MinOccupancy);
            Assert.AreEqual(maxOccupancy, newSkillDataPeriod.ServiceAgreement.MaxOccupancy);
            Assert.AreEqual(serviceLevel.Percent, newSkillDataPeriod.ServiceAgreement.ServiceLevel.Percent);
            Assert.AreEqual(serviceLevel.Seconds, newSkillDataPeriod.ServiceAgreement.ServiceLevel.Seconds);
            Assert.AreEqual(oldSkillData.Shrinkage, newSkillDataPeriod.Shrinkage.Value);

            Assert.AreEqual(1, newSkillDataPeriod.SkillPersonData.MinimumPersons);
            Assert.AreEqual(0, newSkillDataPeriod.SkillPersonData.MaximumPersons);
        }

        [Test]
        public void CanConvertSpecificShrinkage()
        {
            int interval = 26; //06:30
            int intervalLength = 15;

            _percent = new Percent(0.14);
            double seconds = 12;
            Percent minOccupancy = new Percent(0.10);
            Percent maxOccupancy = new Percent(0.80);
            MappedObjectPair mapped = new MappedObjectPair();

            //CreateProjection old skill data
            global::Domain.SkillData oldSkillData = new
                global::Domain.SkillData(interval, 0, 1.07, 1, 0, 0, (int)seconds,
                        (int)(_percent.Value * 100), true, 0, 0, (int)(minOccupancy.Value * 100),
                            (int)(maxOccupancy.Value * 100), null);


            //CreateProjection Expected date Intervall=26	= 06:30 Will be -> '2006-12-15 06:30'
            DateTime expectedIntervallDate = new DateTime(2006, 12, 15, 6, 30, 0, DateTimeKind.Utc);

            SkillDataPeriodMapper target = new SkillDataPeriodMapper(mapped, new CccTimeZoneInfo(TimeZoneInfo.Utc), expectedIntervallDate.Date, intervalLength);

            SkillDataPeriod newSkillDataPeriod = target.Map(oldSkillData);

            Assert.AreEqual(Math.Round(new Percent(0.07).Value, 4), Math.Round(newSkillDataPeriod.Shrinkage.Value, 4));
            oldSkillData = new
                global::Domain.SkillData(interval, 0, 1, 1, 0, 0, (int)seconds,
                        (int)(_percent.Value * 100), true, 0, 0, (int)(minOccupancy.Value * 100),
                            (int)(maxOccupancy.Value * 100), null);
            newSkillDataPeriod = target.Map(oldSkillData);
            Assert.AreEqual(Math.Round(new Percent(0).Value, 4), Math.Round(newSkillDataPeriod.Shrinkage.Value, 4));
        }
    }
}