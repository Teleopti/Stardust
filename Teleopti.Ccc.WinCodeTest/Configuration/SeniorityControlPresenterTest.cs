using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.WinCode.Common.Configuration;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;


namespace Teleopti.Ccc.WinCodeTest.Configuration
{
	[TestFixture]
	public class SeniorityControlPresenterTest
	{
		private SeniorityControlPresenter _target;
		private ISeniorityWorkDayRanks _model;
		private ISeniorityControlView _view;
		private IRepository<ISeniorityWorkDayRanks> _repository;
		private IList<ISeniorityWorkDayRanks> _seniorityWorkDayRanks;
		private IUnitOfWork _unitOfWork;
		
		[SetUp]
		public void SetUp()
		{
			_view = MockRepository.GenerateMock<ISeniorityControlView>();
			_repository = MockRepository.GenerateMock<IRepository<ISeniorityWorkDayRanks>>();
			_unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			_model = new SeniorityWorkDayRanks();
			_target = new SeniorityControlPresenter(_view, _repository);
			_seniorityWorkDayRanks = new List<ISeniorityWorkDayRanks>{_model};	
		}

	
		[Test]
		public void ShouldReturnListWithSeniorityWorkDays()
		{
			_repository.Stub(x => x.LoadAll()).Return(_seniorityWorkDayRanks);
			_target.Initialize();
			var result = _target.SeniorityWorkDays();
			Assert.AreEqual(7, result.Count);
		}

		[Test]
		public void ShouldPersistNewEntity()
		{
			_repository.Stub(x => x.LoadAll()).Return(_seniorityWorkDayRanks);
			_target.Initialize();

			_repository.Stub(x => x.UnitOfWork).Return(_unitOfWork);
			_repository.Stub(x => x.Add(_model));
			_unitOfWork.Stub(x => x.PersistAll());

			_target.Persist();
		}

		[Test]
		public void ShouldPersistExistingEntity()
		{
			_model.SetId(Guid.NewGuid());

			_repository.Stub(x => x.LoadAll()).Return(_seniorityWorkDayRanks);
			_target.Initialize();

			_repository.Stub(x => x.UnitOfWork).Return(_unitOfWork);
			_unitOfWork.Stub(x => x.Merge(_model));
			_unitOfWork.Stub(x => x.PersistAll()).Return(new List<IRootChangeInfo>());

			_target.Persist();	
		}

		[Test]
		public void ShouldUnload()
		{
			_repository.Stub(x => x.LoadAll()).Return(_seniorityWorkDayRanks);
			_target.Initialize();

			_target.Unload();
			Assert.IsEmpty(_target.SeniorityWorkDays());
		}

		[Test]
		public void ShouldMoveSeniorityWorkDayTop()
		{
			_repository.Stub(x => x.LoadAll()).Return(_seniorityWorkDayRanks);
			_target.Initialize();

			_view.Stub(x => x.RefreshListBoxWorkingDays(0));
			_target.MoveTop(6);

			Assert.AreEqual(1, _model.Sunday);
			Assert.AreEqual(2, _model.Monday);
			Assert.AreEqual(3, _model.Tuesday);
			Assert.AreEqual(4, _model.Wednesday);
			Assert.AreEqual(5, _model.Thursday);
			Assert.AreEqual(6, _model.Friday);
			Assert.AreEqual(7, _model.Saturday);
		}

		[Test]
		public void ShouldMoveSeniorityWorkDayBottom()
		{
			_repository.Stub(x => x.LoadAll()).Return(_seniorityWorkDayRanks);
			_target.Initialize();

			_view.Stub(x => x.RefreshListBoxWorkingDays(6));
			_target.MoveBottom(0);

			Assert.AreEqual(1, _model.Tuesday);
			Assert.AreEqual(2, _model.Wednesday);
			Assert.AreEqual(3, _model.Thursday);
			Assert.AreEqual(4, _model.Friday);
			Assert.AreEqual(5, _model.Saturday);
			Assert.AreEqual(6, _model.Sunday);
			Assert.AreEqual(7, _model.Monday);
		}

		[Test]
		public void ShouldMoveSeniorWorkDayUp()
		{
			_repository.Stub(x => x.LoadAll()).Return(_seniorityWorkDayRanks);
			_target.Initialize();

			_view.Stub(x => x.RefreshListBoxWorkingDays(5));
			_target.MoveUp(6);

			Assert.AreEqual(1, _model.Monday);
			Assert.AreEqual(2, _model.Tuesday);
			Assert.AreEqual(3, _model.Wednesday);
			Assert.AreEqual(4, _model.Thursday);
			Assert.AreEqual(5, _model.Friday);
			Assert.AreEqual(6, _model.Sunday);
			Assert.AreEqual(7, _model.Saturday);
		}

		[Test]
		public void ShouldMoveSeniorWorkDayDown()
		{
			_repository.Stub(x => x.LoadAll()).Return(_seniorityWorkDayRanks);
			_target.Initialize();

			_view.Stub(x => x.RefreshListBoxWorkingDays(1));
			_target.MoveDown(0);	

			Assert.AreEqual(1, _model.Tuesday);
			Assert.AreEqual(2, _model.Monday);
			Assert.AreEqual(3, _model.Wednesday);
			Assert.AreEqual(4, _model.Thursday);
			Assert.AreEqual(5, _model.Friday);
			Assert.AreEqual(6, _model.Saturday);
			Assert.AreEqual(7, _model.Sunday);
		}
	}
}
