/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.MessageBroker.js"/>

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.AsmMessage = (function () {

	function _onMessageBrokerEvent(notification) {
		$('a[href*="#MessageTab"]')
			.addClass("asm-new-message-indicator");
	};

	function _listenForEvents() {
		Teleopti.MyTimeWeb.Ajax.Ajax({
			url: '/MyTime/MessageBroker/FetchUserData',
			dataType: "json",
			type: 'GET',
			success: function (data) {
				Teleopti.MyTimeWeb.MessageBroker.AddSubscription({
					url: data.Url,
					callback: _onMessageBrokerEvent,
					//errCallback: test,
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