using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.PeopleAdmin.Controls.Columns
{
    class ReadOnlyRoleDescriptionColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        //private int _maxLength;
        private string _headerText;
        private string _bindingProperty;
        private IList<IApplicationRole> _applicationRoles;
        private bool _readOnly;

        public ReadOnlyRoleDescriptionColumn(string bindingProperty, string headerText, IList<IApplicationRole> applicationRoles, bool readOnly)
        {
            // _maxLength = maxLength;
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _applicationRoles = applicationRoles;
            _readOnly = readOnly;
        }

        private IList<IApplicationRole> CreateAppRoleColByRoleString(string roleString)
        {
            IList<IApplicationRole> roles = new List<IApplicationRole>();
            Char[] separator = { ',' };
            string[] roleCollection = roleString.Split(separator);
            for (int i = 0; i < roleCollection.Length; i++)
            {
                for (int j = 0; j < _applicationRoles.Count; j++)
                {
                    if (_applicationRoles[j].DescriptionText.Trim() == roleCollection[i].Trim())
                    {
                        roles.Add(_applicationRoles[j]);
                        break;
                    }
                }
            }
            return roles;
        }


        public override int PreferredWidth
        {
            get { return 200; }
        }

		public override string BindingProperty
		{
			get
			{
				return _bindingProperty;
			}
		}

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
            if (IsContentRow(e.RowIndex,dataItems.Count))
            {
                T dataItem = dataItems[e.RowIndex - 1];

                IList<IApplicationRole> roles = (IList<IApplicationRole>)_propertyReflector.GetValue(dataItem, _bindingProperty);
                StringBuilder roleString = new StringBuilder();

                if (roles != null)
                {
                    // Orders the application role by application role name
                    IList<IApplicationRole> applicationRoles = roles.OrderBy(s => s.Name).ToList();

                    foreach (IApplicationRole role in applicationRoles)
                    {
                        if (!string.IsNullOrEmpty(roleString.ToString()))
                            roleString.Append(", " + role.DescriptionText);
                        else roleString.Append(role.DescriptionText);
                    }
                }
                e.Style.CellValue = roleString.ToString();
                e.Style.ReadOnly = true;

                OnCellDisplayChanged(dataItem, e);
            }

            if (_readOnly)
            {
                e.Style.ReadOnly = true;
                e.Style.TextColor = Color.DimGray;
            }

            e.Handled = true;
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                if (dataItems.Count == 0) return;
                if (_readOnly) return;

                T dataItem = dataItems.ElementAt(e.RowIndex - 1);
                _propertyReflector.SetValue(dataItem, _bindingProperty, CreateAppRoleColByRoleString(e.Style.CellValue.ToString()));
				OnCellChanged(dataItem, e);
				e.Handled = true;
            }
        }
    }
}
