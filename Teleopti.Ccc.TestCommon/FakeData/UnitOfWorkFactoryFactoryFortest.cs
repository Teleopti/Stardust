using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creates stubs of type IUnitOfWorkFactory
    /// </summary>
    public static class UnitOfWorkFactoryFactoryForTest
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
               return new FakeUnitOfWork(new FakeStorage());
            }

	        public bool HasCurrentUnitOfWork()
	        {
		        return false;
	        }
	        
			public string ConnectionString { get; }
			
	        public IUnitOfWork CreateAndOpenUnitOfWork()
			{
				return new FakeUnitOfWork(new FakeStorage());
			}

	        public IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork()
            {
                throw new NotImplementedException();
            }

            public string Name => _name;

			public void Dispose()
            {
            }
        }
    }
}