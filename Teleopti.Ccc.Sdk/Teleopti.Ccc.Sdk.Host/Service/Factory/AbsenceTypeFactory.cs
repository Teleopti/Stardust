using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
    internal static class AbsenceTypeFactory
    {
        internal static ICollection<AbsenceDto> GetAbsences(AbsenceLoadOptionDto absenceLoadOptionDto)
        {
            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                if (absenceLoadOptionDto.LoadDeleted)
                {
                    using (unitOfWork.DisableFilter(QueryFilter.Deleted))
                    {
                        if (absenceLoadOptionDto.LoadRequestable)
                            return GetAbsencesRequestable(unitOfWork);  // Loads all requestable absences incl deleted

                        return GetAbsences(unitOfWork); // Loads all absences incl deleted
                    }
                }

                if (absenceLoadOptionDto.LoadRequestable)
                    return GetAbsencesRequestable(unitOfWork);  // Loads all requestable absences

                return GetAbsences(unitOfWork); // Loads all absences
            }
        }

        private static ICollection<AbsenceDto> GetAbsencesRequestable(IUnitOfWork uow)
        {
            AbsenceRepository rep = AbsenceRepository.DONT_USE_CTOR(uow);
            IList<IAbsence> absenceList = rep.LoadRequestableAbsence();
            AbsenceAssembler absenceAssembler = new AbsenceAssembler(rep);
            return absenceAssembler.DomainEntitiesToDtos(absenceList).ToList();
        }

        private static ICollection<AbsenceDto> GetAbsences(IUnitOfWork uow)
        {
            AbsenceRepository rep = AbsenceRepository.DONT_USE_CTOR(uow);
            IEnumerable<IAbsence> absenceList = rep.LoadAllSortByName();
            AbsenceAssembler absenceAssembler = new AbsenceAssembler(rep);
            return absenceAssembler.DomainEntitiesToDtos(absenceList).ToList();
        }
    }
}