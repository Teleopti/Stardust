using System;
using System.Collections.Generic;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBConverter.GroupConverter
{
    class SecurityModuleConverter : ModuleConverter
    {
        private readonly DefaultAggregateRoot _defaultAggregateRoot;
        private readonly ICollection<IApplicationFunction> _applicationFunctionsForAll = new List<IApplicationFunction>();
        private readonly ICollection<IApplicationFunction> _applicationFunctionsForAdministrators = new List<IApplicationFunction>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityModuleConverter"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="period">The period.</param>
        /// <param name="timeZoneInfo">The time zone info.</param>
        /// <param name="defaultAggregateRoot">The default aggregate root.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/5/2007
        /// </remarks>
        public SecurityModuleConverter(MappedObjectPair mappedObjectPair, DateTimePeriod period, TimeZoneInfo timeZoneInfo, DefaultAggregateRoot defaultAggregateRoot)
            : base(mappedObjectPair, period, timeZoneInfo)
        {
            _defaultAggregateRoot = defaultAggregateRoot;
        }

        protected override string ModuleName
        {
            get { return "Security module"; }
        }

        protected override IEnumerable<Type> DependedOn
        {
            get
            {
                return new List<Type>();
            }
        }

        /// <summary>
        /// Group convertion.
        /// </summary>
        protected override void GroupConvert()
        {
            AddApplicationFunctions();
            AssignApplicationFunctionsToRoles();
        }

        /// <summary>
        /// Assigns the application functions to roles.
        /// </summary>
        private void AssignApplicationFunctionsToRoles()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                uow.Reassociate(_defaultAggregateRoot.AdministratorRole);
                foreach (ApplicationFunction function in _applicationFunctionsForAdministrators)
                {
                    _defaultAggregateRoot.AdministratorRole.AddApplicationFunction(function);
                }

                uow.Reassociate(_defaultAggregateRoot.BusinessUnitAdministratorRole);
                foreach (ApplicationFunction function in _applicationFunctionsForAdministrators)
                {
                    _defaultAggregateRoot.BusinessUnitAdministratorRole.AddApplicationFunction(function);
                }

                uow.Reassociate(_defaultAggregateRoot.SiteManagerRole);
                foreach (ApplicationFunction function in _applicationFunctionsForAll)
                {
                    _defaultAggregateRoot.SiteManagerRole.AddApplicationFunction(function);
                }

                uow.Reassociate(_defaultAggregateRoot.TeamLeaderRole);
                foreach (ApplicationFunction function in _applicationFunctionsForAll)
                {
                    _defaultAggregateRoot.TeamLeaderRole.AddApplicationFunction(function);
                }

                uow.Reassociate(_defaultAggregateRoot.AgentRole);
                foreach (ApplicationFunction function in _applicationFunctionsForAll)
                {
                    _defaultAggregateRoot.AgentRole.AddApplicationFunction(function);
                }

                uow.PersistAll();
            }
        }

        private void AddApplicationFunctions()
        {

            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                bool itemFound;
                int level = -1;
                IApplicationFunctionRepository rep = new ApplicationFunctionRepository(uow);
                IList<IApplicationFunction> loadedApplicationFunctions = rep.GetAllApplicationFunctionSortedByCode();
                do
                {
                    level++;
                    itemFound = false;
                    foreach (IApplicationFunction applicationFunction in new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList)
                    {
                        if (applicationFunction.Level == level)
                        {
                            itemFound = true;
                            IApplicationFunction realApplicationFunction = ApplicationFunction.FindByPath(loadedApplicationFunctions, applicationFunction.FunctionPath);
                            if (realApplicationFunction == null)
                            {
                                realApplicationFunction = (IApplicationFunction) applicationFunction.Clone();
                                realApplicationFunction.Parent = null;
                                IApplicationFunction parentApplicationFunction = ApplicationFunction.FindByPath(loadedApplicationFunctions, ((IApplicationFunction) applicationFunction.Parent).FunctionPath);
                                parentApplicationFunction.AddChild(realApplicationFunction);

                                rep.Add(realApplicationFunction);
                            }
                            _applicationFunctionsForAdministrators.Add(realApplicationFunction);
                            if (level <= 1)
                                _applicationFunctionsForAll.Add(realApplicationFunction);
                        }
                    }
                    if(itemFound)
                        uow.PersistAll();
                } while (itemFound);
            }
        }
    }
}