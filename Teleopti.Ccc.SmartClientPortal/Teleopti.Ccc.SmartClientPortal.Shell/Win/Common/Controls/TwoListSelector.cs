using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
{
    public partial class TwoListSelector : BaseUserControl
    {
        public event EventHandler<SelectedChangedEventArgs> SelectedAdded;
        public event EventHandler<SelectedChangedEventArgs> SelectedRemoved;

        private const string WrapperDisplayMember = "DisplayName";

        private class EntityWrapper : INotifyPropertyChanged
        {
            public string DisplayName { get; set; }
            public bool Deleted { get; set; }
            public object TheEntity { get; set; }
            //Just for fixing mem leaks when used in BindingSource
#pragma warning disable 0067
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 0067
        }

        private class EntityWrapper<T> : EntityWrapper
        {
            public T Entity { get; set; }
        }

        public TwoListSelector()
        {
            InitializeComponent();
            listBox2.DrawMode = DrawMode.OwnerDrawFixed;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode) return;
            SetTexts();
        }

        private BindingSource BindingSourceAvailable { get { return (BindingSource) listBox1.DataSource; } set { listBox1.DataSource = value; } }

        private BindingSource BindingSourceSelected { get { return (BindingSource) listBox2.DataSource; } set { listBox2.DataSource = value; } }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public IList<T> GetSelected<T>()
        {
            IList<T> retList = new List<T>();
            var bindingSourceSelected = (BindingSource) listBox2.DataSource;
            foreach (var selected in bindingSourceSelected)
            {
                var wrapper = (EntityWrapper<T>) selected;
                retList.Add(wrapper.Entity);
            }
            return retList;
        }

        public void Initiate<T>(IEnumerable<T> available, IEnumerable<T> selected, string displayMember, string availableLabelText,
                                string selectedLabelText)
        {
            var reflector = new PropertyReflector();

            availableLabel.Text = availableLabelText;
            selectedLabel.Text = selectedLabelText;

            BindingSourceSelected = createBindingSourceSelected(selected, reflector, displayMember);
            BindingSourceAvailable = createBindingSourceAvailable(selected, reflector, displayMember, available);

            SetupListboxesSortAndDisplayMembers();
        }

        private void SetupListboxesSortAndDisplayMembers()
        {
            listBox1.DisplayMember = WrapperDisplayMember;
            listBox2.DisplayMember = WrapperDisplayMember;
            BindingSourceAvailable.SafeSort(WrapperDisplayMember);
            BindingSourceSelected.SafeSort(WrapperDisplayMember);
        }

        private BindingSource createBindingSourceAvailable<T>(IEnumerable<T> selected, PropertyReflector reflector, string displayMember,
                                                              IEnumerable<T> available)
        {
            var bindingSource = new BindingSource();
            foreach (var item in available)
            {
                var exists = false;
                foreach (var selectedItem in selected)
                {
                    if (item.Equals(selectedItem))
                    {
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                {
                    var wrapper = createWrapper(item, reflector, displayMember);
                    if (!wrapper.Deleted)
                    {
                        bindingSource.Add(wrapper);
                    }
                }
            }
            bindingSource.SafeSort(WrapperDisplayMember);
            return bindingSource;
        }

        private BindingSource createBindingSourceSelected<T>(IEnumerable<T> selected, PropertyReflector reflector, string displayMember)
        {
            var bindingSource = new BindingSource();
            foreach (var item in selected)
            {
                var wrapper = createWrapper(item, reflector, displayMember);
                bindingSource.Add(wrapper);
            }
            bindingSource.SafeSort(WrapperDisplayMember);
            return bindingSource;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private EntityWrapper<T> createWrapper<T>(T item, PropertyReflector reflector, string displayMember)
        {
            var wrapper = new EntityWrapper<T>();
            wrapper.Entity = item;
            wrapper.TheEntity = item;

            var deletable = item as IDeleteTag;
            if (deletable != null)
                wrapper.Deleted = deletable.IsDeleted;

            var prefix = string.Empty;
            var suffix = string.Empty;
            if (wrapper.Deleted)
            {
                prefix = "(";
                suffix = ")";
            }

            wrapper.DisplayName = prefix + reflector.GetValue(item, displayMember) + suffix;

            return wrapper;
        }

        private void buttonAdvSelectAll_Click(object sender, EventArgs e)
        {
            var copy = ((BindingSource) listBox1.DataSource).CopyEnumerable<EntityWrapper>();
            foreach (var item in copy)
            {
                SelectItem(item);
            }
            BindingSourceAvailable.SafeClear();
            SetupListboxesSortAndDisplayMembers();
        }

        private void buttonAdvSelectOne_Click(object sender, EventArgs e)
        {
            SelectOne();
        }

        private void buttonAdvDeselectOne_Click(object sender, EventArgs e)
        {
            DeselectOne();
        }

        private void buttonAdvDeselectAll_Click(object sender, EventArgs e)
        {
            var copy = ((BindingSource) listBox2.DataSource).CopyEnumerable<EntityWrapper>();
            foreach (var item in copy)
            {
                if (item != null)
                {
                    DeselectItem(item);
                }
            }
            BindingSourceSelected.SafeClear();
            SetupListboxesSortAndDisplayMembers();
        }

        private void SelectItem(EntityWrapper item)
        {
            BindingSourceSelected.Add(item);
            BindingSourceAvailable.Remove(item);

			SelectedAdded?.Invoke(listBox2, new SelectedChangedEventArgs(item.TheEntity));
        }

        private void DeselectItem(EntityWrapper item)
        {
            if (!item.Deleted)
                BindingSourceAvailable.Add(item);

            if(BindingSourceSelected.Contains(item))
                BindingSourceSelected.Remove(item);
            else
            {
                //Could have been reloaded with "other" wrappers
                EntityWrapper toRemove = null;
                foreach (EntityWrapper wrapper in BindingSourceSelected)
                {
                    if (wrapper.TheEntity.Equals(item.TheEntity))
                        toRemove = wrapper;
                }
                if (toRemove != null)
                BindingSourceSelected.Remove(toRemove);
            }
			SelectedRemoved?.Invoke(listBox2, new SelectedChangedEventArgs(item.TheEntity));
        }

        private void listBox2_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();
            using (Brush customBrush = new SolidBrush(e.ForeColor))
            {
                var fontNew = e.Font;

                var wrapper = (EntityWrapper) listBox2.Items[e.Index];
                if (wrapper.Deleted)
                {
                    fontNew = new Font(e.Font, FontStyle.Strikeout);
                }
                e.Graphics.DrawString(wrapper.DisplayName, fontNew, customBrush, e.Bounds,
                                      StringFormat.GenericDefault);
                e.DrawFocusRectangle();
            }
        }

        public override bool HasHelp { get { return false; } }

        private void SelectOne()
        {
            if (listBox1.SelectedItems.Count == 0)
                return;

            var copy = listBox1.SelectedItems.CopyEnumerable<EntityWrapper>();
            foreach (var item in copy)
            {
                SelectItem(item);
            }
            SetupListboxesSortAndDisplayMembers();
        }

        private void DeselectOne()
        {
            if (listBox2.SelectedItems.Count == 0)
                return;

            var copy = listBox2.SelectedItems.CopyEnumerable<EntityWrapper>();
            foreach (var item in copy)
            {
                DeselectItem(item);
            }
            SetupListboxesSortAndDisplayMembers();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            SelectOne();
        }

        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
            DeselectOne();
        }

        private void ReleaseManagedResources()
        {
            if (BindingSourceSelected != null)
            {
                BindingSourceSelected.SafeClear();
                BindingSourceSelected.Dispose();
                BindingSourceSelected = null;
            }
            if (BindingSourceAvailable != null)
            {
                BindingSourceAvailable.SafeClear();
                BindingSourceAvailable.Dispose();
                BindingSourceAvailable = null;
            }
            
            listBox2.DrawItem -= listBox2_DrawItem;
            listBox2.Dispose();
            listBox1.Dispose();
            
        }
    }

    public static class BindingSourceExtensions
    {
        public static void SafeSort(this BindingSource source, string memberName)
        {
            // avoid binding source bug. if no elements sort can not be set.
            if (source.Count == 0)
            {
                source.Sort = null;
            }
            else
            {
                source.Sort = memberName;
            }
        }

        public static void SafeClear(this BindingSource source)
        {
            if (source == null) return;
            // avoid binding source bug. if no elements sort can not be set.
            source.Sort = null;
            source.Clear();
        }
    }

    public class SelectedChangedEventArgs : EventArgs
    {
        public SelectedChangedEventArgs(object movedItem)
        {
            MovedItem = movedItem;
        }

        public object MovedItem { get; private set; }
    }
}