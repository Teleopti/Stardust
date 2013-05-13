/**
* Plugin that loads signalr/hubs:
* + Without .js extension
* + Preloads dependencies
*/
define(function () {

	//API
	return {
	    load: function (name, req, onLoad, config) {
	        req(['jquery', 'signalr'], function($, s) {
	            req([req.toUrl(name)], function (mod) {
	                onLoad(mod);
	            });
	        });
		},
		normalize: function (name, norm) {
		    //append ? to avoid adding .js extension
			return '../../../../signalr/hubs?';
		}

	};
});