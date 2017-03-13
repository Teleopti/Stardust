$(document).ready(function() {

	module("Teleopti.MyTimeWeb.AlertActivity");

	test("Should update notification display time correctly",
		function() {
			var oldAjax = Teleopti.MyTimeWeb.Ajax;


			var oldAsm = Teleopti.MyTimeWeb.Asm;

			var notificationDisplayTimeSetting;

			Teleopti.MyTimeWeb.Asm = {
				UpdateNotificationDisplayTimeSetting: function(value) {
					notificationDisplayTimeSetting = value;
				}
			};

			var target = Teleopti.MyTimeWeb.AlertActivity;

			target._replaceAjax({
				Ajax: function(option) {
					option.success({ DurationInSecond: 3000 });
					return {
						done: function() {}
					};
				}
			});			

			target.GetNotificationDisplayTime(function() {});

			equal(notificationDisplayTimeSetting, 3000);

			Teleopti.MyTimeWeb.Ajax = oldAjax;
			Teleopti.MyTimeWeb.Asm = oldAsm;
		});
});
