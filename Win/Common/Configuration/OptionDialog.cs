using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
{
	public partial class OptionDialog : BaseDialogForm 
	{
		public OptionCore Core { get; private set; }

		protected OptionDialog()
		{
			InitializeComponent();
 
			if (DesignMode) return;
			setColors();
			SetTexts();
		}

		public OptionDialog(OptionCore optionCore) : this()
		{
			Core = optionCore;
		}

		public void Page(Type pageType)
		{
			Core.MarkAsSelected(pageType,null);

			setupPage();
		}

		public void SetUnitOfWork(IUnitOfWork unitOfWork)
		{
			Core.SetUnitOfWork(unitOfWork);
		}

		private void setupPage()
		{
			if (!Core.HasLastPage) return;
			gradientPanel1.Controls.Clear();
			var ctrl = (Control)Core.LastPage;
			ctrl.Dock = DockStyle.Fill;
			var width = ctrl.Width - gradientPanel1.ClientSize.Width;
			var height = ctrl.Height - gradientPanel1.ClientSize.Height;
			Width += width;
			Height += height;
			gradientPanel1.Controls.Add(ctrl);
			ctrl.BringToFront();
			ActiveControl = ctrl;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F1)
			{
				setOptionPageAsActiveControl();
			}
			base.OnKeyDown(e);
		}

		protected override void OnHelpButtonClicked(System.ComponentModel.CancelEventArgs e)
		{
			setOptionPageAsActiveControl();
			base.OnHelpButtonClicked(e);
		}

		private void setOptionPageAsActiveControl()
		{
			if (gradientPanel1.Controls.Count>0)
			{
				ActiveControl = gradientPanel1.Controls[0];
			}
		}

		private void setColors()
		{
			BackColor = ColorHelper.FormBackgroundColor();
		}

		private void buttonAdvOkClick(object sender, EventArgs e)
		{
			// Save changes.
			var canClose = true;
			try
			{
				Core.SaveChanges();
				foreach (var settingPage in Core.AllSelectedPages)
				{
					var checkBeforeClosing = settingPage as ICheckBeforeClosing;
					if (checkBeforeClosing!=null && !checkBeforeClosing.CanClose())
					{
						canClose = false;
					}
				}
			}
			catch (ValidationException ex)
			{
				ViewBase.ShowWarningMessage(UserTexts.Resources.InvalidDataOnFollowingPage + ex.Message, UserTexts.Resources.ValidationError);
				DialogResult = DialogResult.None;
				return;
			}
			catch (OptimisticLockException)
			{
				ViewBase.ShowWarningMessage(UserTexts.Resources.OptimisticLockText, UserTexts.Resources.OptimisticLockHeader);
				DialogResult = DialogResult.None;
				Close();
				return;
			}
			catch (DataSourceException ex)
			{
				DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
				Close();
			}
			catch (Exception exception)
			{
				if (exception.InnerException is SqlException)
					if (exception.InnerException.Message.Contains("IX_KpiTarget"))
					{
						ViewBase.ShowWarningMessage(UserTexts.Resources.OptimisticLockText, UserTexts.Resources.OptimisticLockHeader);
						DialogResult = DialogResult.None;
						Close();
						return;
					}
				throw;
			}

			DialogResult = canClose ? DialogResult.OK : DialogResult.None;
		}
	}
}
