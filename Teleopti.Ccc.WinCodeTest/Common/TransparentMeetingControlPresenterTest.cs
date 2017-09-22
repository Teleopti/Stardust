using System;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.WinCodeTest.Common
{
	[TestFixture]
	public class TransparentMeetingControlPresenterTest : IDisposable
	{
		private MockRepository _mockRepository;
		private TransparentMeetingControlPresenter _presenter;
		private ITransparentMeetingControlView _view;
		private TransparentMeetingControlModel _model;
		private ITransparentControlHelper _helper;
		private UserControl _parent;

		[SetUp]
		public void Setup()
		{
			_parent = new UserControl();
			_mockRepository = new MockRepository();
			_view = _mockRepository.StrictMock<ITransparentMeetingControlView>();
			_helper = _mockRepository.StrictMock<ITransparentControlHelper>();
			_model = new TransparentMeetingControlModel(0, 0, _parent, Color.Blue, 50)
			         	{
			         		BorderWidth = 3,
			         		LeftBorderColor = Color.Blue,
			         		RightBorderColor = Color.Red
			         	};
			_presenter = new TransparentMeetingControlPresenter(_view, _model, _helper);
		}
		
		[Test]
		public void Initialize()
		{
			using(_mockRepository.Record())
			{
				Expect.Call(() => _view.SetBorderWidth(_model.BorderWidth));
				Expect.Call(() => _view.SetWestBorderColor(_model.LeftBorderColor));
				Expect.Call(() => _view.SetEastBorderColor(_model.RightBorderColor));
				Expect.Call(() => _view.Position(_model));
				Expect.Call(() => _view.InvalidateParent());
			}
			using(_mockRepository.Playback())
			{
				_presenter.Initialize();
			}
		}

		[Test]
		public void ShouldNotMovePastMinLeftWhenMovingLeftBorder()
		{
			using(_mockRepository.Record())
			{
				Expect.Call(_helper.GetLeftPosition(1, 70)).Return(5);
				Expect.Call(() => _view.Position(_model));
				Expect.Call(() => _view.InvalidateParent());
			}

			using(_mockRepository.Playback())
			{
				_presenter.AllowMove = true;
				_model.Left = 50;
				_model.Width = 20;
				_presenter.OnMouseMoveBorderWest(-49);

				Assert.AreEqual(5, _model.Left);
				Assert.AreEqual(65, _model.Width);
			}
		}

		[Test]
		public void ShouldNotMovePastRightWhenMovingLeftBorder()
		{
			using (_mockRepository.Record())
			{
				Expect.Call(_helper.GetLeftPosition(80, 70)).Return(70);
				Expect.Call(() => _view.Position(_model));
				Expect.Call(() => _view.InvalidateParent());
			}

			using (_mockRepository.Playback())
			{
				_presenter.AllowMove = true;
				_model.Left = 50;
				_model.Width = 20;
				_presenter.OnMouseMoveBorderWest(30);

				Assert.AreEqual(70, _model.Left);
				Assert.AreEqual(0, _model.Width);
			}	
		}

		[Test]
		public void ShouldNotMovePastMaxRightWhenMovingRightBorder()
		{
			using (_mockRepository.Record())
			{
				Expect.Call(_helper.GetRightPosition(10, 100)).Return(45);
				Expect.Call(() => _view.Position(_model));
				Expect.Call(() => _view.InvalidateParent());
			}

			using(_mockRepository.Playback())
			{
				_presenter.AllowMove = true;
				_model.Left = 10;
				_model.Width = 30;
				_presenter.OnMouseMoveBorderEast(60);

				Assert.AreEqual(10, _model.Left);
				Assert.AreEqual(35, _model.Width);
			}
		}

		[Test]
		public void ShouldNotMoveControlPastMinLeft()
		{
			using (_mockRepository.Record())
			{
				Expect.Call(_helper.GetLeftPositionConsiderWidth(1, 20)).Return(5);
				Expect.Call(() => _view.Position(_model));
				Expect.Call(() => _view.InvalidateParent());
			}

			using (_mockRepository.Playback())
			{
				_presenter.AllowMove = true;
				_model.Left = 50;
				_model.Width = 20;
				_presenter.OnMoveMouse(-49);

				Assert.AreEqual(5, _model.Left);
				Assert.AreEqual(20, _model.Width);
			}
		}

		[Test]
		public void ShouldNotMoveControlPlusWidthPastMaxRight()
		{
			using (_mockRepository.Record())
			{
				Expect.Call(_helper.GetLeftPositionConsiderWidth(80, 20)).Return(70);
				Expect.Call(() => _view.Position(_model));
				Expect.Call(() => _view.InvalidateParent());
			}

			using (_mockRepository.Playback())
			{
				_presenter.AllowMove = true;
				_model.Left = 30;
				_model.Width = 20;
				_presenter.OnMoveMouse(50);

				Assert.AreEqual(70, _model.Left);
				Assert.AreEqual(20, _model.Width);
			}	
		}

		[Test]
		public void ShouldNotMovePastLeftBorderWhenMovingRightBorder()
		{
			using (_mockRepository.Record())
			{
				Expect.Call(_helper.GetRightPosition(10, 5)).Return(10);
				Expect.Call(() => _view.Position(_model));
				Expect.Call(() => _view.InvalidateParent());
			}

			using (_mockRepository.Playback())
			{
				_presenter.AllowMove = true;
				_model.Left = 10;
				_model.Width = 30;
				_presenter.OnMouseMoveBorderEast(-35);

				Assert.AreEqual(10, _model.Left);
				Assert.AreEqual(0, _model.Width);
			}	
		}

		[Test]
		public void ShouldGetTransparencyAndColorFromModel()
		{
			Assert.AreEqual(_model.Transparency, _presenter.Transparency);
			Assert.AreEqual(_model.BackColor, _presenter.BackColor);
		}

		[Test]
		public void ShouldAllowMoveWhenMouseDown()
		{
			_presenter.OnMouseDown(1);

			Assert.IsTrue(_presenter.AllowMove);	
		}

		[Test]
		public void ShouldNotAllowMoveWhenMouseUp()
		{
			using (_mockRepository.Record())
			{
				Expect.Call(_helper.GetSnappedPosition(0)).IgnoreArguments().Return(10).Repeat.Twice();
				Expect.Call(() => _view.Position(_model)).Repeat.Once();
				Expect.Call(() => _view.InvalidateParent()).Repeat.Twice();
			}

			using (_mockRepository.Playback())
			{
				_presenter.OnMouseUp(false, false);

				Assert.IsFalse(_presenter.AllowMove);
			}	
		}

		[Test]
		public void ShouldNotUpdateBackgroundWhenMouseDown()
		{
			_presenter.OnMouseDown(1);

			Assert.IsFalse(_presenter.UpdateBackground);
		}

		[Test]
		public void ShouldUpdateBackgroundWhenMouseUp()
		{
			using (_mockRepository.Record())
			{
				Expect.Call(_helper.GetSnappedPosition(0)).IgnoreArguments().Return(10).Repeat.Twice();
				Expect.Call(() => _view.Position(_model)).Repeat.Once();
				Expect.Call(() => _view.InvalidateParent()).Repeat.Twice();
			}

			using (_mockRepository.Playback())
			{
				_presenter.OnMouseUp(false, false);

				Assert.IsTrue(_presenter.UpdateBackground);
			}
		}

		[Test]
		public void  ShouldNotSnapLeftToSameAsRight()
		{
			_model.Left = 10;
			_model.Width = 10;

			using (_mockRepository.Record())
			{
				Expect.Call(_helper.GetSnappedPosition(10)).Return(20);
				Expect.Call(_helper.GetSnappedPosition(20)).Return(20);
				Expect.Call(_helper.MinWidth()).Return(5).Repeat.Twice();

				Expect.Call(() => _view.Position(_model)).Repeat.Once();
				Expect.Call(() => _view.InvalidateParent()).Repeat.Twice();
			}

			using (_mockRepository.Playback())
			{
				_presenter.OnMouseUp(true, false);

				Assert.AreEqual(15, _model.Left);
			}			
		}

		[Test]
		public void ShouldNotSnapRightToSameAsLeft()
		{
			_model.Left = 10;
			_model.Width = 10;

			using (_mockRepository.Record())
			{
				Expect.Call(_helper.GetSnappedPosition(10)).Return(10);
				Expect.Call(_helper.GetSnappedPosition(20)).Return(10);
				Expect.Call(_helper.MinWidth()).Return(5);

				Expect.Call(() => _view.Position(_model)).Repeat.Once();
				Expect.Call(() => _view.InvalidateParent()).Repeat.Twice();
			}

			using (_mockRepository.Playback())
			{
				_presenter.OnMouseUp(false, true);

				Assert.AreEqual(5, _model.Width);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				_parent.Dispose();
			}	
		}

        [Test]
        public void ShouldThrowIfViewIsNull()
        {
			Assert.Throws<ArgumentNullException>(() => _presenter = new TransparentMeetingControlPresenter(null, _model, _helper));
        }

        [Test]
        public void ShouldThrowIfModelIsNull()
        {
			Assert.Throws<ArgumentNullException>(() => _presenter = new TransparentMeetingControlPresenter(_view, null, _helper));
        }

        [Test]
        public void ShouldThrowIfHelperIsNull()
        {
			Assert.Throws<ArgumentNullException>(() => _presenter = new TransparentMeetingControlPresenter(_view, _model, null));
        }
	}
}
