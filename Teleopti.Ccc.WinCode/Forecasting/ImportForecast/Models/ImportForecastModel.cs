using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models
{
    public class ImportForecastModel
    {
        private readonly ISkill _skill;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;

        public ImportForecastModel(ISkill preselectedSkill, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _skill = preselectedSkill;
            _repositoryFactory = repositoryFactory;
            _unitOfWorkFactory = unitOfWorkFactory;
        }


        public IEnumerable<IWorkload> LoadWorkloadList()
        {
            return _skill.WorkloadCollection.OrderBy(wl => wl.Name).ToList();
        }

        public string GetSelectedSkillName()
        {
            return _skill.Name;
        }

        public Guid SaveForecastFileInDb(string fileName, byte[] fileContent)
        {
            Guid temp = new Guid();
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
               var forecastFile = new ForecastFile(fileName, fileContent);
               var importForecastRepository = new ImportForecastRepository(uow);
               importForecastRepository.Add(forecastFile);
               IEnumerable<IRootChangeInfo> savedItem = uow.PersistAll();
               var item = savedItem.FirstOrDefault();
               if (item != null)temp = extractId(item.Root);

            }
            return temp;
        }

        public IForecastFile GetForecastFileFromDb(Guid id)
        {
            IForecastFile forecastFile;
            
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var importForecastRepository = new ImportForecastRepository(uow);
                forecastFile = importForecastRepository.LoadForecastFileFromDb(id);
            }

            return forecastFile;
        }

        private Guid extractId(object root)
        {
            var entity = root as IEntity;
            if (entity != null) return entity.Id.GetValueOrDefault();

            var custom = root as ICustomChangedEntity;
            if (custom != null) return custom.Id.GetValueOrDefault();

            return Guid.Empty;
        }
    }
}
