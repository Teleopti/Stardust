using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class InitializeRootsPersonAbsence
    {
        private readonly IEnumerable<IPersonAbsence> _personAbsences;

        public InitializeRootsPersonAbsence(IEnumerable<IPersonAbsence> personAbsences)
        {
            _personAbsences = personAbsences;
        }

        public void Initialize()
        {
            foreach (var personAbsence in _personAbsences)
            {
                if (!LazyLoadingManager.IsInitialized(personAbsence.Layer.Payload))
                    LazyLoadingManager.Initialize(personAbsence.Layer.Payload);
            }
        }
    }


}
