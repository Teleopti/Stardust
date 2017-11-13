$(document).ready(function () {
	var target;
	var common = Teleopti.MyTimeWeb.Common;
	var notifier = Teleopti.MyTimeWeb.Notifier;
	var alertActivity = Teleopti.MyTimeWeb.AlertActivity;
	var intervalTimeout = 5 * 60 * 1000; // hardcode to 5 min
	var ajax;
	var realAjax = Teleopti.MyTimeWeb.Ajax;
	var realGetTeleoptiTime = Date.prototype.getTeleoptiTime;
	var realNotify = notifier.Notify;
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
		},
		teardown: function () {
			Teleopti.MyTimeWeb.Ajax = realAjax;
			Date.prototype.getTeleoptiTime = realGetTeleoptiTime;
			notifier.Notify = realNotify;
			target.Destroy();
		}
	});

	test("Should show notice if schedule changed within correct period", function () {
		target = Teleopti.MyTimeWeb.PollScheduleUpdates;

		var currentText;
		notifier.Notify = function (setting, noticeText) {
			currentText = noticeText;
		}
		alertActivity.GetNotificationDisplayTime = function (callback) {
			callback();
		}
		var notifyText = "Your schedule for {0} has changed!"
		target.Init({ intervalTimeout: 0, notifyText: notifyText });

		equal(currentText, target.GetNotifyText(target.GetSettings().notifyPeriod.startDate, target.GetSettings().notifyPeriod.endDate));

	});

	test("Should call listener callback if has schedule change within correct period",
		function () {
			var called = false;
			target.AddListener('Schedule/Week', { startDate: '2017-11-13', endDate: '2017-11-16' }, function () {
				called = true;
			});
			target.Init({ intervalTimeout: 0 });
			equal(called, true);
		});

	test("Should not call listener callback if has schedule change but not in correct period",
		function () {
			var called = false;
			target.AddListener('Schedule/Week', { startDate: '2017-11-17', endDate: '2017-11-17' }, function () {
				called = true;
			});
			target.Init({ intervalTimeout: 0 });
			equal(called, false);
		});



	function fakeAjax() {
		Teleopti.MyTimeWeb.Ajax = function () {
			return {
				Ajax: function (options) {
					switch (options.url) {
						case 'Asm/CheckIfScheduleHasUpdates':
							options.success({ StartDate: '2017-11-13', EndDate: '2017-11-16', HasUpdates: true });
							break;
						case 'Asm/NotificationsTimeToStaySetting':
							console.log('get notification time')
							options.success(1000);
							break;
						case 'UserData/FetchUserData':
							options.success({});
							break;
					}
				}
			}
		};
	}
});