using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Presenters;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingScreenInternals
{
    public class ScheduleGridCutPasteHandler : BaseCutPasteHandler
    {
        private readonly WeakReference _owner;
        private readonly Func<ScheduleViewBase> _scheduleView;
        private readonly Action<DeleteOption> _startDeleteAction;
        private readonly Action _checkPastePermissionsAction;
        private readonly Action<PasteOptions> _pasteFromClipboardAction;
        private readonly Action _enablePasteOperationAction;
        private readonly IExternalExceptionHandler _externalExceptionHandler = new ExternalExceptionHandler();
		private readonly WorkShiftContainsMasterActivitySpecification _workShiftContainsMasterActivity = new WorkShiftContainsMasterActivitySpecification();

        public ScheduleGridCutPasteHandler(SchedulingScreen owner, Func<ScheduleViewBase> scheduleViewFunction, Action<DeleteOption> startDeleteAction, Action checkPastePermissionsAction, Action<PasteOptions> pasteFromClipboardAction, Action enablePasteOperationAction)
        {
            _owner = new WeakReference(owner);
            _scheduleView = scheduleViewFunction;
            _startDeleteAction = startDeleteAction;
            _checkPastePermissionsAction = checkPastePermissionsAction;
            _pasteFromClipboardAction = pasteFromClipboardAction;
            _enablePasteOperationAction = enablePasteOperationAction;
        }

        private void guardAction(Action<SchedulingScreen> action, Action<SchedulingScreen> actionIfNoScheduleView = null)
        {
            if (_owner == null || !_owner.IsAlive) return;

            var owner = (SchedulingScreen) _owner.Target;
            if (_scheduleView() != null)
            {
                if (action!=null)
                    action.Invoke(owner);
            }
            else
            {
                if (actionIfNoScheduleView!=null)
                    actionIfNoScheduleView.Invoke(owner);
            }
        }

        public override void Paste()
        {
            guardAction(owner =>
            {
                var options = new PasteOptions();
                options.Default = true;

                if (owner.ClipsHandlerSchedule.IsInCutMode)
                    options = owner.ClipsHandlerSchedule.CutMode;

                _pasteFromClipboardAction(options);
                checkCutMode(owner);
            });
        }

        private static void checkCutMode(SchedulingScreen owner)
        {
            if (owner.ClipsHandlerSchedule.IsInCutMode)
            {
                owner.ClipsHandlerSchedule.IsInCutMode = false;
            }
        }

        public override void Cut()
        {
            guardAction(owner =>
            {
                var deleteOptions = new PasteOptions();
                deleteOptions.Default = true;
                setCutMode(deleteOptions);
                deleteInMainGrid(deleteOptions);
            });
        }

        public override void CutSpecial()
        {
            guardAction(owner =>
            {
                var options = new PasteOptions();
                var clipboardSpecialOptions = new ClipboardSpecialOptions();
                clipboardSpecialOptions.ShowRestrictions = _scheduleView() is AgentRestrictionsDetailView;
                clipboardSpecialOptions.DeleteMode = true;
                clipboardSpecialOptions.ShowOvertimeAvailability = false;
                clipboardSpecialOptions.ShowShiftAsOvertime = false;

                var cutSpecial = new FormClipboardSpecial(options, clipboardSpecialOptions,
                    owner.MultiplicatorDefinitionSet) {Text = Resources.CutSpecial};
                cutSpecial.ShowDialog();

	            if (!cutSpecial.Cancel())
	            {
		            deleteInMainGrid(options);
		            options.MainShift = options.MainShiftSpecial;
		            options.MainShiftSpecial = false;
					setCutMode(options);
	            }

	            cutSpecial.Close();
            });
        }

        private void setCutMode(PasteOptions cutMode)
        {
            guardAction(owner =>
            {
                _scheduleView().GridClipboardCopy(true);
                owner.ClipsHandlerSchedule.IsInCutMode = true;
                owner.ClipsHandlerSchedule.CutMode = cutMode;
                _checkPastePermissionsAction();
            }, owner =>
            {
                owner.ClipsHandlerSchedule.IsInCutMode = false;
            });
        }

        public override void DeleteSpecial()
        {
            var authorization = PrincipalAuthorization.Current_DONTUSE();
            var options = new PasteOptions();
            var clipboardSpecialOptions = new ClipboardSpecialOptions();
            clipboardSpecialOptions.ShowRestrictions = _scheduleView() is AgentRestrictionsDetailView;
            clipboardSpecialOptions.DeleteMode = true;
            clipboardSpecialOptions.ShowOvertimeAvailability = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyAvailabilities);
            clipboardSpecialOptions.ShowShiftAsOvertime = false;

            guardAction(owner =>
            {
                using (
                    var deleteSpecial = new FormClipboardSpecial(options, clipboardSpecialOptions,
                        owner.MultiplicatorDefinitionSet))
                {
                    deleteSpecial.Text = Resources.DeleteSpecial;
                    deleteSpecial.ShowDialog();

                    if (!deleteSpecial.Cancel())
                    {
                        deleteInMainGrid(options);
                    }
                }
            });
        }

        private void deleteInMainGrid(PasteOptions deleteOptions)
        {
            var localDeleteOption = new DeleteOption();
            localDeleteOption.MainShift = deleteOptions.MainShift;
	        localDeleteOption.MainShiftSpecial = deleteOptions.MainShiftSpecial;
            localDeleteOption.DayOff = deleteOptions.DayOff;
            localDeleteOption.PersonalShift = deleteOptions.PersonalShifts;
            localDeleteOption.Overtime = deleteOptions.Overtime;
            localDeleteOption.Preference = deleteOptions.Preference;
            localDeleteOption.StudentAvailability = deleteOptions.StudentAvailability;
            localDeleteOption.OvertimeAvailability = deleteOptions.OvertimeAvailability;
            PasteAction pasteAction = deleteOptions.Absences;
            if (pasteAction == PasteAction.Replace)
                localDeleteOption.Absence = true;

            localDeleteOption.Default = deleteOptions.Default;
            _startDeleteAction(localDeleteOption);
        }

        public override void Delete()
        {
            guardAction(_ =>
            {
                var deleteOptions = new PasteOptions {Default = true};
	            deleteInMainGrid(deleteOptions);
            });
        }

        public override void CutPersonalShift()
        {
            guardAction(_ =>
            {
                var deleteOptions = new PasteOptions {PersonalShifts = true};
	            setCutMode(deleteOptions);
                deleteInMainGrid(deleteOptions);
            });
        }

        public override void CutDayOff()
        {
            guardAction(_ =>
            {
                var deleteOptions = new PasteOptions {DayOff = true};
	            setCutMode(deleteOptions);
                deleteInMainGrid(deleteOptions);
            });
        }

        public override void CutAbsence()
        {
            guardAction(_ =>
            {
                var deleteOptions = new PasteOptions {Absences = PasteAction.Replace};
	            setCutMode(deleteOptions);
                deleteInMainGrid(deleteOptions);
            });
        }

        public override void CutAssignment()
        {
            guardAction(_ =>
            {
                var deleteOptions = new PasteOptions {MainShift = true};
	            setCutMode(deleteOptions);
                deleteInMainGrid(deleteOptions);
            });
        }

        public override void PasteAssignment()
        {
            guardAction(owner =>
            {
                var options = new PasteOptions {MainShift = true};
                _pasteFromClipboardAction(options);
                checkCutMode(owner);
            });
        }

        public override void PasteAbsence()
        {
            guardAction(owner =>
            {
                var options = new PasteOptions {Absences = PasteAction.Add};
                _pasteFromClipboardAction(options);
                checkCutMode(owner);
            });
        }

        public override void PasteDayOff()
        {
            guardAction(owner =>
            {
                var options = new PasteOptions {DayOff = true};
                _pasteFromClipboardAction(options);
                checkCutMode(owner);
            });
        }

        public override void PastePersonalShift()
        {
            guardAction(owner =>
            {
                var options = new PasteOptions {PersonalShifts = true};
                _pasteFromClipboardAction(options);
                checkCutMode(owner);
            });
        }

        public override void PasteSpecial()
        {
            var options = new PasteOptions();
            var clipboardSpecialOptions = new ClipboardSpecialOptions
            {
	            ShowRestrictions = _scheduleView() is AgentRestrictionsDetailView,
	            DeleteMode = false,
	            ShowOvertimeAvailability = false,
	            ShowShiftAsOvertime = true
            };

	        guardAction(owner =>
            {
	            IEnumerable<IMultiplicatorDefinitionSet> multiplicatorDefinitionSets =
		            new MultiplicatorsetForPasteSpecialFilter()
						.FilterAvailableMultiplicatorSet(_scheduleView().SelectedSchedules());

                var pasteSpecial = new FormClipboardSpecial(options, clipboardSpecialOptions, multiplicatorDefinitionSets) {Text = Resources.PasteSpecial};
                pasteSpecial.ShowDialog();

                if (_scheduleView() != null)
                {
                    if (!pasteSpecial.Cancel())
                    {
                        _pasteFromClipboardAction(options);
                        checkCutMode(owner);
                    }
                }

                pasteSpecial.Close();
            });
        }

        public override void PasteShiftFromShifts()
        {
            var workShift = ShiftInClip.Data;
            if (workShift == null)
                return;
            if (_workShiftContainsMasterActivity.IsSatisfiedBy(workShift))
            {
                ViewBase.ShowErrorMessage(Resources.CannotPasteAShiftWithMasterActivity, Resources.PasteError);
                return;
            }

            guardAction(owner =>
            {
                IScheduleDay scheduleDay;
                if (!tryGetFirstSelectedSchedule(out scheduleDay)) return;

                var part =
                    (IScheduleDay) owner.SchedulerState.SchedulerStateHolder.Schedules[scheduleDay.Person].ReFetch(scheduleDay).Clone();

                part.Clear<IScheduleData>();
                IEditableShift mainShift = workShift.ToEditorShift(part.DateOnlyAsPeriod,
                    part.Person.PermissionInformation.DefaultTimeZone());
                var category =
                    owner.SchedulerState.SchedulerStateHolder.CommonStateHolder.ShiftCategories.FirstOrDefault(
                        cat => cat.Id.Equals(workShift.ShiftCategory.Id));
                if (category != null)
                {
                    mainShift.ShiftCategory = category;
                }

                part.AddMainShift(mainShift);

                owner.ClipsHandlerSchedule.Clear();
                owner.ClipsHandlerSchedule.AddClip(0, 0, part);
                _externalExceptionHandler.AttemptToUseExternalResource(
                    () => Clipboard.SetData("PersistableScheduleData", new int()));
            });
            Paste();
        }

        private bool tryGetFirstSelectedSchedule(out IScheduleDay scheduleDay)
        {
            scheduleDay = null;
            var selectedSchedules = _scheduleView().SelectedSchedules();
            if (selectedSchedules.Count == 0) return false;

            scheduleDay = selectedSchedules[0];
            return true;
        }

        public override void Copy()
        {
            guardAction(owner =>
            {
                _scheduleView().GridClipboardCopy(false);
                _checkPastePermissionsAction();
                _enablePasteOperationAction();
            });
        }
    }
}