﻿using System;
using System.Windows.Forms;
using Autofac;
using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.Services;
using Microsoft.Practices.CompositeUI.WPF;
using Teleopti.Ccc.SmartClientPortal.Shell.Common.Library.Services;
using Teleopti.Ccc.SmartClientPortal.Shell.Common.Services;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Common.Library
{
    [CLSCompliant(false)]
    public abstract class SmartClientApplication<TWorkItem, TShell> : WPFFormShellApplication<TWorkItem, TShell>
        where TWorkItem : WorkItem, new()
        where TShell : Form
    {
        private readonly IComponentContext _container;

        protected SmartClientApplication(IComponentContext container)
        {
            _container = container;
        }

        protected override void AddServices()
        {
            base.AddServices();

            RootWorkItem.Services.AddNew<WorkspaceLocatorService, IWorkspaceLocatorService>();
            RootWorkItem.Services.Remove<IModuleEnumerator>();
            RootWorkItem.Services.Remove<IModuleLoaderService>();
            RootWorkItem.Services.Add(_container);
            RootWorkItem.Services.AddNew<EmptyDependentModuleEnumerator, IModuleEnumerator>();
            RootWorkItem.Services.AddNew<DependentModuleLoaderService, IModuleLoaderService>();
            RootWorkItem.Services.AddOnDemand<ActionCatalogService, IActionCatalogService>();
            RootWorkItem.Services.AddOnDemand<EntityTranslatorService, IEntityTranslatorService>();
        }
    }
}

