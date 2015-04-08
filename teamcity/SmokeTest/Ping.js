var request = require("request");

process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0"

var startTime = new Date();
var ping = function(){
	var currentTime = new Date();
	var diff = (currentTime - startTime) / 1000;
	console.log('time '+diff);
	var webUrl = process.env.UrlToTest + '/Web';
	var bridgeUrl = process.env.UrlToTest + '/AuthenticationBridge';
	var refreshIntervalIdWebUrl = setInterval(function () {
		request(webUrl, function (error, response, body) {
			if (!error && response.statusCode == 200) {
				console.log(webUrl + " is up and running.");
				clearInterval(refreshIntervalIdWebUrl);
			} else {
				var currentTime = new Date();
				var diff = (currentTime - startTime) / 1000;
				console.log(webUrl + " isn't available for " + diff + " seconds.");
				
				if (diff > 1200) {
					clearInterval(refreshIntervalIdWebUrl);
					throw new Error(webUrl + " isn't available for 20 minutes.");
				}
			}
		});
	}, 10000);
	
	var refreshIntervalIdBridgeUrl = setInterval(function () {
		request(bridgeUrl, function (error, response, body) {
			if (!error && response.statusCode == 200) {
				console.log(bridgeUrl + " is up and running.");
				clearInterval(refreshIntervalIdBridgeUrl);
			} else {
				var currentTime = new Date();
				var diff = (currentTime - startTime) / 1000;
				console.log(bridgeUrl + " isn't available for " + diff + " seconds.");
				
				if (diff > 1200) {
					clearInterval(refreshIntervalIdBridgeUrl);
					throw new Error(bridgeUrl + " isn't available for 20 minutes.");
				}
			}
		});
	}, 10000);
};


// cloud deploy and jenkins auto deploy cannot be finished within 5 minutes
setTimeout(ping, process.env.WaitingTime);
