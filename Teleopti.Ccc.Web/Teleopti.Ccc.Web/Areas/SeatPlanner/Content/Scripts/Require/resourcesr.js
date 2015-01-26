﻿/**
* require.js plugin that loads the resources
*/
define(function () {

    var timestamp = new Date().getTime();
    
	//API
	return {
	    load: function (name, req, onLoad, config) {
			req([req.toUrl(name)], function (mod) {
				onLoad(mod);
			});
		},
	    normalize: function (name, norm) {
	        
	        // require.js plugins seems to be called twice, dont ask me why...
	        // so, exit if we'v already been called!
	        if (name.indexOf("?") >= 0)
	            return name;
	        
	    	// adds timestamp to avoid client cache. bleh, but without it test runs may fail..
	        return 'SeatPlanner/Application/Resources?_=' + timestamp;
		}

	};
});