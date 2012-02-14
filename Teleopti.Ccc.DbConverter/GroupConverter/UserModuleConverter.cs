using System;
using System.Collections.Generic;
using Infrastructure;
using log4net;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using DefaultAggregateRoot=Teleopti.Ccc.DatabaseConverter.DefaultAggregateRoot;

namespace Teleopti.Ccc.DBConverter.GroupConverter
{
    class UserModuleConverter : ModuleConverter
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UserModuleConverter));
        private readonly DefaultAggregateRoot _defaultAggregateRoot;

        public UserModuleConverter(MappedObjectPair mappedObjectPair, 
                                DateTimePeriod period, 
                                ICccTimeZoneInfo timeZoneInfo,
                                DefaultAggregateRoot defaultAggregateRoot)
            : base(mappedObjectPair, period, timeZoneInfo)
        {
            _defaultAggregateRoot = defaultAggregateRoot;
        }

        protected override IEnumerable<Type> DependedOn
        {
            get
            {
                return new Type[] {typeof(AgentModuleConverter)};
            }
        }

        protected override string ModuleName
        {
            get { return "User"; }
        }

        protected override void GroupConvert()
        {
            ConvertUsers();
        }

        private void ConvertUsers()
        {
            Logger.Info("Converting Users...");
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                PersonRepository personRep = new PersonRepository(uow);
                IList<IPerson> existingPersons = personRep.LoadAll();
                UserMapper userMapper = new UserMapper(MappedObjectPair, TimeZoneInfo, _defaultAggregateRoot.AgentRole,
                                                       _defaultAggregateRoot.AdministratorRole, existingPersons);
                UserConverter userConverter = new UserConverter(uow, userMapper);
                userConverter.ConvertAndPersist(new UserReader().GetAll().Values);
            }
        }
    }
}
