using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;

using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class SingleSkillDictionaryTest
	{
		private SingleSkillDictionary _singleSkillDictionary;
		private MockRepository _mock = new MockRepository();
		private IList<IPerson> _persons;
		private IPerson _person1;
		private IPerson _person2;
		private DateOnlyPeriod _period;
		private IPersonPeriod _personPeriod1;
		private IPersonPeriod _personPeriod2;
		private IPersonPeriod _personPeriod3;
		private IList<IPersonSkill> _personSkills1;
		private IList<IPersonSkill> _personSkills2;
		private IList<IPersonSkill> _personSkills3;
		private IPersonSkill _personSkill1;
		private IPersonSkill _personSkill2;
		private IPersonSkill _personSkill3;
		private ISkill _phoneSkill1;
		private ISkill _phoneSkill2;
		private ISkill _mailSkill;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_person1 = _mock.StrictMock<IPerson>();
			_person2 = _mock.StrictMock<IPerson>();
			_persons = new List<IPerson> { _person1, _person2 };
			_period = new DateOnlyPeriod(new DateOnly(2012, 1, 1), new DateOnly(2012, 1, 2));
			_singleSkillDictionary = new SingleSkillDictionary();
			_personPeriod1 = _mock.StrictMock<IPersonPeriod>();
			_personPeriod2 = _mock.StrictMock<IPersonPeriod>();
			_personPeriod3 = _mock.StrictMock<IPersonPeriod>();
			_personSkill1 = _mock.StrictMock<IPersonSkill>();
			_personSkill2 = _mock.StrictMock<IPersonSkill>();
			_personSkill3 = _mock.StrictMock<IPersonSkill>();
			
			var skillTypePhone = new SkillTypePhone(new Description(), new ForecastSource());
			_phoneSkill1 = SkillFactory.CreateSkill("PhoneSkill1", skillTypePhone, 15);
			_phoneSkill2 = SkillFactory.CreateSkill("PhoneSkill2", skillTypePhone, 15);

			var skillTypeEmail = new SkillTypeEmail(new Description(), new ForecastSource());
			_mailSkill = SkillFactory.CreateSkill("MailSkill", skillTypeEmail, 15);
		}

		[Test]
		public void ShouldReturnFalseIfDictionaryNotContainsPersonDate()
		{
			var person = _mock.StrictMock<IPerson>();
			var dateOnly = new DateOnly(2012, 10, 1);
			var state = _singleSkillDictionary.IsSingleSkill(person, dateOnly);
			Assert.IsFalse(state);
		}

		[Test]
		public void ShouldReturnFalseIfPersonDoNotHavePersonPeriod()
		{
			_persons = new List<IPerson> { _person1 };
			_period = new DateOnlyPeriod(new DateOnly(2012, 1, 1), new DateOnly(2012, 1, 1));

			using(_mock.Record())
			{
				Expect.Call(_person1.Period(_period.StartDate)).Return(null).Repeat.Twice();
			}

			using(_mock.Playback())
			{
				_singleSkillDictionary.Create(_persons, _period);
				var state = _singleSkillDictionary.IsSingleSkill(_person1, _period.StartDate);
				Assert.IsFalse(state);
			}
		}

		[Test]
		public void ShouldReturnFalseIfPersonIsNotSingleSkilled()
		{
			_persons = new List<IPerson> { _person1 };
			_period = new DateOnlyPeriod(new DateOnly(2012, 1, 1), new DateOnly(2012, 1, 1));	
			_personSkills1 = new List<IPersonSkill> { _personSkill1, _personSkill2 };

			using (_mock.Record())
			{
				Expect.Call(_person1.Period(_period.StartDate)).Return(_personPeriod1);
				Expect.Call(_personPeriod1.PersonSkillCollection).Return(_personSkills1).Repeat.AtLeastOnce();
				Expect.Call(_personSkill1.Skill).Return(_phoneSkill1).Repeat.AtLeastOnce();
				Expect.Call(_personSkill2.Skill).Return(_phoneSkill2).Repeat.AtLeastOnce();
				Expect.Call(_personSkill1.Active).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_personSkill2.Active).Return(true).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				_singleSkillDictionary.Create(_persons, _period);
				var state = _singleSkillDictionary.IsSingleSkill(_person1, _period.StartDate);
				Assert.IsFalse(state);
			}
		}

		[Test]
		public void ShouldReturnTrueIfSkillOnlyExistsOnSingleSkilledAgents()
		{
			_persons = new List<IPerson> { _person1, _person2 };
			_period = new DateOnlyPeriod(new DateOnly(2012, 1, 1), new DateOnly(2012, 1, 2));
			_personSkills1 = new List<IPersonSkill> { _personSkill1 };
			_personSkills2 = new List<IPersonSkill> { _personSkill2 };

			using (_mock.Record())
			{
				Expect.Call(_person1.Period(_period.DayCollection()[0])).Return(_personPeriod1).Repeat.Twice();
				Expect.Call(_person2.Period(_period.DayCollection()[0])).Return(_personPeriod2).Repeat.Twice();
				Expect.Call(_person1.Period(_period.DayCollection()[1])).Return(_personPeriod1).Repeat.Twice();
				Expect.Call(_person2.Period(_period.DayCollection()[1])).Return(_personPeriod2).Repeat.Twice();

				Expect.Call(_personPeriod1.PersonSkillCollection).Return(_personSkills1).Repeat.AtLeastOnce();
				Expect.Call(_personPeriod2.PersonSkillCollection).Return(_personSkills2).Repeat.AtLeastOnce();

				Expect.Call(_personSkill1.Skill).Return(_phoneSkill1).Repeat.AtLeastOnce();
				Expect.Call(_personSkill2.Skill).Return(_phoneSkill1).Repeat.AtLeastOnce();
				Expect.Call(_personSkill1.Active).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_personSkill2.Active).Return(true).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				_singleSkillDictionary.Create(_persons, _period);
				var person1Day1 = _singleSkillDictionary.IsSingleSkill(_person1, _period.DayCollection()[0]);
				var person1Day2 = _singleSkillDictionary.IsSingleSkill(_person1, _period.DayCollection()[1]);
				var person2Day1 = _singleSkillDictionary.IsSingleSkill(_person2, _period.DayCollection()[0]);
				var person2Day2 = _singleSkillDictionary.IsSingleSkill(_person2, _period.DayCollection()[1]);
				Assert.IsTrue(person1Day1);
				Assert.IsTrue(person1Day2);
				Assert.IsTrue(person2Day1);
				Assert.IsTrue(person2Day2);
			}
		}

		[Test]
		public void ShouldReturnFalseOnSingleSkilledAgentIfSkillExistsOnDoubleSkilledAgent()
		{
			_persons = new List<IPerson> { _person1, _person2 };
			_period = new DateOnlyPeriod(new DateOnly(2012, 1, 1), new DateOnly(2012, 1, 2));
			_personSkills1 = new List<IPersonSkill> { _personSkill1 };
			_personSkills2 = new List<IPersonSkill> { _personSkill2 };
			_personSkills3 = new List<IPersonSkill> { _personSkill2, _personSkill3 };

			using (_mock.Record())
			{
				Expect.Call(_person1.Period(_period.DayCollection()[0])).Return(_personPeriod1).Repeat.Twice();
				Expect.Call(_person2.Period(_period.DayCollection()[0])).Return(_personPeriod2).Repeat.Twice();
				Expect.Call(_person1.Period(_period.DayCollection()[1])).Return(_personPeriod1).Repeat.Twice();
				Expect.Call(_person2.Period(_period.DayCollection()[1])).Return(_personPeriod3);

				Expect.Call(_personPeriod1.PersonSkillCollection).Return(_personSkills1).Repeat.AtLeastOnce();
				Expect.Call(_personPeriod2.PersonSkillCollection).Return(_personSkills2).Repeat.AtLeastOnce();
				Expect.Call(_personPeriod3.PersonSkillCollection).Return(_personSkills3).Repeat.AtLeastOnce();

				Expect.Call(_personSkill1.Skill).Return(_phoneSkill1).Repeat.AtLeastOnce();
				Expect.Call(_personSkill2.Skill).Return(_phoneSkill1).Repeat.AtLeastOnce();
				Expect.Call(_personSkill3.Skill).Return(_phoneSkill2).Repeat.AtLeastOnce();

				Expect.Call(_personSkill1.Active).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_personSkill2.Active).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_personSkill3.Active).Return(true).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				_singleSkillDictionary.Create(_persons, _period);
				var person1Day1 = _singleSkillDictionary.IsSingleSkill(_person1, _period.DayCollection()[0]);
				var person1Day2 = _singleSkillDictionary.IsSingleSkill(_person1, _period.DayCollection()[1]);
				var person2Day1 = _singleSkillDictionary.IsSingleSkill(_person2, _period.DayCollection()[0]);
				var person2Day2 = _singleSkillDictionary.IsSingleSkill(_person2, _period.DayCollection()[1]);
				Assert.IsTrue(person1Day1);
				Assert.IsFalse(person1Day2);
				Assert.IsTrue(person2Day1);
				Assert.IsFalse(person2Day2);
			}
		}

		[Test]
		public void ShouldReturnFalseWhenSkillTypeIsNotPhone()
		{
			_persons = new List<IPerson> { _person1, _person2 };
			_period = new DateOnlyPeriod(new DateOnly(2012, 1, 1), new DateOnly(2012, 1, 2));
			_personSkills1 = new List<IPersonSkill> { _personSkill1 };
			_personSkills2 = new List<IPersonSkill> { _personSkill2 };

			using (_mock.Record())
			{
				Expect.Call(_person1.Period(_period.DayCollection()[0])).Return(_personPeriod1).Repeat.Twice();
				Expect.Call(_person2.Period(_period.DayCollection()[0])).Return(_personPeriod2).Repeat.Twice();
				Expect.Call(_person1.Period(_period.DayCollection()[1])).Return(_personPeriod1).Repeat.Twice();
				Expect.Call(_person2.Period(_period.DayCollection()[1])).Return(_personPeriod2).Repeat.Twice();

				Expect.Call(_personPeriod1.PersonSkillCollection).Return(_personSkills1).Repeat.AtLeastOnce();
				Expect.Call(_personPeriod2.PersonSkillCollection).Return(_personSkills2).Repeat.AtLeastOnce();

				Expect.Call(_personSkill1.Skill).Return(_phoneSkill1).Repeat.AtLeastOnce();
				Expect.Call(_personSkill2.Skill).Return(_mailSkill).Repeat.AtLeastOnce();

				Expect.Call(_personSkill1.Active).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_personSkill2.Active).Return(true).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				_singleSkillDictionary.Create(_persons, _period);
				var person1Day1 = _singleSkillDictionary.IsSingleSkill(_person1, _period.DayCollection()[0]);
				var person1Day2 = _singleSkillDictionary.IsSingleSkill(_person1, _period.DayCollection()[1]);
				var person2Day1 = _singleSkillDictionary.IsSingleSkill(_person2, _period.DayCollection()[0]);
				var person2Day2 = _singleSkillDictionary.IsSingleSkill(_person2, _period.DayCollection()[1]);
				Assert.IsTrue(person1Day1);
				Assert.IsTrue(person1Day2);
				Assert.IsFalse(person2Day1);
				Assert.IsFalse(person2Day2);
			}
		}

		[Test]
		public void ShouldReturnTrueIfSecondSkillNotActive()
		{
			_persons = new List<IPerson> { _person1 };
			_period = new DateOnlyPeriod(new DateOnly(2012, 1, 1), new DateOnly(2012, 1, 1));
			_personSkill1 = new PersonSkill(_phoneSkill1, new Percent(1));
			_personSkill2 = new PersonSkill(_mailSkill, new Percent(1));
			((IPersonSkillModify)_personSkill2).Active = false;
			_personSkills1 = new List<IPersonSkill> { _personSkill1, _personSkill2 };

			using (_mock.Record())
			{
				Expect.Call(_person1.Period(_period.DayCollection()[0])).Return(_personPeriod1).Repeat.Twice();
				Expect.Call(_personPeriod1.PersonSkillCollection).Return(_personSkills1).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				_singleSkillDictionary.Create(_persons, _period);
				var person1Day1 = _singleSkillDictionary.IsSingleSkill(_person1, _period.DayCollection()[0]);
				Assert.IsTrue(person1Day1);
			}
		}

		[Test]
		public void ShouldReturnTrueIfSecondSkillDeleted()
		{
			_persons = new List<IPerson> { _person1 };
			_period = new DateOnlyPeriod(new DateOnly(2012, 1, 1), new DateOnly(2012, 1, 1));
			_personSkill1 = new PersonSkill(_phoneSkill1, new Percent(1));
			((IDeleteTag)_mailSkill).SetDeleted();
			_personSkill2 = new PersonSkill(_mailSkill, new Percent(1));
			_personSkills1 = new List<IPersonSkill> { _personSkill1, _personSkill2 };

			using (_mock.Record())
			{
				Expect.Call(_person1.Period(_period.DayCollection()[0])).Return(_personPeriod1).Repeat.Twice();
				Expect.Call(_personPeriod1.PersonSkillCollection).Return(_personSkills1).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				_singleSkillDictionary.Create(_persons, _period);
				var person1Day1 = _singleSkillDictionary.IsSingleSkill(_person1, _period.DayCollection()[0]);
				Assert.IsTrue(person1Day1);
			}
		}

	}
}
