using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Persisters
{
	[TestFixture]
	public class ScheduleDictionaryPersisterTest
	{
		private MockRepository _mocks;
		private ScheduleDictionarySaver _target;
		private IScheduleRepository _scheduleRepository;
		private IScheduleDictionary _scheduleDictionary;
		private IUnitOfWork _unitOfWork;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new ScheduleDictionarySaver();

			_unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
			_scheduleRepository = _mocks.DynamicMock<IScheduleRepository>();
			_scheduleDictionary = _mocks.DynamicMock<IScheduleDictionary>();
		}

		[Test]
		public void ShouldPersistAdded()
		{
			_scheduleRepository = _mocks.StrictMock<IScheduleRepository>();

			var addedEntity = _mocks.DynamicMock<IPersistableScheduleData>();
			var addedEntityClone = _mocks.DynamicMock<IPersistableScheduleData>();
			var addedItem = new DifferenceCollectionItem<IPersistableScheduleData>(null, addedEntity);
			var differenceCollection = new DifferenceCollection<IPersistableScheduleData>() { addedItem };

			Expect.Call(addedEntity.Clone()).Return(addedEntityClone);
			Expect.Call(() => _scheduleRepository.Add(addedEntityClone));

			_mocks.ReplayAll();

			_target.MarkForPersist(_unitOfWork, _scheduleRepository, differenceCollection);

			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldPersistDeleted()
		{
			_scheduleRepository = _mocks.StrictMock<IScheduleRepository>();

			var deletedEntity = _mocks.DynamicMock<IPersistableScheduleData>();
			var deletedItem = new DifferenceCollectionItem<IPersistableScheduleData>(deletedEntity, null);
			var differenceCollection = new DifferenceCollection<IPersistableScheduleData> { deletedItem };

			Expect.Call(() => _scheduleRepository.Remove(deletedEntity));

			_mocks.ReplayAll();

			_target.MarkForPersist(_unitOfWork, _scheduleRepository, differenceCollection);

			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldPersistModified()
		{
			var modifiedEntity = new ScheduleDataStub {Id = Guid.NewGuid()};
			var modifiedItem = new DifferenceCollectionItem<IPersistableScheduleData>(modifiedEntity, modifiedEntity);
			var differenceCollection = new DifferenceCollection<IPersistableScheduleData>() { modifiedItem };

			_unitOfWork.Reassociate(modifiedEntity);
			Expect.Call(_unitOfWork.Merge<IPersistableScheduleData>(modifiedEntity)).Return(modifiedEntity);

			_mocks.ReplayAll();

			_target.MarkForPersist(_unitOfWork, _scheduleRepository, differenceCollection);

			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnModified()
		{
			var modifiedEntity = new ScheduleDataStub() { Id = Guid.NewGuid() };
			var modifiedItem = new DifferenceCollectionItem<IPersistableScheduleData>(modifiedEntity, modifiedEntity);
			var differenceCollection = new DifferenceCollection<IPersistableScheduleData>() { modifiedItem };
			var parameters = MockRepository.GenerateMock<IScheduleParameters>();
			var person = PersonFactory.CreatePerson();
			parameters.Stub(x => x.Person).Return(person);
			var scheduleRange = _mocks.DynamicMock<ScheduleRange>(_mocks.DynamicMock<IScheduleDictionary>(), parameters);

			_mocks.Record();

			Expect.Call(_scheduleDictionary[null]).Return(scheduleRange);
			Expect.Call(_scheduleRepository.Get(modifiedEntity.GetType(), modifiedEntity.Id.Value)).Return(modifiedEntity);
			Expect.Call(_unitOfWork.Merge<IPersistableScheduleData>(modifiedEntity)).Return(modifiedEntity);

			_mocks.ReplayAll();

			var result = _target.MarkForPersist(_unitOfWork, _scheduleRepository, differenceCollection);

			Assert.That(result.ModifiedEntities.Single(), Is.SameAs(modifiedEntity));
		}

		[Test]
		public void ShouldReturnAdded()
		{
			var addedEntity = _mocks.DynamicMock<IPersistableScheduleData>();
			var addedItem = new DifferenceCollectionItem<IPersistableScheduleData>(null, addedEntity);
			var differenceCollection = new DifferenceCollection<IPersistableScheduleData>() { addedItem };
			var parameters = MockRepository.GenerateMock<IScheduleParameters>();
			var person = PersonFactory.CreatePerson();
			parameters.Stub(x => x.Person).Return(person);
			var scheduleRangeDynamic = _mocks.DynamicMock<ScheduleRange>(_mocks.DynamicMock<IScheduleDictionary>(), parameters);

			Expect.Call(_scheduleDictionary[null]).Return(scheduleRangeDynamic);
			Expect.Call(addedEntity.Clone()).Return(addedEntity);
			Expect.Call(() => _scheduleRepository.Add(addedEntity));

			_mocks.ReplayAll();

			var result = _target.MarkForPersist(_unitOfWork, _scheduleRepository, differenceCollection);

			Assert.That(result.AddedEntities.Single(), Is.SameAs(addedEntity));
		}


		private class ScheduleDataStub : IPersistableScheduleData
		{
			public DateTimePeriod Period { get { throw new NotImplementedException(); } }

			public IPerson Person { get { throw new NotImplementedException(); } }

			public IScenario Scenario { get { throw new NotImplementedException(); } }

			public object Clone()
			{
				throw new NotImplementedException();
			}

			public bool BelongsToPeriod(IDateOnlyAsDateTimePeriod dateAndPeriod)
			{
				throw new NotImplementedException();
			}

		    public bool BelongsToPeriod(DateOnlyPeriod dateOnlyPeriod)
		    {
		        throw new NotImplementedException();
		    }

		    public bool BelongsToScenario(IScenario scenario)
			{
				throw new NotImplementedException();
			}

			public int? Version { get { throw new NotImplementedException(); } }

			public void SetVersion(int version)
			{
				throw new NotImplementedException();
			}

			public bool Equals(IEntity other)
			{
				throw new NotImplementedException();
			}

			public Guid? Id { get; set; }

			public void SetId(Guid? newId)
			{
				throw new NotImplementedException();
			}

			public void ClearId()
			{
				throw new NotImplementedException();
			}

			public IPerson UpdatedBy { get { throw new NotImplementedException(); } }

			public DateTime? UpdatedOn { get { throw new NotImplementedException(); } }

			public IAggregateRoot MainRoot { get { throw new NotImplementedException(); } }

			public string FunctionPath { get { throw new NotImplementedException(); } }

			public IPersistableScheduleData CreateTransient()
			{
				throw new NotImplementedException();
			}
		}

	}
}