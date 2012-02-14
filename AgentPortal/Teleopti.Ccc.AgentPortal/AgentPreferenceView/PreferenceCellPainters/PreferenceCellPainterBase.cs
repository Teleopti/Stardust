﻿using System;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView.PreferenceCellPainters
{
    public abstract class PreferenceCellPainterBase : IPreferenceCellPainter
    {
        private readonly GridControlBase _gridControlBase;

        protected PreferenceCellPainterBase(GridControlBase gridControlBase)
        {
            _gridControlBase = gridControlBase;
        }

        protected GridControlBase GridControlBase
        {
            get { return _gridControlBase; }
        }

        protected static float MiddleX(Rectangle clientRectangle)
        {
            return clientRectangle.Left + (clientRectangle.Width / 2);
        }

        protected static float MiddleY(Rectangle clientRectangle)
        {
            return clientRectangle.Top + (clientRectangle.Height / 2);
        }

        protected static Rectangle RestrictionRect(Rectangle clientRectangle)
        {
            Rectangle rect = new Rectangle(clientRectangle.X, clientRectangle.Y + 15, clientRectangle.Width, clientRectangle.Height / 2 / 2);
            rect.Inflate(-6, 0);
            return rect;
        }

        protected static Rectangle EffectiveRect(Rectangle clientRectangle)
        {
            Rectangle rect = new Rectangle(clientRectangle.X, clientRectangle.Y + (clientRectangle.Height / 2), clientRectangle.Width, clientRectangle.Height / 2);
            rect.Inflate(-6, 1);
            return rect;
        }

        protected string LongOrShortText(string name, string shortName, int maxWidth, Graphics g)
        {
            string text = name;
            SizeF size = g.MeasureString(text, _gridControlBase.Font);
            if (size.Width > maxWidth)
                text = shortName;
            return text;
        }

        public abstract bool CanPaint(PreferenceCellData cellValue);

        public abstract void Paint(Graphics g, Rectangle clientRectangle, PreferenceCellData cellValue,
                                   Preference preference, EffectiveRestriction effectiveRestriction, StringFormat format);
    }
}
