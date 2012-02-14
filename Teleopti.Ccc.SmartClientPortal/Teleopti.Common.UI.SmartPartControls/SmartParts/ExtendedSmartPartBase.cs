﻿using System.Windows;


namespace Teleopti.Common.UI.SmartPartControls.SmartParts
{
    public partial class ExtendedSmartPartBase : SmartPartBase, IExtendedSmartPartBase
    {
      
        public ExtendedSmartPartBase()
        {
            InitializeComponent();
        }

        public void LoadExtender(UIElement sourceElement)
        {
            HostContainer.Child = sourceElement;
        }
    }
}
