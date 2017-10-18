$(document).ready(function () {
	var ajax;
	var messageItem;

	module("Teleopti.MyTimeWeb.Messagelist",
		{
			setup: function () {
				setup();
			},
			teardown: function () {
			}
		});

	test("Should reload messages page in mytime when all messages are marked read",
		function () {
			Teleopti.MyTimeWeb.AsmMessageList.Init(ajax);
			for (var i = 0; i < 20; i++) {
				Teleopti.MyTimeWeb.AsmMessageList.AddNewMessageAtTop(messageItem);
			}
			
			var vm = Teleopti.MyTimeWeb.AsmMessageList.Vm();
			$.each(vm.asmMessageList(), function (position, element) {
				element.confirmReadMessage();
			});

			equal(vm.asmMessageList().length, 1);
		});

	function setup() {
		initData();
		setupAjax();
	}

	function initData() {
		messageItem = {
			"Title": "absence",
			"Message": "",
			"Sender": "Ashley Andeen",
			"Date": "\/Date(1505787537000)\/",
			"MessageId": "32450161-aabd-43ce-ab1f-a7bb00890b8b",
			"IsRead": false,
			"AllowDialogueReply": false,
			"DialogueMessages": [],
			"ReplyOptions": ["OK"],
			"MessageType": 0
		};
	}

	function setupAjax() {
		ajax = {
			Ajax: function (options) {
				if (options.url === "Message/Reply") {
					options.success();
				}
				if (options.url === "Message/Messages") {
					var data = [messageItem];
					options.success(data);
				}
			}
		};
	}
});