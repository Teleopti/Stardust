var request = require("request");

process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0"

var startTime = new Date();
var refreshIntervalId = setInterval(function () {
	request(process.env.UrlToTest, function (error, response, body) {
		if (!error && response.statusCode == 200) {
			console.log(process.env.UrlToTest + " is up and running.");
			clearInterval(refreshIntervalId);
		} else {
			var currentTime = new Date();
			var diff = (currentTime - startTime) / 1000;
			console.log(process.env.UrlToTest + " isn't available for " + diff + " seconds.");
			
			if (diff > 1200) {
				clearInterval(refreshIntervalId);
				throw new Error(process.env.UrlToTest + " isn't available for 20 minutes.");
			}
		}
	})
}, 10000);