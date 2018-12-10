using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AuditHistory;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.AuditHistory
{
    [TestFixture]
    public class AuditHistoryPresenterTest
    {
        private AuditHistoryPresenter _presenter;
        private IAuditHistoryView _view;
        private IAuditHistoryModel _model;
        private MockRepository _mocks;
        private IScheduleDay _scheduleDay;
        private IScheduleDay _otherScheduleDay;
        private IProjectionService _projectionService;
        private IProjectionService _otherProjectionService;
        private IVisualLayerCollection _visualLayerCollection;
        private IVisualLayerCollection _otherVisualLayerCollection;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IAuditHistoryView>();
            _model = _mocks.StrictMock<IAuditHistoryModel>();
            _presenter = new AuditHistoryPresenter(_view, _model);
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            _otherScheduleDay = _mocks.StrictMock<IScheduleDay>();
            _projectionService = _mocks.StrictMock<IProjectionService>();
            _otherProjectionService = _mocks.StrictMock<IProjectionService>();
            _visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            _otherVisualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Teleopti.Ccc.WinCode.Scheduling.AuditHistory.AuditHistoryPresenter")]
        [Test]
        public void ShouldGenerateExceptionOnNullView()
        {
            Assert.Throws<ArgumentNullException>(() => new AuditHistoryPresenter(null, _model));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Teleopti.Ccc.WinCode.Scheduling.AuditHistory.AuditHistoryPresenter")]
        [Test]
        public void ShouldGenerateExceptionOnNullModel()
        {
            Assert.Throws<ArgumentNullException>(() => new AuditHistoryPresenter(_view, null));
        }

        [Test]
        public void ShouldCloseView()
        {
            using(_mocks.Record())
            {
                Expect.Call(() => _view.CloseView());
            }

            using(_mocks.Playback())
            {
                _presenter.Close();
            }
        }

        [Test]
        public void ShouldRestore()
        {
            using(_mocks.Record())
            {
                Expect.Call(() => _model.SelectedScheduleDay = _scheduleDay);
                Expect.Call(() => _view.CloseView());
            }

            using(_mocks.Playback())
            {
                _presenter.Restore(_scheduleDay);
            }
        }

        [Test]
        public void ShouldLoadView()
        {
            using(_mocks.Record())
            {
                Expect.Call(() => _view.EnableView = false);
                Expect.Call(() => _view.ShowView());
                Expect.Call(() => _view.StartBackgroundWork(AuditHistoryDirection.InitializeAndFirst));
            }

            using(_mocks.Playback())
            {
                _presenter.Load();
            }
        }

        [Test]
        public void ShouldDoWorkFirst()
        {
            using(_mocks.Record())
            {
                Expect.Call(() => _model.First());
            }

            using(_mocks.Playback())
            {
                var args = new DoWorkEventArgs(AuditHistoryDirection.InitializeAndFirst);
                _presenter.DoWork(args);
            }
        }

        [Test]
        public void ShouldDoWorkNext()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _model.Earlier());
                Expect.Call(_model.CurrentPage).Return(9);
                Expect.Call(_model.NumberOfPages).Return(10);
            }

            using (_mocks.Playback())
            {
                var args = new DoWorkEventArgs(AuditHistoryDirection.Next);
                _presenter.DoWork(args);
            }
        }

        [Test]
        public void ShouldDoWorkPrevious()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _model.Later());
                Expect.Call(_model.CurrentPage).Return(2);
            }

            using (_mocks.Playback())
            {
                var args = new DoWorkEventArgs(AuditHistoryDirection.Previous);
                _presenter.DoWork(args);
            }
        }

        [Test]
        public void ShouldDoWorkCompleted()
        {
            using(_mocks.Record())
            {
               
                Expect.Call(_model.CurrentPage).Return(1).Repeat.Twice();
                Expect.Call(_model.NumberOfPages).Return(1).Repeat.Times(3);
                Expect.Call(() => _view.ShowDefaultCursor());
                Expect.Call(() => _view.EnableView = true);
                Expect.Call(() => _view.LinkLabelEarlierStatus = false);
                Expect.Call(() => _view.LinkLabelLaterStatus = false);
					Expect.Call(() => _view.LinkLabelEarlierVisibility = false);
					Expect.Call(() => _view.LinkLabelLaterVisibility = false);
					Expect.Call(() => _view.RefreshGrid());
                Expect.Call(() => _view.UpdateHeaderText());
                Expect.Call(() => _view.UpdatePageOfStatusText());
                Expect.Call(() => _view.SelectFirstRowOnGrid());
                Expect.Call(() => _view.SetRestoreButtonStatus());
            }

            using(_mocks.Playback())
            {
                var args = new RunWorkerCompletedEventArgs(null,null,false);
                _presenter.WorkCompleted(args);
            }
        }

		[Test]
		public void ShouldShowPagingButtonsWhenMoreThanOnePage()
		{
			using (_mocks.Record())
			{

				Expect.Call(_model.CurrentPage).Return(1).Repeat.Twice();
				Expect.Call(_model.NumberOfPages).Return(3).Repeat.Times(3);
				Expect.Call(() => _view.ShowDefaultCursor());
				Expect.Call(() => _view.EnableView = true);
				Expect.Call(() => _view.LinkLabelEarlierStatus = true);
				Expect.Call(() => _view.LinkLabelLaterStatus = false);
				Expect.Call(() => _view.LinkLabelEarlierVisibility = true);
				Expect.Call(() => _view.LinkLabelLaterVisibility = true);
				Expect.Call(() => _view.RefreshGrid());
				Expect.Call(() => _view.UpdateHeaderText());
				Expect.Call(() => _view.UpdatePageOfStatusText());
				Expect.Call(() => _view.SelectFirstRowOnGrid());
				Expect.Call(() => _view.SetRestoreButtonStatus());
			}

			using (_mocks.Playback())
			{
				var args = new RunWorkerCompletedEventArgs(null, null, false);
				_presenter.WorkCompleted(args);
			}
		}

		[Test]
        public void ShouldCloseOnDataSourceException()
        {
            var exception = new DataSourceException();

            using(_mocks.Record())
            {
                Expect.Call(()=>_view.ShowDataSourceException(exception));
                Expect.Call(()=>_view.CloseView());
            }

            using(_mocks.Playback())
            {
                var args = new RunWorkerCompletedEventArgs(null, exception, true);
                _presenter.WorkCompleted(args);
            }
        }

        [Test]
        public void ShouldStartBackgroundWork()
        {
            using(_mocks.Record())
            {
                Expect.Call(() => _view.EnableView = false);
                Expect.Call(() => _view.ShowWaitCursor());
                Expect.Call(() => _view.StartBackgroundWork(AuditHistoryDirection.Next));
            }

            using(_mocks.Playback())
            {
                _presenter.StartBackgroundWork(AuditHistoryDirection.Next);
            }
        }

        [Test]
        public void ShouldStartBackgroundWorkEarlier()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _view.EnableView = false);
                Expect.Call(() => _view.ShowWaitCursor());
                Expect.Call(() => _view.StartBackgroundWork(AuditHistoryDirection.Next));
            }

            using (_mocks.Playback())
            {
                _presenter.LinkLabelEarlierClicked();
            }    
        }

        [Test]
        public void ShouldStartBackgroundWorkLater()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _view.EnableView = false);
                Expect.Call(() => _view.ShowWaitCursor());
                Expect.Call(() => _view.StartBackgroundWork(AuditHistoryDirection.Previous));
            }

            using (_mocks.Playback())
            {
                _presenter.LinkLabelLaterClicked();
            }
        }

        [Test]
        public void ShouldReturnColCunt1()
        {
            Assert.AreEqual(1, _presenter.GridQueryColCount());
        }

        [Test]
        public void ShouldReturnDoubleFontHeightOnRowIndexGreaterThan0()
        {
            Assert.AreEqual(10, _presenter.GridQueryRowHeight(1, 10, 5,2));       
        }

        [Test]
        public void ShouldReturnInHeightWhenRowIndexEqualsZero()
        {
            Assert.AreEqual(5, _presenter.GridQueryRowHeight(0, 5, 5,2));   
        }

        [Test]
        public void ShouldReturnColWidth200OnHeader()
        {
            Assert.AreEqual(200, _presenter.GridQueryColWidth(0, 1000));   
        }

        [Test]
        public void ShouldReturnSizeMinusColHeaderOnFirstCol()
        {
            Assert.AreEqual(800, _presenter.GridQueryColWidth(1, 1000));
        }

        [Test]
        public void ShouldReturnRowCount()
        {
            var listVisionDisplayRow = new List<RevisionDisplayRow> {new RevisionDisplayRow()};

            using (_mocks.Record())
            {
                Expect.Call(_model.PageRows).Return(listVisionDisplayRow);
            }

            using (_mocks.Playback())
            {
                Assert.AreEqual(1, _presenter.GridQueryRowCount());
            }
        }

        [Test]
        public void ShouldReturnDefaultPeriodWhenNoRevisionsInList()
        {
            var scheduleDayPeriod = new DateTimePeriod(2011, 11, 11, 2011, 11, 11);
            var start = scheduleDayPeriod.StartDateTime.AddHours(7);
            var end = start.AddHours(11);
            var expectedPeriod = new DateTimePeriod(start, end);

            using(_mocks.Record())
            {
                Expect.Call(_model.PageRows)
                    .Return(new List<RevisionDisplayRow>());
                Expect.Call(_model.CurrentScheduleDay)
                    .Return(_scheduleDay);
                Expect.Call(_scheduleDay.Period)
                    .Return(scheduleDayPeriod);

            }

            using(_mocks.Playback())
            {
                var period = _presenter.MergedOrDefaultPeriod();
                Assert.AreEqual(expectedPeriod, period);
            }
        }

        [Test]
        public void ShouldReturnMergedPeriod()
        {
            var startFirst = new DateTime(2011, 11, 11, 11, 0, 0, DateTimeKind.Utc);
            var endFirst = new DateTime(2011, 11, 11, 15, 30, 0, DateTimeKind.Utc);
            var periodFirst = new DateTimePeriod(startFirst, endFirst);

            var startSecond = new DateTime(2011, 11, 11, 10, 30, 0, DateTimeKind.Utc);
            var endSecond = new DateTime(2011, 11, 11, 12, 0, 0, DateTimeKind.Utc);
            var periodSecond = new DateTimePeriod(startSecond, endSecond);

            var expectedStart = new DateTime(2011, 11, 11, 9, 0, 0, DateTimeKind.Utc);
            var expectedEnd = new DateTime(2011, 11, 11, 17, 0, 0, DateTimeKind.Utc);
            var expectedPeriod = new DateTimePeriod(expectedStart, expectedEnd);

            var revisionDisplayRowA = new RevisionDisplayRow {ScheduleDay = _scheduleDay};
            var revisionDisplayRowB = new RevisionDisplayRow {ScheduleDay = _otherScheduleDay};

   
            using (_mocks.Record())
            {
                Expect.Call(_model.PageRows).Return(new List<RevisionDisplayRow>{revisionDisplayRowA, revisionDisplayRowB});
                Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.Period()).Return(periodFirst);

                Expect.Call(_otherScheduleDay.ProjectionService()).Return(_otherProjectionService);
                Expect.Call(_otherProjectionService.CreateProjection()).Return(_otherVisualLayerCollection);
                Expect.Call(_otherVisualLayerCollection.Period()).Return(periodSecond);

            }

            using (_mocks.Playback())
            {
                var period = _presenter.MergedOrDefaultPeriod();
                Assert.AreEqual(expectedPeriod, period);
            }    
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void GridQueryCellInfoShouldNotAlterDefaultCellTypeIfNoPageRows()
		{
			var eventArgs = new GridQueryCellInfoEventArgs(1, 1, new GridStyleInfo());
			using (_mocks.Record())
			{
				Expect.Call(_model.PageRows).Return(new List<RevisionDisplayRow>());
			}

			using (_mocks.Playback())
			{
				_presenter.GridQueryCellInfo(this, eventArgs);
			}

			Assert.AreEqual("TextBox", eventArgs.Style.CellType.ToString(CultureInfo.InvariantCulture));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void GridQueryCellInfoShouldNotAlterDefaultCellTypeIfSomething()
		{
			var eventArgs = new GridQueryCellInfoEventArgs(7, 1, new GridStyleInfo());
			var revDisplayRow = new RevisionDisplayRow();
			using (_mocks.Record())
			{
				Expect.Call(_model.PageRows).Return(new List<RevisionDisplayRow>{ revDisplayRow}).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				_presenter.GridQueryCellInfo(this, eventArgs);
			}

			Assert.AreEqual("TextBox", eventArgs.Style.CellType.ToString(CultureInfo.InvariantCulture));
		}

    }
}
