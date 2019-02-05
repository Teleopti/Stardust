using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Properties;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
    public class DrawRestrictionIcon
    {
        private GridDrawCellEventArgs _gridDrawCellEventArgs;

        public DrawRestrictionIcon(GridDrawCellEventArgs args)
        {
            _gridDrawCellEventArgs = args;
        }

        public void DrawAvailability(PermissionState permissionState)
        {
            if (permissionState != PermissionState.None)
            {
                Brush brush = getBrush(permissionState);
                _gridDrawCellEventArgs.Graphics.FillRectangle(brush, _gridDrawCellEventArgs.Bounds.Right - 15,
                                                              _gridDrawCellEventArgs.Bounds.Top + 5, 7, 7);
            }
        }

        public void DrawRotation(PermissionState permissionState)
        {
            if (permissionState != PermissionState.None)
            {
                Brush brush = getBrush(permissionState);
                _gridDrawCellEventArgs.Graphics.FillEllipse(brush, _gridDrawCellEventArgs.Bounds.Right - 30,
                                                            _gridDrawCellEventArgs.Bounds.Top + 5, 7, 7);
            }
        }



        public void DrawPreference(PermissionState permissionState,bool isMustHave)
        {
            if (permissionState != PermissionState.None)
            {
                //if(!isMustHave)
                {
                    Brush brush = getBrush(permissionState);
                    Point x = new Point(_gridDrawCellEventArgs.Bounds.Right - 45, _gridDrawCellEventArgs.Bounds.Top + 5);
                    Point y = new Point(_gridDrawCellEventArgs.Bounds.Right - 45 + 8, _gridDrawCellEventArgs.Bounds.Top + 5 + 8);
                    Point z = new Point(_gridDrawCellEventArgs.Bounds.Right - 45, _gridDrawCellEventArgs.Bounds.Top + 5 + 8);

                    _gridDrawCellEventArgs.Graphics.FillPolygon(brush, new[] { x, z, y });
                }
				if (isMustHave)
                {
                    Image mustHaveImage = Resources.heart_8x8;
                    //var x = new Point(_gridDrawCellEventArgs.Bounds.Right - 45, _gridDrawCellEventArgs.Bounds.Top + 5);
					var x = new Point(_gridDrawCellEventArgs.Bounds.Right - 45, _gridDrawCellEventArgs.Bounds.Top + 16);
                    _gridDrawCellEventArgs.Graphics.DrawImage( mustHaveImage,x.X,x.Y );
                }
               
            }
        }

        public void DrawStudentAvailability(PermissionState permissionState, bool availabiltyLeft)
        {
            if (permissionState != PermissionState.None)
            {
                Brush brush = getBrush(permissionState);
                Point x = new Point(_gridDrawCellEventArgs.Bounds.Right - 20, _gridDrawCellEventArgs.Bounds.Top + 5);
                Point y = new Point(_gridDrawCellEventArgs.Bounds.Right - 20 + 8, _gridDrawCellEventArgs.Bounds.Top + 5);
                Point z = new Point(_gridDrawCellEventArgs.Bounds.Right - 20, _gridDrawCellEventArgs.Bounds.Top + 5 + 8);

                _gridDrawCellEventArgs.Graphics.FillPolygon(brush, new[] { x, z, y });

				if (availabiltyLeft && permissionState == PermissionState.Satisfied)
				{
					x = new Point(_gridDrawCellEventArgs.Bounds.Right - 20 + 8, _gridDrawCellEventArgs.Bounds.Top + 5 + 8);
					_gridDrawCellEventArgs.Graphics.FillPolygon(Brushes.LightGray, new[] { x, z, y });
				}
            }
        }
        private static Brush getBrush(PermissionState permissionState)
        {
            Brush brush = Brushes.LightGray;
            if (permissionState == PermissionState.Satisfied)
            {
                brush = Brushes.LimeGreen;
            }
            if (permissionState == PermissionState.Broken)
            {
                brush = Brushes.Red;
            }
            return brush;
        }
    }
}