using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models
{
    public class ImportForecastModel
    {
        private readonly ISkill _skill;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IImportForecastsRepository _importForecastsRepository;

        public ImportForecastModel(ISkill preselectedSkill, IUnitOfWorkFactory unitOfWorkFactory, IImportForecastsRepository importForecastsRepository)
        {
            _skill = preselectedSkill;
            _unitOfWorkFactory = unitOfWorkFactory;
            _importForecastsRepository = importForecastsRepository;
        }

        public IEnumerable<IWorkload> LoadWorkloadList()
        {
            return _skill.WorkloadCollection.OrderBy(wl => wl.Name).ToList();
        }

        public string GetSelectedSkillName()
        {
            return _skill.Name;
        }

        public Guid? SaveForecastFileInDb(string fileName, byte[] fileContent)
        {
            Guid? result = null;
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
               var forecastFile = new ForecastFile(fileName, fileContent);
               _importForecastsRepository.Add(forecastFile);
               var savedItem = uow.PersistAll();
               var item = savedItem.FirstOrDefault();
               if (item != null) result = extractId(item.Root);
            }
            return result;
        }

        private static Guid? extractId(object root)
        {
            var entity = root as IEntity;
            if (entity != null) return entity.Id.GetValueOrDefault();

            var custom = root as ICustomChangedEntity;
            if (custom != null) return custom.Id.GetValueOrDefault();

            return null;
        }
    }
}
