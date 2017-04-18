using System;
using System.Drawing;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
	public class TransparentMeetingControlPresenter
	{
		private readonly TransparentMeetingControlModel _model;
		private readonly ITransparentMeetingControlView _view;
		private readonly ITransparentControlHelper _helper;
		private bool _updateBackground;
		private bool _allowMove;
		private int _mousePosX;

		public TransparentMeetingControlPresenter(ITransparentMeetingControlView transparentMeetingControlView, TransparentMeetingControlModel transparentMeetingControlModel, ITransparentControlHelper transparentControlHelper)
		{
			if(transparentMeetingControlView == null)
				throw new ArgumentNullException("transparentMeetingControlView");

			if(transparentMeetingControlModel == null)
				throw new ArgumentNullException("transparentMeetingControlModel");

			if(transparentControlHelper == null)
				throw new ArgumentNullException("transparentControlHelper");

			_model = transparentMeetingControlModel;
			_view = transparentMeetingControlView;
			_helper = transparentControlHelper;
			_updateBackground = true;
		}

		public bool AllowMove
		{
			get { return _allowMove; }
			set { _allowMove = value; }
		}

		public bool UpdateBackground
		{
			get { return _updateBackground; }
			set { _updateBackground = value; }
		}

		public byte Transparency
		{
			get { return _model.Transparency; }
		}

		public Color BackColor
		{
			get { return _model.BackColor; }
		}

		public void OnMouseDown(int pos)
		{
			_allowMove = true;
			_updateBackground = false;
			_mousePosX = pos;
		}

		public void OnMouseUp(bool left, bool right)
		{
			Snap(left, right);
			_allowMove = false;
			_updateBackground = true;
			_view.InvalidateParent();
			_model.RaisePossibleChangeEvent();
		}

		private void Snap(bool left, bool right)
		{
			var leftPos = _helper.GetSnappedPosition(_model.Left);

			if(leftPos != (_model.Left + _model.Width))
				_model.Left = leftPos; // _helper.GetSnappedPosition(_model.Left);

			var rightPos = _helper.GetSnappedPosition(_model.Left + _model.Width);

			if (leftPos != rightPos)
				_model.Width = rightPos - _model.Left;
			else
			{
				if (left)
				{
					_model.Left = rightPos - _helper.MinWidth();
					_model.Width = _helper.MinWidth();
				}

				if (right)
					_model.Width = _helper.MinWidth();
			}

			_view.Position(_model);
			_view.InvalidateParent();
		}

		public void OnMouseMoveBorderWest(int pos)
		{
			if (_allowMove)
			{
				var leftPos = _model.Left + pos;
				var rightPos = leftPos + _model.Width - pos;
				var transformedLeft = _helper.GetLeftPosition(leftPos, rightPos);

				var diff = leftPos - transformedLeft;

				_model.Width = _model.Width - pos + diff;
				_model.Left = transformedLeft;
				_model.ModelChanged = true;
				
				_view.Position(_model);
				_view.InvalidateParent();

			}
		}

		public void OnMouseMoveBorderEast(int pos)
		{
			if (_allowMove)
			{
				var rightPos = _model.Left + _model.Width + pos;
				var transformedRight = _helper.GetRightPosition(_model.Left, rightPos);
				var diff = rightPos - transformedRight;

				_model.Width = _model.Width + pos - diff;
				_model.ModelChanged = true;

				_view.Position(_model);
				_view.InvalidateParent();
			}
		}

		public void OnMoveMouse(int pos)
		{
			if (_allowMove)
			{
				var leftPos = _model.Left + pos - _mousePosX;
				var transformedLeft = _helper.GetLeftPositionConsiderWidth(leftPos, _model.Width);
			
				_model.Left = transformedLeft;
				_model.ModelChanged = true;
				
				_view.Position(_model);
				_view.InvalidateParent();
			}
		}

		public void Initialize()
		{
			_view.SetBorderWidth(_model.BorderWidth);
			_view.SetWestBorderColor(_model.LeftBorderColor);
			_view.SetEastBorderColor(_model.RightBorderColor);
			_view.Position(_model);
			_view.InvalidateParent();
		}	
	}
}
