using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Domain;
using Infrastructure;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter2;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Infrastructure;
using Skill=Teleopti.Ccc.Domain.Forecasting.Skill;
using SkillType=Domain.SkillType;

namespace Teleopti.Ccc.DatabaseConverterTest.CollectionConverter2
{
    /// <summary>
    /// Tests for SkillConverter
    /// </summary>
    [TestFixture]
    public class SkillConverterTest
    {
        private MockRepository _mocks;
        private IUnitOfWork _uowMock;
        private IRepository<Skill> _repMock;
        private SkillConverter _converter;
        private ISkillReader _oldSkillRepMock;
        private MappedObjectPair mappedObjectPair;

        /// <summary>
        /// Sets the up.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            //create converter
            _converter = new SkillConverter();
            //Mocks
            _mocks = new MockRepository();
            _uowMock = _mocks.CreateMock<IUnitOfWork>();
            _repMock = _mocks.CreateMock<IRepository<Skill>>();
            _oldSkillRepMock = _mocks.CreateMock<ISkillReader>();
            mappedObjectPair = new MappedObjectPair();
        }


        /// <summary>
        /// Determines whether this instance [can convert old object].
        /// </summary>
        [Test]
        public void CanConvertOldObject()
        {
            //Get the "old skilltypes"
            ObjectPairCollection<SkillType, Domain.Forecasting.SkillType> skillTypeDic = GetSkillTypes();

            //create an old forecast
            Forecast oldForcast = new Forecast(12, "Workload-Name", "Description");

            ObjectPairCollection<Forecast, Workload> workloadMapList =
                new ObjectPairCollection<Forecast, Workload>();

            //Create a new Workload
            Workload newWorkload = new Workload("Workload-Name", "WorkLoad-Description");

            //Put old an new in a "old-new" object pair list
            workloadMapList.Add(oldForcast, newWorkload);

            mappedObjectPair.Workload = workloadMapList;
            mappedObjectPair.SkillType = skillTypeDic;

            //Also put it in the old skill object (required in old skills ctor)
            CccListCollection<Forecast> oldForeCastList = new CccListCollection<Forecast>();
            oldForeCastList.Add(oldForcast);

            global::Domain.Skill oldSkill =
                new global::Domain.Skill(2, SkillType.InboundTelephony, "SkillName", "Skill description", Color.Blue,
                                         null, null, oldForeCastList, false, true, null);

            //Mock the old skills
            IDictionary<int, global::Domain.Skill> skillDic = new Dictionary<int, global::Domain.Skill>();
            skillDic[0] = oldSkill;


            //Old skill repository
            Expect.On(_oldSkillRepMock)
                .Call(_oldSkillRepMock.GetAll())
                .Return(skillDic);

            _repMock.Add(null);
            LastCall.Repeat.Once().IgnoreArguments();

            _uowMock.PersistAll();
            LastCall.Repeat.AtLeastOnce();

            _mocks.ReplayAll();


            ObjectPairCollection<global::Domain.Skill, Skill> retList =
                _converter.Convert(_uowMock, 
                                   new SkillMapper(mappedObjectPair), 
                                   _oldSkillRepMock, 
                                   _repMock);

            Assert.AreEqual(oldSkill.Name, retList.GetPaired(oldSkill).Name);
            Assert.AreEqual(oldSkill.Description, retList.GetPaired(oldSkill).Description);
            Assert.AreEqual(oldSkill.DisplayColor.ToArgb(), retList.GetPaired(oldSkill).DisplayColor.ToArgb());
            Assert.AreEqual(skillTypeDic.GetPaired(oldSkill.TypeOfSkill), retList.GetPaired(oldSkill).SkillType);

            //Workload name
            Assert.AreEqual("Workload-Name", retList.GetPaired(oldSkill).WorkloadCollection[0].Name);

        }

        private static ObjectPairCollection<SkillType, Domain.Forecasting.SkillType> GetSkillTypes()
        {
            //These skilltypes are system set in CCC 6.5 they are mapped
            //databse <-> Enum global::Domain.SkillType

            Domain.Forecasting.SkillType skillType1 =
                new Domain.Forecasting.SkillType("PHONE", 700245, "inbound telephony");
            Domain.Forecasting.SkillType skillType2 = new Domain.Forecasting.SkillType("EMAIL", 700245, "email");
            Domain.Forecasting.SkillType skillType3 = new Domain.Forecasting.SkillType("FAX", 700245, "fax");
            Domain.Forecasting.SkillType skillType4 = new Domain.Forecasting.SkillType("BOFFICE", 700245, "backoffice");
            Domain.Forecasting.SkillType skillType5 = new Domain.Forecasting.SkillType("PROJECT", 700245, "project");
            Domain.Forecasting.SkillType skillType6 = new Domain.Forecasting.SkillType("TIME", 700245, "time");

            ObjectPairCollection<SkillType, Domain.Forecasting.SkillType> skillTypeDic =
                new ObjectPairCollection<SkillType, Domain.Forecasting.SkillType>();

            skillTypeDic.Add(SkillType.InboundTelephony, skillType1);
            skillTypeDic.Add(SkillType.email, skillType2);
            skillTypeDic.Add(SkillType.fax, skillType3);
            skillTypeDic.Add(SkillType.backoffice, skillType4);
            skillTypeDic.Add(SkillType.project, skillType5);
            skillTypeDic.Add(SkillType.time, skillType6);
            return skillTypeDic;
        }
    }
}