using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
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



        public void DrawPreference(PermissionState permissionState)
        {
            if (permissionState != PermissionState.None)
            {
                Brush brush = getBrush(permissionState);
                Point x = new Point(_gridDrawCellEventArgs.Bounds.Right - 45, _gridDrawCellEventArgs.Bounds.Top + 5);
                Point y = new Point(_gridDrawCellEventArgs.Bounds.Right - 45 + 8, _gridDrawCellEventArgs.Bounds.Top + 5 + 8);
                Point z = new Point(_gridDrawCellEventArgs.Bounds.Right - 45, _gridDrawCellEventArgs.Bounds.Top + 5 + 8);

                _gridDrawCellEventArgs.Graphics.FillPolygon(brush, new[] { x, z, y });
            }
        }

        public void DrawStudentAvailability(PermissionState permissionState)
        {
            if (permissionState != PermissionState.None)
            {
                Brush brush = getBrush(permissionState);
                Point x = new Point(_gridDrawCellEventArgs.Bounds.Right - 60, _gridDrawCellEventArgs.Bounds.Top + 5);
                Point y = new Point(_gridDrawCellEventArgs.Bounds.Right - 60 + 8, _gridDrawCellEventArgs.Bounds.Top + 5);
                Point z = new Point(_gridDrawCellEventArgs.Bounds.Right - 60, _gridDrawCellEventArgs.Bounds.Top + 5 + 8);

                _gridDrawCellEventArgs.Graphics.FillPolygon(brush, new[] { x, z, y });
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