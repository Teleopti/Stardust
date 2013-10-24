using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Persisters
{
	[TestFixture]
	public class ScheduleDictionaryBatchPersisterTest
	{
		[Test]
		public void ShouldMarkAndPersistDiff()
		{
			var currUowFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var scheduleDictionaryPersister = MockRepository.GenerateMock<IScheduleDictionarySaver>();
			var target = new ScheduleDictionaryBatchPersister(currUowFactory, null, scheduleDictionaryPersister, null, null, null, null);

			var difference = new DifferenceCollection<IPersistableScheduleData>() { new DifferenceCollectionItem<IPersistableScheduleData>() };
			var scheduleDictionary = stubScheduleDictionary(difference);
			currUowFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
			uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);

			target.Persist(scheduleDictionary);

			scheduleDictionaryPersister.AssertWasCalled(x => x.MarkForPersist(uow, null, difference));
		}

		[Test]
		public void ShouldCreateOneTransactionPerModifiedRange()
		{
			var currUowFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var uow1 = MockRepository.GenerateMock<IUnitOfWork>();
			var uow2 = MockRepository.GenerateMock<IUnitOfWork>();
			var scheduleDictionaryPersister = MockRepository.GenerateMock<IScheduleDictionarySaver>();
			var target = new ScheduleDictionaryBatchPersister(currUowFactory, null, scheduleDictionaryPersister, null, null, null, null);

			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var ranges = new[] { MockRepository.GenerateMock<IScheduleRange>(), MockRepository.GenerateMock<IScheduleRange>() };
			var difference = new DifferenceCollection<IPersistableScheduleData>() { new DifferenceCollectionItem<IPersistableScheduleData>() };

			currUowFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
			uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow1).Repeat.Once();
			uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow2).Repeat.Once();
			scheduleDictionary.Stub(x => x.Values).Return(ranges);
			ranges[0].Stub(x => x.DifferenceSinceSnapshot(null)).Return(difference);
			ranges[1].Stub(x => x.DifferenceSinceSnapshot(null)).Return(difference);

			target.Persist(scheduleDictionary);

			uow1.AssertWasCalled(x => x.PersistAll(null));
			uow2.AssertWasCalled(x => x.PersistAll(null));
		}

		[Test]
		public void ShouldNotCreateTransactionIfNotModified()
		{
			var currUowFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var target = new ScheduleDictionaryBatchPersister(currUowFactory, null, null, null, null, null, null);
			var scheduleDictionary = stubScheduleDictionary(new DifferenceCollection<IPersistableScheduleData>());
			var range = MockRepository.GenerateMock<IScheduleRange>();
			var difference = new DifferenceCollection<IPersistableScheduleData>();

			currUowFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
			scheduleDictionary.Stub(x => x.Values).Return(new[] { range });
			range.Stub(x => x.DifferenceSinceSnapshot(null)).Return(difference);

			target.Persist(scheduleDictionary);

			uowFactory.AssertWasNotCalled(x => x.CreateAndOpenUnitOfWork());
		}

		[Test]
		public void ShouldReassociate()
		{
			var currUowFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var scheduleDictionaryPersister = MockRepository.GenerateMock<IScheduleDictionarySaver>();
			var reassociateData = MockRepository.GenerateMock<IReassociateDataForSchedules>();
			var data = new[] { new[] { MockRepository.GenerateMock<IAggregateRoot>(), MockRepository.GenerateMock<IAggregateRoot>() } };
			var target = new ScheduleDictionaryBatchPersister(currUowFactory, null, scheduleDictionaryPersister, null, null, reassociateData, null);

			var scheduleDictionary = stubScheduleDictionary();

			currUowFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
			reassociateData.Stub(x => x.DataToReassociate(scheduleDictionary.Values.Single().Person)).Return(data);
			uowFactory.Stub(x => x.CreateAndOpenUnitOfWork(data)).Return(uow);

			target.Persist(scheduleDictionary);

			uowFactory.AssertWasCalled(x => x.CreateAndOpenUnitOfWork(data));
		}

		[Test]
		public void ShouldCallbackOnModified() 
		{
			var currUowFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var callback = MockRepository.GenerateMock<IScheduleDictionaryModifiedCallback>();
			var scheduleDictionaryPersister = MockRepository.GenerateMock<IScheduleDictionarySaver>();
			var target = new ScheduleDictionaryBatchPersister(currUowFactory , null, scheduleDictionaryPersister, null, null, null, callback);
			var result = new ScheduleDictionaryPersisterResult();
			var scheduleDictionary = stubScheduleDictionary();
			var range = scheduleDictionary.Values.Single();

			currUowFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
			uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			scheduleDictionaryPersister.Stub(x => x.MarkForPersist(uow, null, null)).IgnoreArguments().Return(result);

			target.Persist(scheduleDictionary);

			callback.AssertWasCalled(x => x.Callback(range, result.ModifiedEntities, result.AddedEntities, result.DeletedEntities));
		}

		private static IScheduleDictionary stubScheduleDictionary(DifferenceCollection<IPersistableScheduleData> differenceCollection)
		{
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var range = MockRepository.GenerateMock<IScheduleRange>();
			var person = new Person();
			scheduleDictionary.Stub(x => x.Values).Return(new[] { range });
			range.Stub(x => x.DifferenceSinceSnapshot(null)).Return(differenceCollection);
			range.Stub(x => x.Person).Return(person);
			return scheduleDictionary;
		}

		private static IScheduleDictionary stubScheduleDictionary()
		{
			return stubScheduleDictionary(new DifferenceCollection<IPersistableScheduleData> {new DifferenceCollectionItem<IPersistableScheduleData>()});
		}
	}
}