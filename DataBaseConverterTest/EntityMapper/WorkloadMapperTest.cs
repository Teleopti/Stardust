using System.Collections.Generic;
using System.Globalization;
using Domain;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// WorkLoadMapper Test
    /// </summary>
    [TestFixture]
    public class WorkloadMapperTest : MapperTest<Forecast>
    {
        ISkill _skill;

        [SetUp]
        public void Setup()
        {
            _skill = SkillFactory.CreateSkill("TestSkill");
        }

       
        [Test]
        public void CanMapCorrectWorkloadToQueueId()
        {
            string oldName = "Name";
            string oldDescription = "Desc";

            //create a forecastQueuesourceMap list
            ObjectPairCollection<int, IQueueSource> queueSourceDic = new ObjectPairCollection<int, IQueueSource>();
            
            //****THIS WILL MAP old queueId(99) to foreCastId(12)
            int queueId = 99;
            int foreCastId = 12;

            Forecast oldWorkload = new Forecast(foreCastId, oldName, oldDescription);
            IDictionary<int, IntegerPair> queueSourceMap = new Dictionary<int, IntegerPair>();
            IntegerPair forecastQueueMap = new IntegerPair(foreCastId, queueId);
            queueSourceMap.Add(1, forecastQueueMap); //Key 1 has no meaning

            QueueSource qs =
                new QueueSource(queueId.ToString(CultureInfo.CurrentCulture),
                                queueId.ToString(CultureInfo.CurrentCulture), queueId);

            //The queue is the key to the QueueSource
            queueSourceDic.Add(queueId, qs);

            MappedObjectPair mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.QueueSource = queueSourceDic;

            WorkloadMapper wlMap = new WorkloadMapper(mappedObjectPair, _skill, queueSourceMap);
            IWorkload newWorkload = wlMap.Map(oldWorkload);

            Assert.AreEqual(queueId, newWorkload.QueueSourceCollection[0].QueueAggId);
        }

        [Test]
        public void VerifyCanMapQueueAdjustments()
        {
            string oldName = "Name";
            string oldDescription = "Desc";

            Forecast oldWorkload = new Forecast(12, oldName, oldDescription,10,20,30,40,50,1);
            //oldWorkload.ActOflInPercent = 1;
            ObjectPairCollection<int, IQueueSource> queueSourceDic = new ObjectPairCollection<int, IQueueSource>();

            MappedObjectPair mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.QueueSource = queueSourceDic;

            WorkloadMapper wlMap = new WorkloadMapper(mappedObjectPair, _skill, new Dictionary<int, IntegerPair>());
            IWorkload newWorkload = wlMap.Map(oldWorkload);

            Assert.AreEqual(new Percent(1), newWorkload.QueueAdjustments.OfferedTasks);
            Assert.AreEqual(new Percent(-1), newWorkload.QueueAdjustments.Abandoned);
            Assert.AreEqual(new Percent(0.10),newWorkload.QueueAdjustments.AbandonedShort);
            Assert.AreEqual(new Percent(0.20), newWorkload.QueueAdjustments.AbandonedWithinServiceLevel);
            Assert.AreEqual(new Percent(0.30), newWorkload.QueueAdjustments.AbandonedAfterServiceLevel);
            Assert.AreEqual(new Percent(0.40), newWorkload.QueueAdjustments.OverflowIn);
            Assert.AreEqual(new Percent(0.50), newWorkload.QueueAdjustments.OverflowOut);
        }

        protected override int NumberOfPropertiesToConvert
        {
            get { return 10; }
        }
    }
}