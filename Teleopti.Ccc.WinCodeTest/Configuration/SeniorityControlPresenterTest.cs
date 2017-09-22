using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.TestCommon.FakeData;

#pragma warning disable 618


namespace Teleopti.Ccc.WinCodeTest.Configuration
{
	[TestFixture]
	public class SeniorityControlPresenterTest
	{
		private SeniorityControlPresenter _target;
		private ISeniorityWorkDayRanks _modelWorkDayRanks;
		private ISeniorityControlView _view;
		private ISeniorityWorkDayRanksRepository _repositoryWorkDay;
		private IRepository<IShiftCategory> _repositoryShiftCategory;
		private IList<ISeniorityWorkDayRanks> _seniorityWorkDayRanks;
		private IShiftCategory _shiftCategory1;
		private IShiftCategory _shiftCategory2;
		private IUnitOfWork _unitOfWork;
		
		[SetUp]
		public void SetUp()
		{
			_view = MockRepository.GenerateMock<ISeniorityControlView>();
			_repositoryWorkDay = MockRepository.GenerateMock<ISeniorityWorkDayRanksRepository>();
			_repositoryShiftCategory = MockRepository.GenerateMock<IRepository<IShiftCategory>>();
	
			_shiftCategory1 = ShiftCategoryFactory.CreateShiftCategory("shiftCategory1");
			_shiftCategory1.SetId(Guid.NewGuid());
			_shiftCategory2 = ShiftCategoryFactory.CreateShiftCategory("shiftCategory2");
			_shiftCategory2.SetId(Guid.NewGuid());

			_unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			_modelWorkDayRanks = new SeniorityWorkDayRanks();
			_target = new SeniorityControlPresenter(_view, _repositoryWorkDay, _repositoryShiftCategory);
			_seniorityWorkDayRanks = new List<ISeniorityWorkDayRanks>{_modelWorkDayRanks};
		}

	
		[Test]
		public void ShouldReturnListModels()
		{
			_repositoryWorkDay.Stub(x => x.LoadAll()).Return(_seniorityWorkDayRanks);
			_repositoryWorkDay.Stub(x => x.UnitOfWork).Return(_unitOfWork);
			_unitOfWork.Stub(x => x.PersistAll());
			_repositoryShiftCategory.Stub(x => x.LoadAll()).Return(new List<IShiftCategory> { _shiftCategory1, _shiftCategory2 });
			_view.Stub(x => x.SetChangedInfo(_modelWorkDayRanks));
			_target.Initialize();

			var resultWorkDays = _target.SeniorityWorkDays();
			Assert.AreEqual(7, resultWorkDays.Count);

			var resultShiftCategory = _target.SeniorityShiftCategoryRanks();
			Assert.AreEqual(2, resultShiftCategory.Count);
			Assert.AreEqual(1, resultShiftCategory[0].Rank);
			Assert.AreEqual(2, resultShiftCategory[1].Rank);
		}

		
		[Test]
		public void ShouldPersist()
		{
			_repositoryWorkDay.Stub(x => x.LoadAll()).Return(_seniorityWorkDayRanks);
			_repositoryWorkDay.Stub(x => x.UnitOfWork).Return(_unitOfWork);
			_unitOfWork.Stub(x => x.PersistAll());
			_repositoryShiftCategory.Stub(x => x.LoadAll()).Return(new List<IShiftCategory> { _shiftCategory1, _shiftCategory2 });
			_view.Stub(x => x.SetChangedInfo(_modelWorkDayRanks));
			_target.Initialize();

			_repositoryWorkDay.Stub(x => x.UnitOfWork).Return(_unitOfWork);
			_repositoryWorkDay.Stub(x => x.Remove(_modelWorkDayRanks));
			_repositoryWorkDay.Stub(x => x.Add(_modelWorkDayRanks));
			_unitOfWork.Stub(x => x.PersistAll());

			_repositoryWorkDay.Stub(x => x.UnitOfWork).Return(_unitOfWork);
			_unitOfWork.Stub(x => x.Merge(_shiftCategory1)).Return(_shiftCategory1);
			_unitOfWork.Stub(x => x.Merge(_shiftCategory2)).Return(_shiftCategory2);

			_target.Persist();
		}

		[Test]
		public void ShouldUnload()
		{
			_repositoryWorkDay.Stub(x => x.LoadAll()).Return(_seniorityWorkDayRanks);
			_repositoryWorkDay.Stub(x => x.UnitOfWork).Return(_unitOfWork);
			_unitOfWork.Stub(x => x.PersistAll());
			_repositoryShiftCategory.Stub(x => x.LoadAll()).Return(new List<IShiftCategory> { _shiftCategory1, _shiftCategory2 });
			_view.Stub(x => x.SetChangedInfo(_modelWorkDayRanks));
			_target.Initialize();

			_target.Unload();
			Assert.IsEmpty(_target.SeniorityWorkDays());
			Assert.IsEmpty(_target.SeniorityShiftCategoryRanks());
		}

