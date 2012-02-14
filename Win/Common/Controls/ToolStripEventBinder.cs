using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Common.Controls
{
    public class ToolStripEventBinder : Component
    {
        private IDictionary<ToolStripItem, Delegate> controlEventList;

        protected ToolStripEventBinder()
        {
            controlEventList = new Dictionary<ToolStripItem, Delegate>();
        }

        private static ToolStripEventBinder _default = new ToolStripEventBinder();
        public static ToolStripEventBinder Default { get { return _default; } }

        //private static EventBinder _default = new EventBinder();
        //public static EventBinder Default
        //{
        //    get
        //    {
        //        return _default;
        //    }
        //}

        public void BindEventHandler(ToolStripItem target, Delegate handler)
        {
            Delegate d = FindEventHandler(target);
            RemoveEventHandler(target, d);
            AddEventHandler(target, handler);
        }

        public void AddEventHandler(ToolStripItem target, Delegate handler)
        {
            if (handler != null)
            {
                EventHandler targetHandler = (EventHandler)handler;
                ToolStripSplitButton splitButton = (target as ToolStripSplitButton);

                if (splitButton != null)
                {
                    splitButton.ButtonClick += targetHandler;
                }
                else
                {
                    target.Click += targetHandler;
                }
                controlEventList.Add(target, targetHandler);
            }
        }

        public bool RemoveEventHandler(ToolStripItem target, Delegate handler)
        {
            bool deleted = false;
            if (handler != null)
            {
                EventHandler targetHandler = (EventHandler)handler;
                ToolStripSplitButton splitButton = (target as ToolStripSplitButton);

                if (splitButton != null)
                {
                    splitButton.ButtonClick -= targetHandler;
                }
                else
                {
                    target.Click -= targetHandler;
                }
                deleted = controlEventList.Remove(new KeyValuePair<ToolStripItem, Delegate>(target, targetHandler));
            }
            return deleted;
        }

        public Delegate FindEventHandler(ToolStripItem target)
        {
            if (controlEventList.Count == 0) return null;

            if (controlEventList.ContainsKey(target))
                return controlEventList[target];

            return null;
        }
    }
}
