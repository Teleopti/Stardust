$(document).ready(function () {
	var target = Teleopti.MyTimeWeb.PollScheduleUpdates;
	var common = Teleopti.MyTimeWeb.Common;
	var notifier = Teleopti.MyTimeWeb.Notifier;
	var alertActivity = Teleopti.MyTimeWeb.AlertActivity;
	var intervalTimeout = 5 * 60 * 1000; // hardcode to 5 min
	var currentText;
	var notifyText = "Your schedule has changed!"

	var realAjax = Teleopti.MyTimeWeb.Ajax;
	var realGetTeleoptiTime = Date.prototype.getTeleoptiTime;
	var realNotify = notifier.Notify;
	var realGetNotificationDisplayTime = alertActivity.GetNotificationDisplayTime;
	var hasUpdates = true;
	module("Teleopti.MyTimeWeb.PollScheduleUpdates", {
		setup: function () {
			fakeAjax();
			var options = {
				UserTimezoneOffsetMinute: 0,
				HasDayLightSaving: 'False',
				DayLightSavingStart: null,
				DayLightSavingEnd: null,
				DayLightSavingAdjustmentInMinute: null
			};
			Date.prototype.getTeleoptiTime = common.SetupTeleoptiTime(options);
			notifier.Notify = function (setting, noticeText) {
				currentText = noticeText;
			}
			alertActivity.GetNotificationDisplayTime = function (callback) {
				callback();
			}
		},
		teardown: function () {
			Teleopti.MyTimeWeb.Ajax = realAjax;
			Date.prototype.getTeleoptiTime = realGetTeleoptiTime;
			notifier.Notify = realNotify;
			alertActivity.GetNotificationDisplayTime = realGetNotificationDisplayTime;
			target.Destroy();
		}
	});

	test("Should show notice if schedule changed within correct period", function () {
		target.Init({ intervalTimeout: 0, notifyText: notifyText });
		equal(currentText, target.GetNotifyText());
	});

	test("Should not show notice if no schedule changed within correct period", function () {
		hasUpdates = false;
		currentText = null;
		target.Init({ intervalTimeout: 0, notifyText: notifyText });
		equal(currentText, null);
		hasUpdates = true;
	});

	test("Should call listener callback if has schedule change within  period",
		function () {
			var called = false;
			target.SetListener('Schedule/Week', function () {
				called = true;
			});
			target.Init({ intervalTimeout: 0, notifyText: notifyText });
			equal(called, true);
		});


	test("Should only one listener be invoked when time is up", function () {
		var executedListener;
		target.SetListener('Schedule/Week', function () {
			executedListener = 'Schedule/Week';
		});
		target.SetListener('Schedule/ASM', function () {
			executedListener = 'Schedule/ASM';
		});
		target.Init({ intervalTimeout: 0, notifyText: notifyText });
		equal(executedListener, 'Schedule/ASM');
	});



	function fakeAjax() {
		Teleopti.MyTimeWeb.Ajax = function () {
			return {
				Ajax: function (options) {
					switch (options.url) {
						case 'Asm/StartPolling':
							options.success('mailboxId');
							break;
						case 'Asm/CheckIfScheduleHasUpdates':
							options.success({ HasUpdates: hasUpdates });
							break;
						case 'Asm/NotificationsTimeToStaySetting':
							options.success(1000);
							break;
						case 'UserData/FetchUserData':
							options.success({});
							break;
					}
				}
			}
		};
		return Teleopti.MyTimeWeb.Ajax;
	}
});