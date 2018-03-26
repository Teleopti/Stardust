$(document).ready(function() {
	var oldAjax;
	module("Teleopti.MyTimeWeb.Asm",
		{
			setup:function() {
				Date.prototype.getTeleoptiTime = function() { return new Date("2018-03-05T07:30:00Z").getTime(); };
				Date.prototype.getTeleoptiTimeInUserTimezone = function() {
					return '2018-03-04';
				};
				Teleopti.MyTimeWeb.Common.FakeToggles({
					MyTimeWeb_PollToCheckScheduleChanges_46595:false
				});
				oldAjax = Teleopti.MyTimeWeb.Ajax;
			},
			teardown: function() {
				Teleopti.MyTimeWeb.Ajax = oldAjax;
			}
		});

	test("Should fetch layer with yesterday under user timezone",
		function() {
			var target = Teleopti.MyTimeWeb.Asm;
			var ajaxOption;
			target._replaceAjax({
				Ajax: function(option) {
					ajaxOption = option;
					option.success({ });
					return {
						done: function() {}
					};
				}
			});	
			target.ShowAsm();

			equal(ajaxOption.data.asmZeroLocal, "2018-03-03");
			
		});


	test("Should adjust position when entering DST",
		function () {
			Date.prototype.getTeleoptiTime = function () { return new Date("2018-03-11T07:30:00Z").getTime(); };
			var target = Teleopti.MyTimeWeb.Asm;
			var ajaxOption;
			target._replaceAjax({
				Ajax: function (option) {
					ajaxOption = option;
					option.success({
						UserTimeZoneMinuteOffset: -360,
						DSTAdjustmentInMinutes: 60
					});
					return {
						done: function () { }
					};
				}
			});
			target.ShowAsm();
			equal(target.Vm().canvasPosition(), "-1020px");

		});
});
