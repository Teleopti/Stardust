/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.MessageBroker.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Communication.List.js"/>

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.AsmMessage = (function () {

	function _onMessageBrokerEvent(notification) {
		var messageTab = $('a[href*="#MessageTab"]');
		messageTab.addClass("asm-new-message-indicator");
		var firstParenthesis = messageTab.text().indexOf('(');
		if (firstParenthesis > -1) {
			var messageCount = messageTab.text().substring(firstParenthesis + 1, messageTab.text().length - 1);
			messageTab.text(messageTab.text().replace(messageCount, parseInt(messageCount) + 1));
			return;
		}

		messageTab.text(messageTab.text() + ' (1)');
	};

	function test() {
		//console.log('fel');
	}

	function _listenForEvents() {
		Teleopti.MyTimeWeb.Ajax.Ajax({
			url: '/MyTime/MessageBroker/FetchUserData',
			dataType: "json",
			type: 'GET',
			success: function (data) {
				Teleopti.MyTimeWeb.MessageBroker.AddSubscription({
					url: data.Url,
					callback: _onMessageBrokerEvent,
					errCallback: test,
					domainType: 'IPushMessageDialogue',
					businessUnitId: data.BusinessUnitId,
					datasource: data.DataSourceName,
					referenceId: data.AgentId
				});
			}
		});
	}

	return {
		Init: function () {
			_listenForEvents();
		},
		OnMessageBrokerEvent: function () {
			_onMessageBrokerEvent();
		}
	};
})(jQuery);