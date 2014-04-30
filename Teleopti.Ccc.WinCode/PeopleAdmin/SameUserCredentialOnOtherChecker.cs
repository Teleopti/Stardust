using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
    public class SameUserCredentialOnOtherChecker
    {
        private readonly IPersonRepository _personRepository;

        public SameUserCredentialOnOtherChecker(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public IList<ISameUserCredentialOnOther> CheckConflictsBeforeSave(IList<IPerson> personsToSave)
        {
            var retlist = new List<ISameUserCredentialOnOther>();
            retlist.AddRange(checkAginstPotentialConflicts(personsToSave, personsToSave));

            var conflicts = _personRepository.FindPersonsWithGivenUserCredentials(personsToSave);
            retlist.AddRange(checkAginstPotentialConflicts(personsToSave, conflicts));

            return retlist;
        }

        private static IEnumerable<ISameUserCredentialOnOther> checkAginstPotentialConflicts(IEnumerable<IPerson> personsToSave, IEnumerable<IPerson> potentialConlicts)
        {
            var retList = new List<ISameUserCredentialOnOther>();
            var conflicts = new List<IPerson>();
            foreach (var person in personsToSave)
            {
                if (conflicts.Contains(person))
                    continue;

                var identity = person.AuthenticationInfo == null ? "" : person.AuthenticationInfo.Identity;
                var appLogOnName = person.ApplicationAuthenticationInfo == null ? "" : person.ApplicationAuthenticationInfo.ApplicationLogOnName;

                if ((!string.IsNullOrEmpty(identity)) || !string.IsNullOrEmpty(appLogOnName))
                {
                    foreach (var conflictPerson in potentialConlicts)
                    {
                        if (person.Equals(conflictPerson))
                            continue;
                        if (conflicts.Contains(conflictPerson))
                            continue;
                        var conflictIdentity = conflictPerson.AuthenticationInfo == null ? "" : conflictPerson.AuthenticationInfo.Identity;
                        var conflictAppLogOnName = conflictPerson.ApplicationAuthenticationInfo == null ? "" : conflictPerson.ApplicationAuthenticationInfo.ApplicationLogOnName.ToUpperInvariant();
                        if (!string.IsNullOrEmpty(appLogOnName) && appLogOnName.ToUpperInvariant().Equals(conflictAppLogOnName))
                        {
                            retList.Add(new SameUserCredentialOnOther(person, conflictPerson));
                            conflicts.Add(person);
                            conflicts.Add(conflictPerson);
                            continue;
                        }
						
                        if(!string.IsNullOrEmpty(identity))
                        {
                            if (identity.ToUpperInvariant().Equals(conflictIdentity.ToUpperInvariant()))
                            {
                                retList.Add(new SameUserCredentialOnOther(person, conflictPerson));
                                conflicts.Add(person);
                                conflicts.Add(conflictPerson);
                                continue;
                            }
                        }
                    }
                }
            }
            return retList;
        }
    }

    public interface ISameUserCredentialOnOther
    {
        IPerson Person { get; }
        IPerson ConflictingPerson { get; }
    }

    public class SameUserCredentialOnOther : ISameUserCredentialOnOther
    {
        private readonly IPerson _person;
        private readonly IPerson _conflictsWith;

        public SameUserCredentialOnOther(IPerson person, IPerson conflictsWith)
        {
            _person = person;
            _conflictsWith = conflictsWith;
        }

        public IPerson Person { get { return _person; } }
        public IPerson ConflictingPerson { get { return _conflictsWith; } }
    }
}