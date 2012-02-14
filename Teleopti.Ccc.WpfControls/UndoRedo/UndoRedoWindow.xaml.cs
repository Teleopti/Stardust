using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WpfControls.UndoRedo.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.UndoRedo
{
    /// <summary>
    /// Interaction logic for UndoRedoWindow.xaml
    /// </summary>
    public partial class UndoRedoWindow : Window
    {

        private UndoRedoViewModel _model;
        
        public UndoRedoWindow(IUndoRedoContainer container):this(container,new EventAggregator(),new CreateLayerViewModelService())
        {
          
        }

        public UndoRedoWindow(IUndoRedoContainer container,IEventAggregator eventAggregator,ICreateLayerViewModelService createLayerViewModelService)
        {
            InitializeComponent();
            _model = new UndoRedoViewModel(container, eventAggregator,createLayerViewModelService);
            DataContext = _model;
        }
    }
}