		[Test]
		public void ShouldMoveSeniorityShiftCategoryTop()
		{
			_repositoryWorkDay.Stub(x => x.LoadAll()).Return(_seniorityWorkDayRanks);
			_repositoryWorkDay.Stub(x => x.UnitOfWork).Return(_unitOfWork);
			_unitOfWork.Stub(x => x.PersistAll());
			_repositoryShiftCategory.Stub(x => x.LoadAll()).Return(new List<IShiftCategory> { _shiftCategory1, _shiftCategory2 });
			_view.Stub(x => x.SetChangedInfo(_modelWorkDayRanks));
			_target.Initialize();

			_view.Stub(x => x.RefreshListBoxShiftCategoryRank(0));
			_target.MoveTopShiftCategory(1);
			var result = _target.SeniorityShiftCategoryRanks();
			Assert.AreEqual(1, result[0].Rank);
			Assert.AreEqual(2, result[1].Rank);
			Assert.AreEqual(_shiftCategory2, result[0].ShiftCategory);
		
		}

		[Test]
		public void ShouldMoveSeniorityShiftCategoryBottom()
		{
			_repositoryWorkDay.Stub(x => x.LoadAll()).Return(_seniorityWorkDayRanks);
			_repositoryWorkDay.Stub(x => x.UnitOfWork).Return(_unitOfWork);
			_unitOfWork.Stub(x => x.PersistAll());
			_repositoryShiftCategory.Stub(x => x.LoadAll()).Return(new List<IShiftCategory> { _shiftCategory1, _shiftCategory2 });
			_view.Stub(x => x.SetChangedInfo(_modelWorkDayRanks));
			_target.Initialize();

			_view.Stub(x => x.RefreshListBoxShiftCategoryRank(1));
			_target.MoveBottomShiftCategory(0);
			var result = _target.SeniorityShiftCategoryRanks();
			Assert.AreEqual(1, result[0].Rank);
			Assert.AreEqual(2, result[1].Rank);
			Assert.AreEqual(_shiftCategory2, result[0].ShiftCategory);
		}

		[Test]
		public void ShouldMoveSeniorityShiftCategoryUp()
		{
			_repositoryWorkDay.Stub(x => x.LoadAll()).Return(_seniorityWorkDayRanks);
			_repositoryWorkDay.Stub(x => x.UnitOfWork).Return(_unitOfWork);
			_unitOfWork.Stub(x => x.PersistAll());
			_repositoryShiftCategory.Stub(x => x.LoadAll()).Return(new List<IShiftCategory> { _shiftCategory1, _shiftCategory2 });
			_view.Stub(x => x.SetChangedInfo(_modelWorkDayRanks));
			_target.Initialize();

			_view.Stub(x => x.RefreshListBoxShiftCategoryRank(1));
			_target.MoveUpShiftCategory(1);
			var result = _target.SeniorityShiftCategoryRanks();
			Assert.AreEqual(1, result[0].Rank);
			Assert.AreEqual(2, result[1].Rank);
			Assert.AreEqual(_shiftCategory2, result[0].ShiftCategory);
		}

		[Test]
		public void ShouldMoveSeniorityShiftCategoryDown()
		{
			_repositoryWorkDay.Stub(x => x.LoadAll()).Return(_seniorityWorkDayRanks);
			_repositoryWorkDay.Stub(x => x.UnitOfWork).Return(_unitOfWork);
			_unitOfWork.Stub(x => x.PersistAll());
			_repositoryShiftCategory.Stub(x => x.LoadAll()).Return(new List<IShiftCategory> { _shiftCategory1, _shiftCategory2 });
			_view.Stub(x => x.SetChangedInfo(_modelWorkDayRanks));
			_target.Initialize();

			_view.Stub(x => x.RefreshListBoxShiftCategoryRank(1));
			_target.MoveDownShiftCategory(0);
			var result = _target.SeniorityShiftCategoryRanks();
			Assert.AreEqual(1, result[0].Rank);
			Assert.AreEqual(2, result[1].Rank);
			Assert.AreEqual(_shiftCategory2, result[0].ShiftCategory);
		}

