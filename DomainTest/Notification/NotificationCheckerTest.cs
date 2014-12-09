using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Notification
{
	[TestFixture]
	public class NotificationCheckerTest
	{
		private MockRepository _mocks;
		private ICurrentUnitOfWork _unitOfWorkFactory;
		private IRepositoryFactory _repositoryFactory;
		private NotificationChecker _target;
		private IPerson _person;
		private ISettingDataRepository _rep;
		private IUnitOfWork _uow;
		private IToggleManager _toggleManager;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_unitOfWorkFactory = _mocks.StrictMock<ICurrentUnitOfWork>();
			_repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
			_toggleManager = _mocks.StrictMock<IToggleManager>();
			_target = new NotificationChecker(_unitOfWorkFactory, _repositoryFactory);
			_uow = _mocks.StrictMock<IUnitOfWork>();
			_rep = _mocks.StrictMock<ISettingDataRepository>();
			_person = _mocks.StrictMock<IPerson>();
		}

		[Test]
		public void ShouldReturnEmptyIfNoColumnDefinedForSms()
		{
			Expect.Call(_unitOfWorkFactory.Current()).Return(_uow);
			Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(_uow)).Return(_rep);
			Expect.Call(_rep.FindValueByKey("SmsSettings", new SmsSettings())).Return(new SmsSettings()).IgnoreArguments();
			Expect.Call(_uow.Dispose).Repeat.Never();
			_mocks.ReplayAll();
			var no = _target.SmsMobileNumber(_person);
			Assert.That(no,Is.Empty);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnEmptyIfNoOptionalValuesForSms()
		{
			var settings = new SmsSettings{OptionalColumnId = Guid.NewGuid()};
			Expect.Call(_unitOfWorkFactory.Current()).Return(_uow);
			Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(_uow)).Return(_rep);
			Expect.Call(_rep.FindValueByKey("SmsSettings", new SmsSettings())).Return(settings).IgnoreArguments();
			Expect.Call(_person.OptionalColumnValueCollection).Return(
				new ReadOnlyCollection<IOptionalColumnValue>(new List<IOptionalColumnValue>()));
			Expect.Call(_uow.Dispose).Repeat.Never();
			_mocks.ReplayAll();
			var no = _target.SmsMobileNumber(_person);
			Assert.That(no, Is.Empty);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnEmptyIfNoOptionalValueMatchesForSms()
		{
			var parent = _mocks.StrictMock<IEntity>();
			var val = _mocks.StrictMock<IOptionalColumnValue>();
			
			var settings = new SmsSettings { OptionalColumnId = Guid.NewGuid() };
            Expect.Call(_unitOfWorkFactory.Current()).Return(_uow);
			Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(_uow)).Return(_rep);
			Expect.Call(_rep.FindValueByKey("SmsSettings", new SmsSettings())).Return(settings).IgnoreArguments();
			Expect.Call(_person.OptionalColumnValueCollection).Return(
				new ReadOnlyCollection<IOptionalColumnValue>(new List<IOptionalColumnValue>{val}));
			Expect.Call(val.Parent).Return(parent);
			Expect.Call(parent.Id).Return(Guid.NewGuid());
			Expect.Call(_uow.Dispose).Repeat.Never();
			_mocks.ReplayAll();
			var no = _target.SmsMobileNumber(_person);
			Assert.That(no, Is.Empty);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnValueWhenColumnMatchesForSms()
		{
			var parent = _mocks.StrictMock<IEntity>();
			var val = _mocks.StrictMock<IOptionalColumnValue>();
			var id = Guid.NewGuid();
			var settings = new SmsSettings { OptionalColumnId = id };
            Expect.Call(_unitOfWorkFactory.Current()).Return(_uow);
			Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(_uow)).Return(_rep);
			Expect.Call(_rep.FindValueByKey("SmsSettings", new SmsSettings())).Return(settings).IgnoreArguments();
			Expect.Call(_person.OptionalColumnValueCollection).Return(
				new ReadOnlyCollection<IOptionalColumnValue>(new List<IOptionalColumnValue> { val }));
			Expect.Call(val.Parent).Return(parent);
			Expect.Call(parent.Id).Return(id);
			Expect.Call(val.Description).Return("123456789");
			Expect.Call(_uow.Dispose).Repeat.Never();
			_mocks.ReplayAll();
			var no = _target.SmsMobileNumber(_person);
			Assert.That(no, Is.EqualTo("123456789"));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldHaveSmsAsDefaultNotificationType()
		{
			Expect.Call(_unitOfWorkFactory.Current()).Return(_uow);
			Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(_uow)).Return(_rep);
			Expect.Call(_rep.FindValueByKey("SmsSettings", new SmsSettings())).Return(new SmsSettings()).IgnoreArguments();
			Expect.Call(_uow.Dispose).Repeat.Never();
			_mocks.ReplayAll();
			Assert.That(_target.NotificationType(), Is.EqualTo(NotificationType.Sms));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldHaveEmailNotificationType()
		{
			var emailSetting = new SmsSettings { NotificationSelection = NotificationType.Email };
			Expect.Call(_unitOfWorkFactory.Current()).Return(_uow);
			Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(_uow)).Return(_rep);
			Expect.Call(_rep.FindValueByKey("SmsSettings", new SmsSettings())).Return(emailSetting).IgnoreArguments();
			Expect.Call(_uow.Dispose).Repeat.Never();
			_mocks.ReplayAll();
			var type = _target.NotificationType();
			Assert.That(type, Is.EqualTo(emailSetting.NotificationSelection));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldHaveDefaultEmailFromAddress()
		{
			Expect.Call(_unitOfWorkFactory.Current()).Return(_uow);
			Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(_uow)).Return(_rep);
			Expect.Call(_rep.FindValueByKey("SmsSettings", new SmsSettings())).Return(new SmsSettings()).IgnoreArguments();
			Expect.Call(_uow.Dispose).Repeat.Never();
			_mocks.ReplayAll();
			Assert.That(_target.EmailSender, Is.EqualTo("no-reply@teleopti.com"));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldGetEmailSender()
		{
			var emailSetting = new SmsSettings {NotificationSelection = NotificationType.Email, EmailFrom = "ashley@andeen.com"};
			Expect.Call(_unitOfWorkFactory.Current()).Return(_uow);
			Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(_uow)).Return(_rep);
			Expect.Call(_rep.FindValueByKey("SmsSettings", new SmsSettings())).Return(emailSetting).IgnoreArguments();
			Expect.Call(_uow.Dispose).Repeat.Never();
			_mocks.ReplayAll();
			var sender = _target.EmailSender;
			Assert.That(sender, Is.EqualTo(emailSetting.EmailFrom));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldCallRepositoryOnlyOnce()
		{
			var emailSetting = new SmsSettings { NotificationSelection = NotificationType.Email, EmailFrom = "ashley@andeen.com" };
			Expect.Call(_unitOfWorkFactory.Current()).Return(_uow).Repeat.Once();
			Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(_uow)).Return(_rep).Repeat.Once();
			Expect.Call(_rep.FindValueByKey("SmsSettings", new SmsSettings())).Return(emailSetting).IgnoreArguments().Repeat.Once();
			Expect.Call(_uow.Dispose).Repeat.Never();
			_mocks.ReplayAll();
			var type = _target.NotificationType();
			var sender = _target.EmailSender;
			Assert.That(type, Is.EqualTo(emailSetting.NotificationSelection));
			Assert.That(sender, Is.EqualTo(emailSetting.EmailFrom));
			_mocks.VerifyAll();
		}
	}
}