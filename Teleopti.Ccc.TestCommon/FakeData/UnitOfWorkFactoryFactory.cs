using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creates stubs of type IUnitOfWorkFactory
    /// </summary>
    public static class UnitOfWorkFactoryFactory
    {
        /// <summary>
        /// Creates the unit of work factory.
        /// </summary>
        /// <param name="name">The name of the factory.</param>
        /// <returns></returns>
        public static IUnitOfWorkFactory CreateUnitOfWorkFactory(string name)
        {
            return new UnitOfWorkFactoryStub(name);
        }

        private class UnitOfWorkFactoryStub : IUnitOfWorkFactory
        {
            private readonly string _name;

            internal UnitOfWorkFactoryStub(string name)
            {
                _name = name;
            }

            public IUnitOfWork CurrentUnitOfWork()
            {
                throw new NotImplementedException();
            }

	        public bool HasCurrentUnitOfWork()
	        {
		        throw new NotImplementedException();
	        }

	        public IAuditSetter AuditSetting
        	{
        		get { return null; }
        	}

	        public string ConnectionString { get; private set; }

					public IUnitOfWork CreateAndOpenUnitOfWork(TransactionIsolationLevel isolationLevel)
            {
                throw new NotImplementedException();
            }

	        public IUnitOfWork CreateAndOpenUnitOfWork(IInitiatorIdentifier initiator)
	        {
		        throw new NotImplementedException();
	        }

	        public IUnitOfWork CreateAndOpenUnitOfWork(IQueryFilter businessUnitFilter)
	        {
				throw new NotImplementedException();
			}

	        public IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork()
            {
                throw new NotImplementedException();
            }

            public string Name
            {
                get { return _name; }
            }

            public long? NumberOfLiveUnitOfWorks
            {
                get { return null; }
            }

            public void Dispose()
            {
            }
        }
    }
}