		[Test]
		public void ShouldMoveSeniorityWorkDayTop()
		{
			_repositoryWorkDay.Stub(x => x.LoadAll()).Return(_seniorityWorkDayRanks);
			_repositoryWorkDay.Stub(x => x.UnitOfWork).Return(_unitOfWork);
			_unitOfWork.Stub(x => x.PersistAll());
			_repositoryShiftCategory.Stub(x => x.LoadAll()).Return(new List<IShiftCategory> { _shiftCategory1, _shiftCategory2 });
			_view.Stub(x => x.SetChangedInfo(_modelWorkDayRanks));
			_target.Initialize();

			_view.Stub(x => x.RefreshListBoxWorkingDays(0));
			_target.MoveTopWorkDay(6);

			Assert.AreEqual(1, _modelWorkDayRanks.Sunday);
			Assert.AreEqual(2, _modelWorkDayRanks.Monday);
			Assert.AreEqual(3, _modelWorkDayRanks.Tuesday);
			Assert.AreEqual(4, _modelWorkDayRanks.Wednesday);
			Assert.AreEqual(5, _modelWorkDayRanks.Thursday);
			Assert.AreEqual(6, _modelWorkDayRanks.Friday);
			Assert.AreEqual(7, _modelWorkDayRanks.Saturday);
		}

		[Test]
		public void ShouldMoveSeniorityWorkDayBottom()
		{
			_repositoryWorkDay.Stub(x => x.LoadAll()).Return(_seniorityWorkDayRanks);
			_repositoryWorkDay.Stub(x => x.UnitOfWork).Return(_unitOfWork);
			_unitOfWork.Stub(x => x.PersistAll());
			_repositoryShiftCategory.Stub(x => x.LoadAll()).Return(new List<IShiftCategory> { _shiftCategory1, _shiftCategory2 });
			_view.Stub(x => x.SetChangedInfo(_modelWorkDayRanks));
			_target.Initialize();

			_view.Stub(x => x.RefreshListBoxWorkingDays(6));
			_target.MoveBottomWorkDay(0);

			Assert.AreEqual(1, _modelWorkDayRanks.Tuesday);
			Assert.AreEqual(2, _modelWorkDayRanks.Wednesday);
			Assert.AreEqual(3, _modelWorkDayRanks.Thursday);
			Assert.AreEqual(4, _modelWorkDayRanks.Friday);
			Assert.AreEqual(5, _modelWorkDayRanks.Saturday);
			Assert.AreEqual(6, _modelWorkDayRanks.Sunday);
			Assert.AreEqual(7, _modelWorkDayRanks.Monday);
		}

		[Test]
		public void ShouldMoveSeniorWorkDayUp()
		{
			_repositoryWorkDay.Stub(x => x.LoadAll()).Return(_seniorityWorkDayRanks);
			_repositoryWorkDay.Stub(x => x.UnitOfWork).Return(_unitOfWork);
			_unitOfWork.Stub(x => x.PersistAll());
			_repositoryShiftCategory.Stub(x => x.LoadAll()).Return(new List<IShiftCategory> { _shiftCategory1, _shiftCategory2 });
			_view.Stub(x => x.SetChangedInfo(_modelWorkDayRanks));
			_target.Initialize();

			_view.Stub(x => x.RefreshListBoxWorkingDays(5));
			_target.MoveUpWorkDay(6);

			Assert.AreEqual(1, _modelWorkDayRanks.Monday);
			Assert.AreEqual(2, _modelWorkDayRanks.Tuesday);
			Assert.AreEqual(3, _modelWorkDayRanks.Wednesday);
			Assert.AreEqual(4, _modelWorkDayRanks.Thursday);
			Assert.AreEqual(5, _modelWorkDayRanks.Friday);
			Assert.AreEqual(6, _modelWorkDayRanks.Sunday);
			Assert.AreEqual(7, _modelWorkDayRanks.Saturday);
		}

		[Test]
		public void ShouldMoveSeniorWorkDayDown()
		{
			_repositoryWorkDay.Stub(x => x.LoadAll()).Return(_seniorityWorkDayRanks);
			_repositoryWorkDay.Stub(x => x.UnitOfWork).Return(_unitOfWork);
			_unitOfWork.Stub(x => x.PersistAll());
			_repositoryShiftCategory.Stub(x => x.LoadAll()).Return(new List<IShiftCategory> { _shiftCategory1, _shiftCategory2 });
			_view.Stub(x => x.SetChangedInfo(_modelWorkDayRanks));
			_target.Initialize();

			_view.Stub(x => x.RefreshListBoxWorkingDays(1));
			_target.MoveDownWorkDay(0);	

			Assert.AreEqual(1, _modelWorkDayRanks.Tuesday);
			Assert.AreEqual(2, _modelWorkDayRanks.Monday);
			Assert.AreEqual(3, _modelWorkDayRanks.Wednesday);
			Assert.AreEqual(4, _modelWorkDayRanks.Thursday);
			Assert.AreEqual(5, _modelWorkDayRanks.Friday);
			Assert.AreEqual(6, _modelWorkDayRanks.Saturday);
			Assert.AreEqual(7, _modelWorkDayRanks.Sunday);
		}
	}
}
