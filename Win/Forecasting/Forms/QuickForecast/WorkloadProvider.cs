using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
    public class WorkloadProvider : IWorkloadProvider
    {
        private readonly IWorkloadRepository _workloadRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        
        private IEnumerable<IWorkload> _workloadCollection;
        private readonly object _lockObject = new object();
        
        public WorkloadProvider(IWorkloadRepository workloadRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _workloadRepository = workloadRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public IEnumerable<IWorkload> WorkloadCollection
        {
            get
            {
                VerifyInitialized();
                return _workloadCollection.OrderBy(w => w.Skill.Name).ThenBy(w => w.Name);
            }
        }

        private void VerifyInitialized()
        {
            if (_workloadCollection!=null) return;

            lock (_lockObject)
            {
                using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
                {
                    _workloadCollection = _workloadRepository.LoadAll();
                    foreach (IWorkload workload in _workloadCollection)
                    {
                        if (!LazyLoadingManager.IsInitialized(workload.Skill))
                        {
                            LazyLoadingManager.Initialize(workload.Skill);
                        }
                    }
                }
            }
        }
    }
}