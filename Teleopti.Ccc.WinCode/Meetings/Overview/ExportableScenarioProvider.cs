using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Meetings.Overview
{
    public interface IExportableScenarioProvider
    {
        IList<IScenario>  AllowedScenarios();
    }

    public class ExportableScenarioProvider : IExportableScenarioProvider
    {
        private readonly IMeetingOverviewViewModel _model;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private IEnumerable<IScenario> _allScenarios;

        public ExportableScenarioProvider(IMeetingOverviewViewModel model, IScenarioRepository scenarioRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _model = model;
            _scenarioRepository = scenarioRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public IList<IScenario>  AllowedScenarios()
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var tempScenarios = new List<IScenario>(allScenarios());
                
                unitOfWork.Reassociate(tempScenarios);
                
                var canModifyRestricted =
                    TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(
                        DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario);
                if (!canModifyRestricted)
                {
                    for (var i = tempScenarios.Count - 1; i > -1; i--)
                    {
                        if (tempScenarios[i].Restricted)
                            tempScenarios.RemoveAt(i);
                    }
                }
                tempScenarios.Remove(_model.CurrentScenario);
                return tempScenarios;
            }
        }

        private IEnumerable<IScenario> allScenarios()
        {
            return _allScenarios ?? (_allScenarios = _scenarioRepository.FindAllSorted());
        }
    }
}