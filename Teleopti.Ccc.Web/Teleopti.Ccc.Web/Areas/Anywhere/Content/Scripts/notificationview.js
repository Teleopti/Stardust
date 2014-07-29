define(
    [
		'text!templates/notification.html'
    ], function (
        notificationTemplate
    ) {

    	function _displayNotification(incoming) {

    		var display = {
    			title: "Notification!",
    			message: "No notification!"
    		};

    		if (typeof incoming != "string") {
    			display.title = incoming.title || display.title;
    			if (typeof incoming.message != "string") {
    				display.message = JSON.stringify(incoming.message);
    			} else {
    				display.message = incoming.message || display.message;
    			}
    		} else {
    			display.message = incoming;
    		}

    		var placeHolder = $('#notification-placeholder')
                .html(notificationTemplate)
    		;
    		placeHolder
                .find('strong')
                .text(display.title);
    		placeHolder
                .find('span')
                .text(display.message);
    	}

    	function _removeNotification() {
    		$('#notification-placeholder').html("");
    	}

    	return {
    		display: function (message) {
    			_displayNotification(message);
    		},
    		remove: _removeNotification
    	};

    });
