using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.PayrollExportPages;


namespace Teleopti.Ccc.WinCodeTest.Payroll.PayrollExportPages
{
	[TestFixture]
	public class PersonsSelectionPresenterTest
	{
		private PersonsSelectionPresenter _target;
		private MockRepository _mocks;
		private IPersonsSelectionView _view;
		private PersonsSelectionModel _model;
		private IRepositoryFactory _repositoryFactory;
		private IUnitOfWorkFactory _unitOfWorkFactory;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_view = _mocks.StrictMock<IPersonsSelectionView>();
			_repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
			_unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
			_model = new PersonsSelectionModel();
			_target = new PersonsSelectionPresenter(_view, _model, _repositoryFactory, _unitOfWorkFactory);
		}

		[Test]
		public void VerifyCanInitialize()
		{
			using (_mocks.Record())
			{
				_view.ApplicationFunction = _model.ApplicationFunction();
			}

			using (_mocks.Record())
			{
				_target.Initialize();
			}
		}





		[Test]
		public void ShouldAddSelectedPersonsToPayrollExport()
		{
			var uow = _mocks.StrictMock<IUnitOfWork>();
			var rep = _mocks.StrictMock<IPersonRepository>();
			var guid = new HashSet<Guid>();
			var person = _mocks.StrictMock<IPerson>();
			var persons = new List<IPerson> { person };
			var payrollExport = _mocks.StrictMock<IPayrollExport>();
			using (_mocks.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
				Expect.Call(_repositoryFactory.CreatePersonRepository(uow)).Return(rep);
				Expect.Call(rep.FindPeople(guid)).Return(persons);
				Expect.Call(payrollExport.ClearPersons);
				Expect.Call(() => payrollExport.AddPersons(persons));
				Expect.Call(uow.Dispose);
			}
			using (_mocks.Playback())
			{
				_target.SetSelectedPeopleToPayrollExport(payrollExport, guid);
			}
		}

		[Test]
		public void ShouldSetPeriodAndPersonToModel()
		{
			var person = _mocks.StrictMock<IPerson>();
			var date = new DateOnlyPeriod(2010, 9, 28, 2010, 10, 20);
			var payrollExport = _mocks.StrictMock<IPayrollExport>();
			using (_mocks.Record())
			{
				Expect.Call(payrollExport.Period).Return(date);
				Expect.Call(payrollExport.Persons).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { person }));
			}
			using (_mocks.Playback())
			{
				_target.PopulateModel(payrollExport);
				Assert.AreEqual(person, _model.SelectedPersons.Single());
				Assert.AreEqual(date, _model.SelectedPeriod);
			}
		}

		[Test]
		public void ShouldThrowOnPopulateModelGivenNullAsPayrollExport()
		{
			Assert.Throws<ArgumentNullException>(() => _target.PopulateModel(null));
		}

		[Test]
		public void ShouldThrowOnSetSelectedPeopleGivenNullAsPayrollExport()
		{
			Assert.Throws<ArgumentNullException>(() => _target.SetSelectedPeopleToPayrollExport(null, new HashSet<Guid>()));
		}


	}
}
