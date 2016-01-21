var request = require("request");

process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0"

var startTime = new Date();
var ping = function(){
	var currentTime = new Date();
	var diff = (currentTime - startTime) / 1000;
	console.log('time '+diff);
	var webUrl = process.env.UrlToTest + '/Web';
	var bridgeUrl = process.env.UrlToTest + '/AuthenticationBridge';
	var sdkUrl = process.env.UrlToTest + '/SDK/TeleoptiCCCSdkService.svc';
	
	var expectUrlToWorkWithinGivenTime = function (url, intervalIdCallback) {
		request(url, function (error, response, body) {
			if (!error && response.statusCode == 200) {
				console.log(url + " is up and running.");
				clearInterval(intervalIdCallback());
			} else {
				var currentTime = new Date();
				var diff = (currentTime - startTime) / 1000;
				console.log(url + " isn't available for " + diff + " seconds.");
				
				if (diff > 1200) {
					clearInterval(intervalIdCallback());
					throw new Error(url + " isn't available for 20 minutes.");
				}
			}
		});
	}
	
	var refreshIntervalIdWebUrl = setInterval(function(){
		expectUrlToWorkWithinGivenTime(webUrl,function (){return refreshIntervalIdWebUrl;});
	}, 10000);
	
	var refreshIntervalIdBridgeUrl = setInterval(function () {
		expectUrlToWorkWithinGivenTime(bridgeUrl,function (){return refreshIntervalIdBridgeUrl;});
	}, 10000);
	
	var refreshIntervalIdSdkUrl = setInterval(function () {
		expectUrlToWorkWithinGivenTime(sdkUrl,function (){return refreshIntervalIdSdkUrl;});
	}, 10000);
};


// cloud deploy and jenkins auto deploy cannot be finished within 5 minutes
setTimeout(ping, process.env.WaitingTime);
