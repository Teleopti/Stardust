using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
    public class QuickForecastScenarioProvider : IQuickForecastScenarioProvider
    {
        private readonly IScenarioRepository _scenarioRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        private IScenario _defaultScenario;
        private IList<IScenario> _allScenarios;
        private readonly object _lockObject = new object();

        public QuickForecastScenarioProvider(IScenarioRepository scenarioRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _scenarioRepository = scenarioRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public IScenario DefaultScenario
        {
            get { 
                VerifyInitialized();
                return _defaultScenario;
            }
        }

        private void VerifyInitialized()
        {
            if (_allScenarios!=null) return;

            lock (_lockObject)
            {
                using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
                {
                    _allScenarios = _scenarioRepository.FindAllSorted();
                    _defaultScenario = _allScenarios.FirstOrDefault(d => d.DefaultScenario);
                }
            }
        }

        public IEnumerable<IScenario> AllScenarios
        {
            get
            {
                VerifyInitialized();
                return _allScenarios;
            }
        }
    }
}