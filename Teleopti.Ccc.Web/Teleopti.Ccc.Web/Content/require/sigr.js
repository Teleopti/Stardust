/**
* A modified copy of noext that doesnt add query string parameter.
* works with signalr
*/
define(function () {

	//API
	return {
		load: function (name, req, onLoad, config) {
			req([req.toUrl(name)], function (mod) {
				onLoad(mod);
			});
		},
		normalize: function (name, norm) {
			//append ? to avoid adding .js extension
			//needs to be on normalize otherwise it won't work after build
			name += (name.indexOf('?') < 0) ? '?' : '';
			return name;
		}

	};
});