using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.WinCode.PeopleAdmin;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
    [TestFixture]
    public class SameUserCredentialOnOtherCheckerTest
    {
        private MockRepository _mock;
        private IPersonRepository _personRepository;
        private SameUserCredentialOnOtherChecker _target;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _target = new SameUserCredentialOnOtherChecker(_personRepository);
        }

        [Test]
        public void ShouldFindDuplicatesInUnsavedData()
        {
            var p1 = createPerson("aa", "a", "aaa");
            var p2 = createPerson("aa", "a", "bbb");
            var p3 = createPerson("bb", "b", "aaa");

            var persons = new List<IPerson> {p1, p2, p3};
            var savedPersonsConflicting = new List<IPerson>();
            Expect.Call(_personRepository.FindPersonsWithGivenUserCredentials(persons)).Return(savedPersonsConflicting);
            _mock.ReplayAll();
            var ret = _target.CheckConflictsBeforeSave(persons);

            Assert.That(ret.Count, Is.EqualTo(2));
            _mock.VerifyAll();
        }

		[Test]
		public void ShouldFindDuplicatesWhenSameDomainNameAndEmptyLogOnName()
		{
			var p1 = createPerson("", "domain", "a");
			var p2 = createPerson("", "domain", "b");

			var persons = new List<IPerson> {p1, p2};
			var savedPersonsConflicting = new List<IPerson>();
			
			Expect.Call(_personRepository.FindPersonsWithGivenUserCredentials(persons)).Return(savedPersonsConflicting);
			_mock.ReplayAll();
			var ret = _target.CheckConflictsBeforeSave(persons);

			Assert.That(ret.Count, Is.EqualTo(1));
			_mock.VerifyAll();
		}

        [Test]
        public void ShouldFindDuplicatesInSavedData()
        {
            var p1 = createPerson("aa", "a", "aaa");
            var p2 = createPerson("bb", "b", "bbb");
            var p3 = createPerson("cc", "c", "ccc");
            
            var p4 = createPerson("cc", "c", "ccc");

            var persons = new List<IPerson> { p1, p2, p3 };
            var savedPersonsConflicting = new List<IPerson> { p4};
            Expect.Call(_personRepository.FindPersonsWithGivenUserCredentials(persons)).Return(savedPersonsConflicting);
            _mock.ReplayAll();
            var ret = _target.CheckConflictsBeforeSave(persons);

            Assert.That(ret.Count, Is.EqualTo(1));
            
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldNotFindDuplicateOnEmptyAppLogOn()
        {
            var p1 = createPerson("aa", "a", "");
            var p2 = createPerson("bb", "b", "");

            var persons = new List<IPerson> { p1, p2};
            var savedPersonsConflicting = new List<IPerson>();
            Expect.Call(_personRepository.FindPersonsWithGivenUserCredentials(persons)).Return(savedPersonsConflicting);
            _mock.ReplayAll();
            var ret = _target.CheckConflictsBeforeSave(persons);

            Assert.That(ret.Count, Is.EqualTo(0));

            _mock.VerifyAll();
        }

        [Test]
        public void ShouldFindDuplicatesInSmallAndBigLetters()
        {
            var p1 = createPerson("AA", "A", "aaA");
            var p2 = createPerson("aa", "a", "bbb");
            var p3 = createPerson("bb", "b", "aaa");

            var persons = new List<IPerson> { p1, p2, p3 };
            var savedPersonsConflicting = new List<IPerson>();
            Expect.Call(_personRepository.FindPersonsWithGivenUserCredentials(persons)).Return(savedPersonsConflicting);
            _mock.ReplayAll();
            var ret = _target.CheckConflictsBeforeSave(persons);

            Assert.That(ret.Count, Is.EqualTo(2));
            _mock.VerifyAll();
        }

        private static IPerson createPerson(string winLogOn, string domain, string appLogOn)
        {
            var person = new Person
                             {
								 AuthenticationInfo = new AuthenticationInfo { Identity = IdentityHelper.Merge(domain, winLogOn) }
                             };

            person.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo {ApplicationLogOnName = appLogOn};
            return person;
        }
    }

    
}