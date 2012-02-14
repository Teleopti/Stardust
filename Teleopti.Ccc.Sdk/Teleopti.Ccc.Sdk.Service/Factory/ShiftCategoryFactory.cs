using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    internal static class ShiftCategoryFactory
    {
        internal static ICollection<ShiftCategoryDto> GetShiftCategories(LoadOptionDto loadOptionDto)
        {
            ICollection<ShiftCategoryDto> list;
            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IRepositoryFactory repositoryFactory = new RepositoryFactory();
                IShiftCategoryRepository repository = repositoryFactory.CreateShiftCategoryRepository(unitOfWork);
                ShiftCategoryAssembler shiftCategoryAssembler = new ShiftCategoryAssembler(repository);
                
                IList<IShiftCategory> shiftCategoryList;
                if (loadOptionDto.LoadDeleted)
                {
                    using (unitOfWork.DisableFilter(QueryFilter.Deleted))
                    {
                        shiftCategoryList = repository.LoadAll();
                    }
                }
                else
                {
                    shiftCategoryList = repository.LoadAll();
                }
                list = shiftCategoryAssembler.DomainEntitiesToDtos(shiftCategoryList).ToList();
            }
            return list;
        }
    }
}