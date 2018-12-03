using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.TeamSchedule;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler;


namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule.Core.AbsenceHandler
{
	[TestFixture]
	class AbsencePersisterTest
	{
		private IAbsenceCommandConverter _absenceCommandConverter;
		private IPersonAbsenceCreator _personAbsenceCreator;
		private AbsenceCreatorInfo _absenceCreatorInfo;
		private IPermissionChecker _permissionChecker;

		[SetUp]
		public void SetUp()
		{
			_absenceCommandConverter = MockRepository.GenerateMock<IAbsenceCommandConverter>();
			_personAbsenceCreator = MockRepository.GenerateMock<IPersonAbsenceCreator>();
			_permissionChecker = MockRepository.GenerateMock<IPermissionChecker>();

			_absenceCreatorInfo = new AbsenceCreatorInfo()
			{
				Person = new Person()
			};
		}

		[Test]
		public void ShouldPersistFullDayAbsence()
		{
			var command = new AddFullDayAbsenceCommand();
			_absenceCommandConverter.Stub(x => x.GetCreatorInfoForFullDayAbsence(command)).Return(_absenceCreatorInfo);

			var target = new AbsencePersister(_absenceCommandConverter, _personAbsenceCreator, _permissionChecker);
			var result = target.PersistFullDayAbsence(command);

			_personAbsenceCreator.AssertWasCalled(x=>x.Create(_absenceCreatorInfo, true));
			result.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnErrorWhenNoPermissonForAddFullDayAbsence()
		{
			var expectedErrorMessage = "no permisson for add full day absence";
			var command = new AddFullDayAbsenceCommand();
			_absenceCommandConverter.Stub(x => x.GetCreatorInfoForFullDayAbsence(command)).Return(_absenceCreatorInfo);
			_permissionChecker.Stub(x => x.CheckAddFullDayAbsenceForPerson(_absenceCreatorInfo.Person, new DateOnly())).IgnoreArguments().Return(expectedErrorMessage);

			var target = new AbsencePersister(_absenceCommandConverter, _personAbsenceCreator, _permissionChecker);
			var result = target.PersistFullDayAbsence(command);

			result.ErrorMessages[0].Should().Be.EqualTo(expectedErrorMessage);
		}
		
		[Test]
		public void ShouldReturnFailResultWhenCreateFullDayAbsence()
		{
			var command = new AddFullDayAbsenceCommand();
			var createResult = new List<string>() { "add absence fail" };
			_absenceCommandConverter.Stub(x => x.GetCreatorInfoForFullDayAbsence(command)).Return(_absenceCreatorInfo);
			_personAbsenceCreator.Stub(x => x.Create(_absenceCreatorInfo, true)).Return(createResult);

			var target = new AbsencePersister(_absenceCommandConverter, _personAbsenceCreator, _permissionChecker);
			var result = target.PersistFullDayAbsence(command);

			result.PersonId.Should().Be.EqualTo(_absenceCreatorInfo.Person.Id.GetValueOrDefault());
			result.ErrorMessages.Should().Be.Equals(createResult);
		}

		[Test]
		public void ShouldPersistIntradayAbsence()
		{
			var command = new AddIntradayAbsenceCommand();
			_absenceCommandConverter.Stub(x => x.GetCreatorInfoForIntradayAbsence(command)).Return(_absenceCreatorInfo);

			var target = new AbsencePersister(_absenceCommandConverter, _personAbsenceCreator, _permissionChecker);
			var result = target.PersistIntradayAbsence(command);

			_personAbsenceCreator.AssertWasCalled(x => x.Create(_absenceCreatorInfo, false));
			result.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnErrorWhenNoPermissonForAddIntradayAbsence()
		{
			var expectedErrorMessage = "no permisson for add intraday absence";
			var command = new AddIntradayAbsenceCommand();
			_absenceCommandConverter.Stub(x => x.GetCreatorInfoForIntradayAbsence(command)).Return(_absenceCreatorInfo);
			_permissionChecker.Stub(x => x.CheckAddIntradayAbsenceForPerson(_absenceCreatorInfo.Person, new DateOnly())).IgnoreArguments().Return(expectedErrorMessage);

			var target = new AbsencePersister(_absenceCommandConverter, _personAbsenceCreator, _permissionChecker);
			var result = target.PersistIntradayAbsence(command);

			result.ErrorMessages[0].Should().Be.EqualTo(expectedErrorMessage);
		}

		[Test]
		public void ShouldReturnFailResultWhenCreateIntradayAbsence()
		{
			var command = new AddIntradayAbsenceCommand();
			var createResult = new List<string>() { "add absence fail" };
			_absenceCommandConverter.Stub(x => x.GetCreatorInfoForIntradayAbsence(command)).Return(_absenceCreatorInfo);
			_personAbsenceCreator.Stub(x => x.Create(_absenceCreatorInfo, false)).Return(createResult);

			var target = new AbsencePersister(_absenceCommandConverter, _personAbsenceCreator, _permissionChecker);
			var result = target.PersistIntradayAbsence(command);

			result.PersonId.Should().Be.EqualTo(_absenceCreatorInfo.Person.Id.GetValueOrDefault());
			result.ErrorMessages.Should().Be.Equals(createResult);
		}
	}
}
