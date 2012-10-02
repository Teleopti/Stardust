/// <reference path="Teleopti.MyTimeWeb.Common.js"/>

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Communication = (function ($) {

	return {
		Init: function () {
			Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Message/Index', Teleopti.MyTimeWeb.Communication.MessagePartialInit);
		},
		MessagePartialInit: function () {
			Teleopti.MyTimeWeb.CommunicationList.Init();
		}
	};

})(jQuery);

