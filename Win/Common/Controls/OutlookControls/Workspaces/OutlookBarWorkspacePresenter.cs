using System.Collections.Generic;
using System.Drawing;

namespace Teleopti.Ccc.Win.Common.Controls.OutlookControls.Workspaces
{
    public class OutlookBarWorkspacePresenter
    {
        private readonly IOutlookBarWorkspaceView _view;
        private readonly OutlookBarWorkspaceModel _model;

        public OutlookBarWorkspacePresenter(IOutlookBarWorkspaceView view, OutlookBarWorkspaceModel model)
        {
            _view = view;
            _model = model;
        }

        public void Initialize()
        {
            _view.CreateGroupBars();
            _view.SetNumberOfVisibleGroupBars(_model.NumberOfVisibleGroupBars);
            _view.StartupModule(_model.StartupModule);
        }

        public IList<OutlookBarInfo> GetItems()
        {
            return _model.ItemCollection;
        }

        public void Close()
        {
            _model.NumberOfVisibleGroupBars = _view.NumberOfVisibleGroupBars;
            _model.LastModule = _view.LastModule;
        }

        //Crap to convert to icon, should'nt be here, dunno how to fix this fxCop
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public Icon ConvertToIcon(Image image, int size, bool keepAspRatio)
        {
            var square = new Bitmap(size, size);

            var g = Graphics.FromImage(square);
            int x, y, w, h;
            if (!keepAspRatio || image.Height == image.Width)
            {
                x = y = 0;
                w = h = size;
            }
            else
            {
                var r = image.Width/(float) image.Height;
                if (r > 1)
                {
                    w = size;
                    h = (int) (size/r);
                    x = 0;
                    y = (size - h)/2;
                }
                else
                {
                    w = (int) (size*r);
                    h = size;
                    y = 0;
                    x = (size - w)/2;
                }
            }
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, x, y, w, h);
            g.Flush();
            return Icon.FromHandle(square.GetHicon());
        }
    }
}
