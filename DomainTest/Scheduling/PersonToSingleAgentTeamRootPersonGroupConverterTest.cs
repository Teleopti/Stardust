using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class PersonToSingleAgentTeamRootPersonGroupConverterTest
	{
		private IPersonToSingleAgentTeamRootPersonGroupConverter _target;
		private DateOnly _dateOnly;
		private IPerson _person;

		[SetUp]
		public void Setup()
		{
			_target = new PersonToSingleAgentTeamRootPersonGroupConverter(new SingleAgentTeamGroupPage());
			_person = PersonFactory.CreatePerson("Bill");
			_dateOnly = new DateOnly();
		}

		[Test]
		public void ShouldConvertToRootPersonGroup()
		{
			var result = _target.Convert(_person, _dateOnly);
			Assert.That(result.PersonCollection.First(), Is.EqualTo(_person));
		}
	}
}
