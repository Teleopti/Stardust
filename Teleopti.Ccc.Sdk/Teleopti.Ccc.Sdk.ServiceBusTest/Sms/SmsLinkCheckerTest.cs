using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Sdk.ServiceBus.SMS;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Sms
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms"), TestFixture]
	public class SmsLinkCheckerTest
	{
		private MockRepository _mocks;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IRepositoryFactory _repositoryFactory;
		private SmsLinkChecker _target;
		private IPerson _person;
		private ISettingDataRepository _rep;
		private IUnitOfWork _uow;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
			_repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
			_target = new SmsLinkChecker(_unitOfWorkFactory, _repositoryFactory);
			_uow = _mocks.StrictMock<IUnitOfWork>();
			_rep = _mocks.StrictMock<ISettingDataRepository>();
			_person = _mocks.StrictMock<IPerson>();
		}

		[Test]
		public void ShouldReturnEmptyIfNoColumnDefined()
		{
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
			Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(_uow)).Return(_rep);
			Expect.Call(_rep.FindValueByKey("SmsSettings", new SmsSettings())).Return(new SmsSettings()).IgnoreArguments();
			Expect.Call(_uow.Dispose);
			_mocks.ReplayAll();
			var no = _target.SmsMobileNumber(_person);
			Assert.That(no,Is.Empty);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnEmptyIfNoOptionalValues()
		{
			var settings = new SmsSettings{OptionalColumnId = Guid.NewGuid()};
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
			Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(_uow)).Return(_rep);
			Expect.Call(_rep.FindValueByKey("SmsSettings", new SmsSettings())).Return(settings).IgnoreArguments();
			Expect.Call(_person.OptionalColumnValueCollection).Return(
				new ReadOnlyCollection<IOptionalColumnValue>(new List<IOptionalColumnValue>()));
			Expect.Call(_uow.Dispose);
			_mocks.ReplayAll();
			var no = _target.SmsMobileNumber(_person);
			Assert.That(no, Is.Empty);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnEmptyIfNoOptionalValueMatches()
		{
			var parent = _mocks.StrictMock<IEntity>();
			var val = _mocks.StrictMock<IOptionalColumnValue>();
			
			var settings = new SmsSettings { OptionalColumnId = Guid.NewGuid() };
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
			Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(_uow)).Return(_rep);
			Expect.Call(_rep.FindValueByKey("SmsSettings", new SmsSettings())).Return(settings).IgnoreArguments();
			Expect.Call(_person.OptionalColumnValueCollection).Return(
				new ReadOnlyCollection<IOptionalColumnValue>(new List<IOptionalColumnValue>{val}));
			Expect.Call(val.Parent).Return(parent);
			Expect.Call(parent.Id).Return(Guid.NewGuid());
			Expect.Call(_uow.Dispose);
			_mocks.ReplayAll();
			var no = _target.SmsMobileNumber(_person);
			Assert.That(no, Is.Empty);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnValueWhenColumnMatches()
		{
			var parent = _mocks.StrictMock<IEntity>();
			var val = _mocks.StrictMock<IOptionalColumnValue>();
			var id = Guid.NewGuid();
			var settings = new SmsSettings { OptionalColumnId = id };
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
			Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(_uow)).Return(_rep);
			Expect.Call(_rep.FindValueByKey("SmsSettings", new SmsSettings())).Return(settings).IgnoreArguments();
			Expect.Call(_person.OptionalColumnValueCollection).Return(
				new ReadOnlyCollection<IOptionalColumnValue>(new List<IOptionalColumnValue> { val }));
			Expect.Call(val.Parent).Return(parent);
			Expect.Call(parent.Id).Return(id);
			Expect.Call(val.Description).Return("123456789");
			Expect.Call(_uow.Dispose);
			_mocks.ReplayAll();
			var no = _target.SmsMobileNumber(_person);
			Assert.That(no, Is.EqualTo("123456789"));
			_mocks.VerifyAll();
		}

	}

}