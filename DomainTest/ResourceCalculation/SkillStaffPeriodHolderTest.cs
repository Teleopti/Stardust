using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class SkillStaffPeriodHolderTest
    {
        private SkillStaffPeriodHolder _skillStaffPeriodHolder;
        private IDictionary<ISkill, IEnumerable<ISkillDay>> _skillDays;

        [SetUp]
        public void Setup()
        {
            _skillDays  = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
            _skillStaffPeriodHolder = new SkillStaffPeriodHolder(_skillDays);
        }

        [Test]
        public void VerifyCreate()
        {
            Assert.IsNotNull(_skillStaffPeriodHolder);
            Assert.IsNotNull(_skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary);
        }

        [Test]
        public void VerifyTheDictionary()
        {
            MockRepository mocks = new MockRepository();
            ISkill skill = mocks.StrictMock<ISkill>();
            ISkillDay skillDay = mocks.StrictMock<ISkillDay>();
            IList<ISkillDay> skillDays = new List<ISkillDay>{skillDay};

            var period1 = mocks.StrictMock<ISkillStaffPeriod>();
            var period2 = mocks.StrictMock<ISkillStaffPeriod>();
            var period3 = mocks.StrictMock<ISkillStaffPeriod>();

            var skillStaffPeriods = new [] { period1, period2, period3};

            DateTime dateTime = new DateTime(2009,2,1,23,0,0,DateTimeKind.Utc);

            DateTimePeriod dateTimePeriod1 = new DateTimePeriod(dateTime, dateTime.AddMinutes(15));
            DateTimePeriod dateTimePeriod2 = dateTimePeriod1.MovePeriod(TimeSpan.FromMinutes(15));
            DateTimePeriod dateTimePeriod3 = dateTimePeriod2.MovePeriod(TimeSpan.FromMinutes(15));

            IDictionary<ISkill, IEnumerable<ISkillDay>> skillDaysDictionary = new Dictionary<ISkill, IEnumerable<ISkillDay>>
                                                                            {{skill, skillDays}};

            using (mocks.Record())
            {
                Expect.Call(skillDay.SkillStaffPeriodCollection).Return(skillStaffPeriods);
                Expect.Call(period1.Period).Return(dateTimePeriod1).Repeat.AtLeastOnce();
                Expect.Call(period2.Period).Return(dateTimePeriod2).Repeat.AtLeastOnce();
                Expect.Call(period3.Period).Return(dateTimePeriod3).Repeat.AtLeastOnce();
            }

            using (mocks.Playback())
            {
                _skillStaffPeriodHolder = new SkillStaffPeriodHolder(skillDaysDictionary);
                Assert.AreEqual(1,_skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.Count);
                var dic = _skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skill];
                Assert.AreEqual(3, dic.Count);
            }
        }

        [Test]
        public void VerifyCanCreateSkillStaffPeriodList()
        {
            var mocks = new MockRepository();
            var skill1 = mocks.StrictMock<ISkill>();
            var skill2 = mocks.StrictMock<ISkill>();
            var skillDay1 = mocks.StrictMock<ISkillDay>();
            var skillDay2 = mocks.StrictMock<ISkillDay>();
            IList<ISkillDay> skillDays1 = new List<ISkillDay> { skillDay1 };
            IList<ISkillDay> skillDays2 = new List<ISkillDay> { skillDay2 };

            var period1 = mocks.StrictMock<ISkillStaffPeriod>();
            var period2 = mocks.StrictMock<ISkillStaffPeriod>();
            var period3 = mocks.StrictMock<ISkillStaffPeriod>();

            var skillStaffPeriods1 = new [] { period1, period2, period3 };
            var skillStaffPeriods2 = new [] { period3 };

            var dateTime = new DateTime(2009, 2, 1, 23, 0, 0, DateTimeKind.Utc);

            var dateTimePeriod1 = new DateTimePeriod(dateTime, dateTime.AddMinutes(15));
            DateTimePeriod dateTimePeriod2 = dateTimePeriod1.MovePeriod(TimeSpan.FromMinutes(15));
            DateTimePeriod dateTimePeriod3 = dateTimePeriod2.MovePeriod(TimeSpan.FromMinutes(15));

            IDictionary<ISkill, IEnumerable<ISkillDay>> skillDaysDictionary = new Dictionary<ISkill, IEnumerable<ISkillDay>> { { skill1, skillDays1 },{skill2,skillDays2} };

            using (mocks.Record())
            {
                Expect.Call(skillDay1.SkillStaffPeriodCollection).Return(skillStaffPeriods1).Repeat.AtLeastOnce();
                Expect.Call(skillDay2.SkillStaffPeriodCollection).Return(skillStaffPeriods2).Repeat.AtLeastOnce();
                Expect.Call(period1.Period).Return(dateTimePeriod1).Repeat.AtLeastOnce();
                Expect.Call(period2.Period).Return(dateTimePeriod2).Repeat.AtLeastOnce();
                Expect.Call(period3.Period).Return(dateTimePeriod3).Repeat.AtLeastOnce();
            }

            using (mocks.Playback())
            {
                _skillStaffPeriodHolder = new SkillStaffPeriodHolder(skillDaysDictionary);
                Assert.AreEqual(2, _skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.Count);
                var dic = _skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skill1];
                Assert.AreEqual(3, dic.Count);
                dic = _skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skill2];
                Assert.AreEqual(1, dic.Count);

                var list = _skillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> {skill1, skill2},
                                                                        dateTimePeriod1.ChangeEndTime(
                                                                            TimeSpan.FromMinutes(30)));
                Assert.AreEqual(4,list.Count);

                list = _skillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> { skill1 },
                                                                        dateTimePeriod1.ChangeEndTime(
                                                                            TimeSpan.FromMinutes(15)));
                Assert.AreEqual(2, list.Count);

                list = _skillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> { skill2 },
                                                                        dateTimePeriod1.ChangeEndTime(
                                                                            TimeSpan.FromMinutes(30)));
                Assert.AreEqual(1, list.Count);
            }
        }

		[Test]
		public void ShouldReturnDictionaryOnSkill()
		{
			var mocks = new MockRepository();
			var skill1 = mocks.StrictMock<ISkill>();
			var skill2 = mocks.StrictMock<ISkill>();
			var skillDay1 = mocks.StrictMock<ISkillDay>();
			var skillDay2 = mocks.StrictMock<ISkillDay>();
			IList<ISkillDay> skillDays1 = new List<ISkillDay> { skillDay1 };
			IList<ISkillDay> skillDays2 = new List<ISkillDay> { skillDay2 };

			var period1 = mocks.StrictMock<ISkillStaffPeriod>();
			var period2 = mocks.StrictMock<ISkillStaffPeriod>();
			var period3 = mocks.StrictMock<ISkillStaffPeriod>();

			var skillStaffPeriods1 = new [] { period1, period2, period3 };
			var skillStaffPeriods2 = new [] { period3 };

			var dateTime = new DateTime(2009, 2, 1, 23, 0, 0, DateTimeKind.Utc);

			var dateTimePeriodoutside = new DateTimePeriod(dateTime.AddDays(-1), dateTime.AddDays(-1).AddMinutes(15));
			var dateTimePeriod1 = new DateTimePeriod(dateTime, dateTime.AddMinutes(15));
			var dateTimePeriod2 = dateTimePeriod1.MovePeriod(TimeSpan.FromMinutes(15));
			var dateTimePeriod3 = dateTimePeriod2.MovePeriod(TimeSpan.FromMinutes(15));

			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDaysDictionary = new Dictionary<ISkill, IEnumerable<ISkillDay>> { { skill1, skillDays1 }, { skill2, skillDays2 } };

			using (mocks.Record())
			{
				Expect.Call(skillDay1.SkillStaffPeriodCollection).Return(skillStaffPeriods1).Repeat.AtLeastOnce();
				Expect.Call(skillDay2.SkillStaffPeriodCollection).Return(skillStaffPeriods2).Repeat.AtLeastOnce();
				Expect.Call(period1.Period).Return(dateTimePeriodoutside).Repeat.AtLeastOnce();
				Expect.Call(period2.Period).Return(dateTimePeriod2).Repeat.AtLeastOnce();
				Expect.Call(period3.Period).Return(dateTimePeriod3).Repeat.AtLeastOnce();
			}

			using (mocks.Playback())
			{
				_skillStaffPeriodHolder = new SkillStaffPeriodHolder(skillDaysDictionary);
				Assert.AreEqual(2, _skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.Count);
				var dic = _skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skill1];
				Assert.AreEqual(3, dic.Count);
				dic = _skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skill2];
				Assert.AreEqual(1, dic.Count);

				var list = _skillStaffPeriodHolder.SkillStaffPeriodDictionary(new List<ISkill> { skill1, skill2 },
																		dateTimePeriod1.ChangeEndTime(
																			TimeSpan.FromMinutes(30)));
				Assert.AreEqual(2, list.Count);
				Assert.That(list[skill1].Count, Is.EqualTo(3));

				list = _skillStaffPeriodHolder.SkillStaffPeriodDictionary(new List<ISkill> { skill1 },
																		dateTimePeriod1.ChangeEndTime(
																			TimeSpan.FromMinutes(15)));
				Assert.AreEqual(1, list.Count);

				list = _skillStaffPeriodHolder.SkillStaffPeriodDictionary(new List<ISkill> { skill2 },
																		dateTimePeriod1.ChangeEndTime(
																			TimeSpan.FromMinutes(30)));
				Assert.AreEqual(1, list.Count);
			}
		}

        [Test]
        public void VerifyCanCreateSkillStaffPeriodListForVirtualSkill()
        {
            MockRepository mocks = new MockRepository();
	        ISkill skill1 = SkillFactory.CreateSkill("skill1");
	        skill1.DefaultResolution = 15;
            ISkill skill2 = SkillFactory.CreateSkill("skill2");
			skill2.DefaultResolution = 60;
            IAggregateSkill aggregateSkillSkill = mocks.StrictMock<IAggregateSkill>();
            ISkillDay skillDay1 = mocks.StrictMock<ISkillDay>();
            ISkillDay skillDay2 = mocks.StrictMock<ISkillDay>();
            IList<ISkillDay> skillDays1 = new List<ISkillDay> { skillDay1 };
            IList<ISkillDay> skillDays2 = new List<ISkillDay> { skillDay2 };
            IList<ISkill> skills = new List<ISkill> { skill1, skill2 };
            ReadOnlyCollection<ISkill> aggregatedSkills = new ReadOnlyCollection<ISkill>(skills);

            DateTime dateTime = new DateTime(2009, 2, 1, 23, 0, 0, DateTimeKind.Utc);

            DateTimePeriod dateTimePeriod1 = new DateTimePeriod(dateTime, dateTime.AddMinutes(15));
            DateTimePeriod dateTimePeriod2 = dateTimePeriod1.MovePeriod(TimeSpan.FromMinutes(15));
            DateTimePeriod dateTimePeriod3 = dateTimePeriod2.MovePeriod(TimeSpan.FromMinutes(15));
            DateTimePeriod dateTimePeriod4 = dateTimePeriod3.MovePeriod(TimeSpan.FromMinutes(15));
            DateTimePeriod dateTimePeriod5 = dateTimePeriod1.ChangeEndTime(TimeSpan.FromMinutes(45));

            TimeSpan averageTaskTime = TimeSpan.FromSeconds(20);
            TimeSpan averageAfterTaskTime = TimeSpan.FromSeconds(40);
            ISkillStaffPeriod period1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod1, new Task(5, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay1);
            ISkillStaffPeriod period2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod2, new Task(5, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay1);
            ISkillStaffPeriod period3 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod3, new Task(6, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay1);
            ISkillStaffPeriod period4 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod4, new Task(5, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay1);
            ISkillStaffPeriod period5 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod5, new Task(4, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay2);

            var skillStaffPeriods1 = new [] { period1, period2, period3, period4 };
            var skillStaffPeriods2 = new [] { period5 };

            IDictionary<ISkill, IEnumerable<ISkillDay>> skillDaysDictionary = new Dictionary<ISkill, IEnumerable<ISkillDay>> { { skill1, skillDays1 }, { skill2, skillDays2 } };

            using (mocks.Record())
            {
                Expect.Call(skillDay1.SkillStaffPeriodCollection).Return(skillStaffPeriods1).Repeat.AtLeastOnce();
                Expect.Call(skillDay2.SkillStaffPeriodCollection).Return(skillStaffPeriods2).Repeat.AtLeastOnce();
                Expect.Call(skillDay1.Skill).Return(skill1).Repeat.AtLeastOnce();
                Expect.Call(aggregateSkillSkill.AggregateSkills).Return(aggregatedSkills).Repeat.AtLeastOnce();

            }

            using (mocks.Playback())
            {
                _skillStaffPeriodHolder = new SkillStaffPeriodHolder(skillDaysDictionary);
                Assert.AreEqual(2, _skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.Count);
                var dic = _skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skill1];
                Assert.AreEqual(4, dic.Count);
                dic = _skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skill2];
                Assert.AreEqual(1, dic.Count);

                var list = _skillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkillSkill, dateTimePeriod5);
                Assert.AreEqual(4, list.Count);
                Assert.AreEqual(6,list[0].Payload.TaskData.Tasks);
                Assert.AreEqual(6,list[1].Payload.TaskData.Tasks);
                Assert.AreEqual(7,list[2].Payload.TaskData.Tasks);
                Assert.AreEqual(6,list[3].Payload.TaskData.Tasks);
                Assert.IsTrue(((IAggregateSkillStaffPeriod)list[0]).IsAggregate);
                Assert.IsTrue(((IAggregateSkillStaffPeriod)list[1]).IsAggregate);
                Assert.IsTrue(((IAggregateSkillStaffPeriod)list[2]).IsAggregate);
                Assert.IsTrue(((IAggregateSkillStaffPeriod)list[3]).IsAggregate);
            }
        }


		[Test]
		public void ShouldCreateSkillStaffPeriodListWithCorrectFStaffForVirtualSkillWithDifferentOpenHours()
		{
			var mocks = new MockRepository();
			var skill1 = SkillFactory.CreateSkill("skill1");
			skill1.DefaultResolution = 15;
			var skill2 = SkillFactory.CreateSkill("skill2");
			skill2.DefaultResolution = 60;
			var aggregateSkillSkill = mocks.StrictMock<IAggregateSkill>();
			var skillDay1 = mocks.StrictMock<ISkillDay>();
			var skillDay2 = mocks.StrictMock<ISkillDay>();
			IList<ISkillDay> skillDays1 = new List<ISkillDay> { skillDay1 };
			IList<ISkillDay> skillDays2 = new List<ISkillDay> { skillDay2 };
			IList<ISkill> skills = new List<ISkill> { skill1, skill2 };
			var aggregatedSkills = new ReadOnlyCollection<ISkill>(skills);

			var dateTime = new DateTime(2009, 2, 1, 23, 0, 0, DateTimeKind.Utc);

			var dateTimePeriod1 = new DateTimePeriod(dateTime, dateTime.AddMinutes(15));
			var dateTimePeriod2 = dateTimePeriod1.MovePeriod(TimeSpan.FromMinutes(15));
			var dateTimePeriod3 = dateTimePeriod2.MovePeriod(TimeSpan.FromMinutes(15));
			var dateTimePeriod4 = dateTimePeriod3.MovePeriod(TimeSpan.FromMinutes(15));
			var dateTimePeriod5 = new DateTimePeriod(dateTimePeriod1.StartDateTime.AddMinutes(-30), dateTimePeriod4.EndDateTime);
			
			var averageTaskTime = TimeSpan.FromSeconds(20);
			var averageAfterTaskTime = TimeSpan.FromSeconds(40);
			ISkillStaffPeriod period1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod1, new Task(5, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay1);
			ISkillStaffPeriod period2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod2, new Task(5, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay1);
			ISkillStaffPeriod period3 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod3, new Task(6, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay1);
			ISkillStaffPeriod period4 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod4, new Task(5, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay1);
			ISkillStaffPeriod period5 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod5, new Task(4, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay2);

			((IAggregateSkillStaffPeriod)period4).AggregatedFStaff = 2d;
			((IAggregateSkillStaffPeriod)period4).IsAggregate = true;

			((IAggregateSkillStaffPeriod) period5).AggregatedFStaff = 5d;
			((IAggregateSkillStaffPeriod) period5).IsAggregate = true;
			
			var skillStaffPeriods1 = new [] { period1, period2, period3, period4 };
			var skillStaffPeriods2 = new [] { period5 };

			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDaysDictionary = new Dictionary<ISkill, IEnumerable<ISkillDay>> { { skill1, skillDays1 }, { skill2, skillDays2 } };

			using (mocks.Record())
			{
				Expect.Call(skillDay1.SkillStaffPeriodCollection).Return(skillStaffPeriods1).Repeat.AtLeastOnce();
				Expect.Call(skillDay2.SkillStaffPeriodCollection).Return(skillStaffPeriods2).Repeat.AtLeastOnce();
				Expect.Call(skillDay1.Skill).Return(skill1).Repeat.AtLeastOnce();
				Expect.Call(aggregateSkillSkill.AggregateSkills).Return(aggregatedSkills).Repeat.AtLeastOnce();
			}

			using (mocks.Playback())
			{
				_skillStaffPeriodHolder = new SkillStaffPeriodHolder(skillDaysDictionary);
				Assert.AreEqual(2, _skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.Count);
				var dic = _skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skill1];
				Assert.AreEqual(4, dic.Count);
				dic = _skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skill2];
				Assert.AreEqual(1, dic.Count);

				var list = _skillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkillSkill, dateTimePeriod5);
				Assert.AreEqual(6, list.Count);
				Assert.AreEqual(5d, list[0].FStaff);
				Assert.AreEqual(5d, list[1].FStaff);
				Assert.AreEqual(5d, list[2].FStaff);
				Assert.AreEqual(5d, list[3].FStaff);
				Assert.AreEqual(5d, list[4].FStaff);
				Assert.AreEqual(7d, list[5].FStaff);
			}
		}


        [Test]
        public void VerifyAggregatedPropertiesAreSet()
        {
			var skill1 = SkillFactory.CreateSkill("skill1");
			skill1.DefaultResolution = 15;
			var skill2 = SkillFactory.CreateSkill("skill2");
			skill2.DefaultResolution = 60;

            MockRepository mocks = new MockRepository();
            IAggregateSkill aggregateSkillSkill = mocks.StrictMock<IAggregateSkill>();
            ISkillDay skillDay1 = mocks.StrictMock<ISkillDay>();
            IList<ISkillDay> skillDays1 = new List<ISkillDay> { skillDay1 };
            IList<ISkill> skills = new List<ISkill> { skill1, skill2 };
            ReadOnlyCollection<ISkill> aggregatedSkills = new ReadOnlyCollection<ISkill>(skills);

            DateTime dateTime = new DateTime(2009, 2, 1, 23, 0, 0, DateTimeKind.Utc);

            DateTimePeriod dateTimePeriod1 = new DateTimePeriod(dateTime, dateTime.AddMinutes(15));

            TimeSpan averageTaskTime = TimeSpan.FromSeconds(20);
            TimeSpan averageAfterTaskTime = TimeSpan.FromSeconds(40);
            SkillStaffPeriod period1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod1, new Task(5, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay1);
            SkillStaffPeriodFactory.InjectEstimatedServiceLevel(period1, new Percent(0.07));
            SkillStaffPeriodFactory.InjectForecastedIncomingDemand(period1, 99);
            SkillStaffPeriodFactory.InjectCalculatedResource(period1, 88);

            var skillStaffPeriods1 = new [] { period1 };

            IDictionary<ISkill, IEnumerable<ISkillDay>> skillDaysDictionary = new Dictionary<ISkill, IEnumerable<ISkillDay>> { { skill1, skillDays1 } };
            
            using (mocks.Record())
            {
                Expect.Call(skillDay1.SkillStaffPeriodCollection).Return(skillStaffPeriods1).Repeat.AtLeastOnce();
                Expect.Call(aggregateSkillSkill.AggregateSkills).Return(aggregatedSkills).Repeat.AtLeastOnce();
                Expect.Call(skillDay1.Skill).Return(skill1).Repeat.AtLeastOnce();
            }

            using (mocks.Playback())
            {
                _skillStaffPeriodHolder = new SkillStaffPeriodHolder(skillDaysDictionary);

                var list = _skillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkillSkill, dateTimePeriod1);
                ISkillStaffPeriod skillStaffPeriod = list[0];
                ((IAggregateSkillStaffPeriod)skillStaffPeriod).IsAggregate = false;
                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(99, skillStaffPeriod.Payload.ForecastedIncomingDemand);
                Assert.AreEqual(88, skillStaffPeriod.Payload.CalculatedResource);
                Assert.AreEqual(new Percent(0.07), skillStaffPeriod.EstimatedServiceLevel);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifyCanCreateSkillStaffPeriodDictionaryForVirtualSkillOn()
		{
			MockRepository mocks = new MockRepository();
			ISkill skill1 = SkillFactory.CreateSkill("skill1");
			skill1.DefaultResolution = 15;
			ISkill skill2 = SkillFactory.CreateSkill("skill2");
			skill2.DefaultResolution = 60;
			IAggregateSkill aggregateSkillSkill = mocks.StrictMock<IAggregateSkill>();

			DateTime dateTime = new DateTime(2009, 2, 1, 23, 0, 0, DateTimeKind.Utc);
			ISkillDay skillDay1 = mocks.StrictMock<ISkillDay>();
			ISkillDay skillDay2 = mocks.StrictMock<ISkillDay>();
			IList<ISkillDay> skillDays1 = new List<ISkillDay> { skillDay1 };
			IList<ISkillDay> skillDays2 = new List<ISkillDay> { skillDay2 };
			IList<ISkill> skills = new List<ISkill> { skill1, skill2 };
			ReadOnlyCollection<ISkill> aggregatedSkills = new ReadOnlyCollection<ISkill>(skills);

			DateTimePeriod dateTimePeriod1 = new DateTimePeriod(dateTime, dateTime.AddMinutes(15));
			DateTimePeriod dateTimePeriod2 = dateTimePeriod1.MovePeriod(TimeSpan.FromMinutes(15));
			DateTimePeriod dateTimePeriod3 = dateTimePeriod2.MovePeriod(TimeSpan.FromMinutes(15));
			DateTimePeriod dateTimePeriod4 = dateTimePeriod3.MovePeriod(TimeSpan.FromMinutes(15));
			DateTimePeriod dateTimePeriod5 = dateTimePeriod1.ChangeEndTime(TimeSpan.FromMinutes(45));

			TimeSpan averageTaskTime = TimeSpan.FromSeconds(20);
			TimeSpan averageAfterTaskTime = TimeSpan.FromSeconds(40);
			ISkillStaffPeriod period1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod1, new Task(5, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay1);
			ISkillStaffPeriod period2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod2, new Task(5, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay1);
			ISkillStaffPeriod period3 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod3, new Task(6, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay1);
			ISkillStaffPeriod period4 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod4, new Task(5, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay1);
			ISkillStaffPeriod period5 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod5, new Task(4, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay2);

			var skillStaffPeriods1 = new [] { period1, period2, period3, period4 };
			var skillStaffPeriods2 = new [] { period5 };

			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDaysDictionary = new Dictionary<ISkill, IEnumerable<ISkillDay>> { { skill1, skillDays1 }, { skill2, skillDays2 } };

			using (mocks.Record())
			{
				Expect.Call(skillDay1.SkillStaffPeriodCollection).Return(skillStaffPeriods1).Repeat.AtLeastOnce();
				Expect.Call(skillDay2.SkillStaffPeriodCollection).Return(skillStaffPeriods2).Repeat.AtLeastOnce();
				Expect.Call(skillDay1.Skill).Return(skill1).Repeat.AtLeastOnce();
				Expect.Call(aggregateSkillSkill.AggregateSkills).Return(aggregatedSkills).Repeat.AtLeastOnce();
			}

			using (mocks.Playback())
			{
				_skillStaffPeriodHolder = new SkillStaffPeriodHolder(skillDaysDictionary);
				Assert.AreEqual(2, _skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.Count);
				var dic = _skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skill1];
				Assert.AreEqual(4, dic.Count);
				dic = _skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skill2];
				Assert.AreEqual(1, dic.Count);

				var list = _skillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkillSkill, dateTimePeriod5, true);
				Assert.AreEqual(4, list.Count);
				Assert.AreEqual(6, list[dateTimePeriod1].Payload.TaskData.Tasks);
				Assert.AreEqual(6, list[dateTimePeriod2].Payload.TaskData.Tasks);
				Assert.AreEqual(7, list[dateTimePeriod3].Payload.TaskData.Tasks);
				Assert.AreEqual(6, list[dateTimePeriod4].Payload.TaskData.Tasks);
			}
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldHandleEmptyVirtualSkill()
        {
            MockRepository mocks = new MockRepository();
            ISkill skill1 = SkillFactory.CreateSkill("skill1");
            skill1.DefaultResolution = 15;
            ISkill skill2 = SkillFactory.CreateSkill("skill2");
            skill2.DefaultResolution = 60;
            IAggregateSkill aggregateSkillSkill = mocks.StrictMock<IAggregateSkill>();

            DateTime dateTime = new DateTime(2009, 2, 1, 23, 0, 0, DateTimeKind.Utc);
            ISkillDay skillDay1 = mocks.StrictMock<ISkillDay>();
            ISkillDay skillDay2 = mocks.StrictMock<ISkillDay>();
            IList<ISkillDay> skillDays1 = new List<ISkillDay> { skillDay1 };
            IList<ISkillDay> skillDays2 = new List<ISkillDay> { skillDay2 };
            ReadOnlyCollection<ISkill> aggregatedSkills = new ReadOnlyCollection<ISkill>(new List<ISkill>());

            DateTimePeriod dateTimePeriod1 = new DateTimePeriod(dateTime, dateTime.AddMinutes(15));
            DateTimePeriod dateTimePeriod2 = dateTimePeriod1.MovePeriod(TimeSpan.FromMinutes(15));
            DateTimePeriod dateTimePeriod3 = dateTimePeriod2.MovePeriod(TimeSpan.FromMinutes(15));
            DateTimePeriod dateTimePeriod4 = dateTimePeriod3.MovePeriod(TimeSpan.FromMinutes(15));
            DateTimePeriod dateTimePeriod5 = dateTimePeriod1.ChangeEndTime(TimeSpan.FromMinutes(45));

            TimeSpan averageTaskTime = TimeSpan.FromSeconds(20);
            TimeSpan averageAfterTaskTime = TimeSpan.FromSeconds(40);
            ISkillStaffPeriod period1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod1, new Task(5, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay1);
            ISkillStaffPeriod period2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod2, new Task(5, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay1);
            ISkillStaffPeriod period3 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod3, new Task(6, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay1);
            ISkillStaffPeriod period4 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod4, new Task(5, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay1);
            ISkillStaffPeriod period5 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod5, new Task(4, averageTaskTime, averageAfterTaskTime), ServiceAgreement.DefaultValues(), skillDay2);

            var skillStaffPeriods1 = new [] { period1, period2, period3, period4 };
            var skillStaffPeriods2 = new [] { period5 };

            IDictionary<ISkill, IEnumerable<ISkillDay>> skillDaysDictionary = new Dictionary<ISkill, IEnumerable<ISkillDay>> { { skill1, skillDays1 }, { skill2, skillDays2 } };

            using (mocks.Record())
            {
                Expect.Call(skillDay1.SkillStaffPeriodCollection).Return(skillStaffPeriods1).Repeat.AtLeastOnce();
                Expect.Call(skillDay2.SkillStaffPeriodCollection).Return(skillStaffPeriods2).Repeat.AtLeastOnce();
                Expect.Call(aggregateSkillSkill.AggregateSkills).Return(aggregatedSkills).Repeat.AtLeastOnce();
            }

            using (mocks.Playback())
            {
                _skillStaffPeriodHolder = new SkillStaffPeriodHolder(skillDaysDictionary);
                Assert.AreEqual(2, _skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.Count);
                var dic = _skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skill1];
                Assert.AreEqual(4, dic.Count);
                dic = _skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skill2];
                Assert.AreEqual(1, dic.Count);

                var list = _skillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkillSkill, dateTimePeriod5, true);
                list.Count.Should().Be.EqualTo(0);
            }
        }

        [Test]
        public void ShouldRecalcualteMinMaxStaffAlarm()
        {
            var mocks = new MockRepository();
            var skill1 = SkillFactory.CreateSkill(" ");
            skill1.DefaultResolution = 15;
            var aggregateSkillSkill = mocks.StrictMock<IAggregateSkill>();
            var dateTime = new DateTime(2014, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            var skillDay1 = mocks.StrictMock<ISkillDay>();
            var skillDays1 = new List<ISkillDay> { skillDay1 };
            var aggregatedSkills = new ReadOnlyCollection<ISkill>(new List<ISkill>{skill1});
            var dateTimePeriod1 = new DateTimePeriod(dateTime, dateTime.AddMinutes(15));

            var period1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod1, new Task(5, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20)), ServiceAgreement.DefaultValues(), skillDay1);
            period1.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.BothBroken;
            period1.Payload.SkillPersonData = new SkillPersonData(0, 0);
            period1.Payload.CalculatedLoggedOn = 0;

            var skillStaffPeriods1 = new [] { period1};
            var skillDaysDictionary = new Dictionary<ISkill, IEnumerable<ISkillDay>> { { skill1, skillDays1 } };

            using (mocks.Record())
            {
	            Expect.Call(skillDay1.Skill).Return(skill1).Repeat.AtLeastOnce();
                Expect.Call(skillDay1.SkillStaffPeriodCollection).Return(skillStaffPeriods1).Repeat.AtLeastOnce();
                Expect.Call(aggregateSkillSkill.AggregateSkills).Return(aggregatedSkills).Repeat.AtLeastOnce();
            }

            using (mocks.Playback())
            {
                var target = new SkillStaffPeriodHolder(skillDaysDictionary);
                var aggregatedSkillStaffPeriod = (IAggregateSkillStaffPeriod)target.SkillStaffPeriodList(aggregateSkillSkill, dateTimePeriod1).First();

                Assert.That(aggregatedSkillStaffPeriod.AggregatedMinMaxStaffAlarm, Is.EqualTo(MinMaxStaffBroken.Ok));
            }
        }

        [Test]
        public void VerifyPerActivityDictionary()
        {
            MockRepository mocks = new MockRepository();

            ISkillDay skillDay = mocks.StrictMock<ISkillDay>();
            ISkillDay skillDay2 = mocks.StrictMock<ISkillDay>();

            IList<ISkillDay> skillDays = new List<ISkillDay> {skillDay};
            IList<ISkillDay> skillDays2 = new List<ISkillDay> {skillDay2};

            ISkillStaffPeriod period1 = mocks.StrictMock<ISkillStaffPeriod>();
            ISkillStaffPeriod period2 = mocks.StrictMock<ISkillStaffPeriod>();
            ISkillStaffPeriod period3 = mocks.StrictMock<ISkillStaffPeriod>();
            ISkillStaffPeriod period4 = mocks.StrictMock<ISkillStaffPeriod>();

            var skillStaffPeriods = new [] {period1, period2};
            var skillStaffPeriods2 = new [] {period3, period4};
			
            DateTime dateTime = new DateTime(2009, 2, 1, 23, 0, 0, DateTimeKind.Utc);

            DateTimePeriod dateTimePeriod1 = new DateTimePeriod(dateTime, dateTime.AddMinutes(15));
            DateTimePeriod dateTimePeriod2 = new DateTimePeriod(dateTime.AddMinutes(15), dateTime.AddMinutes(30));
            DateTimePeriod dateTimePeriod3 = new DateTimePeriod(dateTime.AddMinutes(15), dateTime.AddMinutes(30));
            DateTimePeriod dateTimePeriod4 = new DateTimePeriod(dateTime.AddMinutes(30), dateTime.AddMinutes(45));
			IActivity activity1 = new Activity("one");
			ISkill skill1 = SkillFactory.CreateSkill("skill1");
            ISkill skill2 = SkillFactory.CreateSkill("skill2");
	        skill1.Activity = activity1;
			skill2.Activity = activity1;

			IList<ISkill> skills = new List<ISkill> {skill1, skill2};

            IDictionary<ISkill, IEnumerable<ISkillDay>> skillDaysDictionary = new Dictionary<ISkill, IEnumerable<ISkillDay>>
                                                                            {
                                                                                {skill1, skillDays},
                                                                                {skill2, skillDays2}
                                                                            };

            

            ISkillStaff payload1 = mocks.StrictMock<ISkillStaff>();
            ISkillStaff payload2 = mocks.StrictMock<ISkillStaff>();
            ISkillStaff payload3 = mocks.StrictMock<ISkillStaff>();
            ISkillStaff payload4 = mocks.StrictMock<ISkillStaff>();

            DateTimePeriod period = new DateTimePeriod(dateTime, dateTime.AddMinutes(45));
            SkillPersonData skillPersonData = new SkillPersonData(2, 5);

            IPeriodDistribution periodDistribution = mocks.StrictMock<IPeriodDistribution>();

            using (mocks.Record())
            {
                Expect.Call(skillDay.SkillStaffPeriodCollection).Return(skillStaffPeriods);
                Expect.Call(skillDay2.SkillStaffPeriodCollection).Return(skillStaffPeriods2);

                Expect.Call(period1.Period).Return(dateTimePeriod1).Repeat.AtLeastOnce();
                Expect.Call(period2.Period).Return(dateTimePeriod2).Repeat.AtLeastOnce();
                Expect.Call(period3.Period).Return(dateTimePeriod3).Repeat.AtLeastOnce();
                Expect.Call(period4.Period).Return(dateTimePeriod4).Repeat.AtLeastOnce();

                Expect.Call(period1.Payload).Return(payload1).Repeat.AtLeastOnce();
                Expect.Call(period2.Payload).Return(payload2).Repeat.AtLeastOnce();
                Expect.Call(period3.Payload).Return(payload3).Repeat.AtLeastOnce();
                Expect.Call(period4.Payload).Return(payload4).Repeat.AtLeastOnce();

                Expect.Call(payload1.SkillPersonData).Return(skillPersonData).Repeat.Twice();
                Expect.Call(payload2.SkillPersonData).Return(skillPersonData).Repeat.Twice();
                Expect.Call(payload3.SkillPersonData).Return(skillPersonData).Repeat.Twice();
                Expect.Call(payload4.SkillPersonData).Return(skillPersonData).Repeat.Twice();



                Expect.Call(period1.FStaffTime()).Return(new TimeSpan(0, 50, 0)).Repeat.AtLeastOnce();
                Expect.Call(period2.FStaffTime()).Return(new TimeSpan(0, 50, 0)).Repeat.AtLeastOnce();
                Expect.Call(period3.FStaffTime()).Return(new TimeSpan(0, 50, 0)).Repeat.AtLeastOnce();
                Expect.Call(period4.FStaffTime()).Return(new TimeSpan(0, 50, 0)).Repeat.AtLeastOnce();

                Expect.Call(payload1.CalculatedResource).Return(2);
                Expect.Call(payload2.CalculatedResource).Return(2);
                Expect.Call(payload3.CalculatedResource).Return(2);
                Expect.Call(payload4.CalculatedResource).Return(2);

                Expect.Call(period1.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(true)).Return(0).Repeat.AtLeastOnce();
                Expect.Call(period2.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(true)).Return(0).Repeat.AtLeastOnce();
                Expect.Call(period3.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(true)).Return(0).Repeat.AtLeastOnce();
                Expect.Call(period4.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(true)).Return(0).Repeat.AtLeastOnce();
                Expect.Call(period1.PeriodDistribution).Return(periodDistribution);
                Expect.Call(period2.PeriodDistribution).Return(periodDistribution);
                Expect.Call(period3.PeriodDistribution).Return(periodDistribution);
                Expect.Call(period4.PeriodDistribution).Return(periodDistribution);
            }

            using (mocks.Playback())
            {
                _skillStaffPeriodHolder = new SkillStaffPeriodHolder(skillDaysDictionary);
                var ret = _skillStaffPeriodHolder.SkillStaffDataPerActivity(period, skills, new SkillPriorityProvider());
                Assert.IsNotNull(ret);
                var dic = ret[activity1];

                Assert.AreEqual(5, dic[dateTime].MaximumPersons);
                Assert.AreEqual(10, dic[dateTime.AddMinutes(15)].MaximumPersons);
                Assert.AreEqual(5, dic[dateTime.AddMinutes(30)].MaximumPersons);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyBoostFirstAndLastInterval()
        {
            var key1 = new DateTime(2009,2,2,8,0,0,DateTimeKind.Utc);
            var key2 = key1.AddMinutes(15);
            var key3 = key1.AddMinutes(30);
            // gap
            var key4 = key1.AddHours(3);
            var key5 = key1.AddHours(3).AddMinutes(15);
            var key6 = key1.AddHours(3).AddMinutes(30);

            var holder1 = new SkillStaffPeriodDataInfo(10, 10, new DateTimePeriod(key1, key1.AddMinutes(15)), 0, 0, 5,
                                                         null, new Percent(.5), 1);
            var holder2 = new SkillStaffPeriodDataInfo(10, 10, new DateTimePeriod(key2, key2.AddMinutes(15)), 0, 0, 5,
                                                                     null, new Percent(.5), 1);
            var holder3 = new SkillStaffPeriodDataInfo(10, 10, new DateTimePeriod(key3, key3.AddMinutes(15)), 0, 0, 5,
                                                         null, new Percent(.5), 1);
            var holder4 = new SkillStaffPeriodDataInfo(10, 10, new DateTimePeriod(key4, key4.AddMinutes(15)), 0, 0, 5,
                                                         null, new Percent(.5), 1);
            var holder5 = new SkillStaffPeriodDataInfo(10, 10, new DateTimePeriod(key5, key5.AddMinutes(15)), 0, 0, 5,
                                                         null, new Percent(.5), 1);
            var holder6 = new SkillStaffPeriodDataInfo(10, 10, new DateTimePeriod(key6, key6.AddMinutes(15)), 0, 0, 5,
                                                         null, new Percent(.5), 1);

            var dataHolders = new Dictionary<DateTime, ISkillStaffPeriodDataHolder>();
            dataHolders.Add(key2,holder2);
            dataHolders.Add(key1, holder1);
            dataHolders.Add(key6, holder6);
            dataHolders.Add(key4, holder4);
            dataHolders.Add(key5, holder5);
            dataHolders.Add(key3, holder3);

            var result = SkillStaffPeriodHolder.BoostFirstAndLastInterval(dataHolders);

            Assert.IsTrue(result[key1].Boost);
            Assert.IsFalse(result[key2].Boost);
            Assert.IsTrue(result[key3].Boost);
            Assert.IsTrue(result[key4].Boost);
            Assert.IsFalse(result[key5].Boost);
            Assert.IsTrue(result[key6].Boost);
        }

        [Test]
        public void VerifyIntersectingSkillStaffPeriodList()
        {
            IList<ISkill> skills = new List<ISkill>();
            skills.Add(SkillFactory.CreateSkill("xxx"));
            IList<ISkillStaffPeriod> ret = _skillStaffPeriodHolder.IntersectingSkillStaffPeriodList(skills,
                                                                                                    new DateTimePeriod());
            Assert.AreEqual(0, ret.Count);
        }

		[Test]
		public void ShouldNotSplitForecastedIncomingDemand()
		{
			const int length = 15;
			const double forecastValue = 42d;
			var skill = SkillFactory.CreateSkill("skill");
			var skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(skill, new DateTime(2013, 1, 1, 0, 0, 0, DateTimeKind.Utc), length, forecastValue, 10d);
			var result = SkillStaffPeriodHolder.SplitSkillStaffPeriod(skillStaffPeriod, 0.5d, TimeSpan.FromMinutes(5));

			foreach (var staffPeriod in result)
			{
				Assert.AreEqual(forecastValue, staffPeriod.Payload.ForecastedIncomingDemand);
			}
		}

		[Test]
		public void ShouldHandleEstimatedServiceLevelShrinkageWhenHandleAggregate()
		{
			var mock = new MockRepository();
			var estimatedServiceLevel = new Percent(0.7);
			var estimatedServiceLevelShrinkage = new Percent(0.5);
			var skillStaffPeriod = mock.StrictMock<ISkillStaffPeriod>();
			var skillDay = mock.StrictMock<ISkillDay>();
			var skillStaff = new SkillStaff(new Task(5, TimeSpan.FromSeconds(40), TimeSpan.FromSeconds(20)), ServiceAgreement.DefaultValues());
			var aggregateSkillStaffPeriod = (IAggregateSkillStaffPeriod)SkillStaffPeriodFactory.CreateSkillStaffPeriod();
			var kvp = new KeyValuePair<DateTimePeriod, IList<ISkillStaffPeriod>>(new DateTimePeriod(2015, 1, 1, 2015, 1, 1), new List<ISkillStaffPeriod>());
			var skill = SkillFactory.CreateSkill("skill");


			using (mock.Record())
			{
				Expect.Call(skillStaffPeriod.FStaff).Return(0.0);
				Expect.Call(skillStaffPeriod.CalculatedResource).Return(0.0);
				Expect.Call(skillStaffPeriod.Payload).Return(skillStaff).Repeat.AtLeastOnce();
				Expect.Call(skillStaffPeriod.EstimatedServiceLevel).Return(estimatedServiceLevel);
				Expect.Call(skillStaffPeriod.EstimatedServiceLevelShrinkage).Return(estimatedServiceLevelShrinkage);
				Expect.Call(skillStaffPeriod.CalculatedLoggedOn).Return(0);
				Expect.Call(skillStaffPeriod.SkillDay).Return(skillDay);
				Expect.Call(skillDay.Skill).Return(skill);
				Expect.Call(skillStaffPeriod.RelativeDifference).Return(0.0).Repeat.AtLeastOnce();
				Expect.Call(skillStaffPeriod.DateTimePeriod).Return(new DateTimePeriod()).Repeat.Any();
			}

			using (mock.Playback())
			{
				SkillStaffPeriodHolder.HandleAggregate(kvp, null, skillStaffPeriod, aggregateSkillStaffPeriod);
				Assert.AreEqual(estimatedServiceLevelShrinkage, aggregateSkillStaffPeriod.AggregatedEstimatedServiceLevelShrinkage);	
			}	
	    }
    }
}
