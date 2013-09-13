using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Persisters {
	[TestFixture]
	public class ScheduleDictionaryConflictCollectorTest {
		private MockRepository _mocks;
		private ScheduleDictionaryConflictCollector _target;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRepository _scheduleRepository;
		private ILazyLoadingManager _lazyLoadingManager;
		private IOwnMessageQueue _ownMessageQueue;
		private IUnitOfWork _unitOfWork;
		private IPersonAssignmentRepository _personAssignmentRepository;

		[SetUp]
		public void Setup(){
			_mocks = new MockRepository();
			_unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
			_scheduleDictionary = _mocks.DynamicMock<IScheduleDictionary>();
			_scheduleRepository = _mocks.DynamicMock<IScheduleRepository>();
			_personAssignmentRepository = _mocks.DynamicMock<IPersonAssignmentRepository>();
			_lazyLoadingManager = _mocks.DynamicMock<ILazyLoadingManager>();
			_ownMessageQueue = _mocks.DynamicMock<IOwnMessageQueue>();
		}

		private void MakeTarget()
		{
			_target = new ScheduleDictionaryConflictCollector(_scheduleRepository, _personAssignmentRepository, _lazyLoadingManager, TimeZoneInfoFactory.UtcTimeZoneInfo());
		}

		[Test]
		public void ShouldInitializeLoadedConflictedEntityUsingLazyLoadingManager()
		{
			MakeTarget();

			var conflictingEntity = new ScheduleDataStub() { Id = Guid.NewGuid(), Version = 1 };
			var conflictingDatabaseEntity = new ScheduleDataStub() { Id = Guid.NewGuid(), Version = 2 };

			var conflictingDifference = new DifferenceCollectionItem<IPersistableScheduleData>(conflictingEntity, conflictingEntity);
			var differences = new DifferenceCollection<IPersistableScheduleData>();
			differences.Add(conflictingDifference);

			Expect.Call(_scheduleDictionary.Period).Return(new ScheduleDateTimePeriod(new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1))));
			Expect.Call(_scheduleDictionary.DifferenceSinceSnapshot()).Return(differences);
			Expect.Call(_scheduleRepository.UnitOfWork).Return(_unitOfWork);
			Expect.Call(_unitOfWork.DatabaseVersion(conflictingEntity)).Return(conflictingDatabaseEntity.Version);
			Expect.Call(_scheduleRepository.LoadScheduleDataAggregate(conflictingEntity.GetType(), conflictingEntity.Id.Value)).Return(conflictingDatabaseEntity);
			Expect.Call(() => _lazyLoadingManager.Initialize(conflictingDatabaseEntity.Person));
			Expect.Call(() => _lazyLoadingManager.Initialize(conflictingDatabaseEntity.UpdatedBy));

			_mocks.ReplayAll();

			_target.GetConflicts(_scheduleDictionary, _ownMessageQueue);

			_mocks.VerifyAll();

		}

		[Test]
		public void ShouldReassociateData()
		{
			MakeTarget();

			Expect.Call(() => _ownMessageQueue.ReassociateDataWithAllPeople());

			_mocks.ReplayAll();

			_target.GetConflicts(_scheduleDictionary, _ownMessageQueue);

			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnItemsWithDifferenceInDatabaseVersion()
		{
			MakeTarget();

			var conflictingEntity = new ScheduleDataStub() { Id = Guid.NewGuid(), Version = 1 };
			var conflictingDatabaseEntity = new ScheduleDataStub();

			var conflictingDifference = new DifferenceCollectionItem<IPersistableScheduleData>(conflictingEntity, conflictingEntity);
			var differences = new DifferenceCollection<IPersistableScheduleData>();
			differences.Add(conflictingDifference);

			Expect.Call(_scheduleDictionary.Period).Return(new ScheduleDateTimePeriod(new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1))));
			Expect.Call(_scheduleDictionary.DifferenceSinceSnapshot()).Return(differences);
			Expect.Call(_scheduleRepository.UnitOfWork).Return(_unitOfWork);
			Expect.Call(_unitOfWork.DatabaseVersion(conflictingEntity)).Return(conflictingDatabaseEntity.Version);
			Expect.Call(_scheduleRepository.LoadScheduleDataAggregate(conflictingEntity.GetType(), conflictingEntity.Id.Value)).Return(conflictingDatabaseEntity);

			_mocks.ReplayAll();

			var conflicts = _target.GetConflicts(_scheduleDictionary, _ownMessageQueue);

			_mocks.VerifyAll();

			Assert.That(conflicts.Count(), Is.EqualTo(1));
			Assert.That(conflicts.ElementAt(0).DatabaseVersion, Is.SameAs(conflictingDatabaseEntity));
			Assert.That(conflicts.ElementAt(0).ClientVersion, Is.EqualTo(conflictingDifference));
		}

		[Test]
		public void ShouldNotReturnItemsWithSameDatabaseVersion()
		{
			MakeTarget();

			var nonConflictingEntity = new ScheduleDataStub() { Id = Guid.NewGuid(), Version = 1 };

			var nonConflictingDifference = new DifferenceCollectionItem<IPersistableScheduleData>(nonConflictingEntity, nonConflictingEntity);
			var differences = new DifferenceCollection<IPersistableScheduleData>();
			differences.Add(nonConflictingDifference);

			Expect.Call(_scheduleDictionary.Period).Return(new ScheduleDateTimePeriod(new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1))));
			Expect.Call(_scheduleDictionary.DifferenceSinceSnapshot()).Return(differences);
			Expect.Call(_scheduleRepository.UnitOfWork).Return(_unitOfWork);
			Expect.Call(_unitOfWork.DatabaseVersion(nonConflictingEntity)).Return(nonConflictingEntity.Version);

			_mocks.ReplayAll();

			var conflicts = _target.GetConflicts(_scheduleDictionary, _ownMessageQueue);

			_mocks.VerifyAll();

			Assert.That(conflicts.Count(), Is.EqualTo(0));
		}

		[Test]
		public void ShouldReturnItemsWhereDatabaseEntityIsDeleted()
		{
			MakeTarget();

			var conflictingEntity = new ScheduleDataStub() { Id = Guid.NewGuid(), Version = 1 };

			var conflictingDifference = new DifferenceCollectionItem<IPersistableScheduleData>(conflictingEntity, conflictingEntity);
			var differences = new DifferenceCollection<IPersistableScheduleData>();
			differences.Add(conflictingDifference);

			Expect.Call(_scheduleDictionary.Period).Return(new ScheduleDateTimePeriod(new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1))));
			Expect.Call(_scheduleDictionary.DifferenceSinceSnapshot()).Return(differences);
			Expect.Call(_scheduleRepository.UnitOfWork).Return(_unitOfWork);
			Expect.Call(_unitOfWork.DatabaseVersion(conflictingEntity)).Return(null);

			_mocks.ReplayAll();

			var conflicts = _target.GetConflicts(_scheduleDictionary, _ownMessageQueue);

			_mocks.VerifyAll();

			Assert.That(conflicts.Count(), Is.EqualTo(1));
			Assert.That(conflicts.ElementAt(0).DatabaseVersion, Is.Null);
			Assert.That(conflicts.ElementAt(0).ClientVersion, Is.EqualTo(conflictingDifference));
		}

		private class ScheduleDataStub : IPersistableScheduleData
		{
			private readonly IPerson _person = new Person();
			private readonly IPerson _createdBy = new Person();
			private readonly IPerson _updateBy = new Person();

			public DateTimePeriod Period
			{
				get { throw new NotImplementedException(); }
			}

			public IPerson Person
			{
				get { return _person; }
			}

			public IScenario Scenario
			{
				get { throw new NotImplementedException(); }
			}

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

			public int? Version { get; set; }

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

			public IPerson CreatedBy
			{
				get { return _createdBy; }
			}

			public DateTime? CreatedOn
			{
				get { throw new NotImplementedException(); }
			}

			public IPerson UpdatedBy
			{
				get { return _updateBy; }
			}

			public DateTime? UpdatedOn
			{
				get { throw new NotImplementedException(); }
			}

			public IAggregateRoot MainRoot
			{
				get { throw new NotImplementedException(); }
			}

			public string FunctionPath
			{
				get { throw new NotImplementedException(); }
			}

			public IPersistableScheduleData CreateTransient()
			{
				throw new NotImplementedException();
			}
		}
	}

}