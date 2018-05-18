var webdriverio = require('webdriverio');
var request = require("request");
var client = webdriverio
	.remote({
		desiredCapabilities: {
			browserName: 'chrome'
		}
	});
	
var log = function(msg){
	if(client)
		client.call(function(){console.log(msg)});
};

var closeAndThrow = function(msg){
	log(msg);
	closeConnection(function () {
		throw new Error(msg);
	});
};	

var closeConnection = function (endcallback) {
	log('shutdown client.');
	if (endcallback == undefined)
		endcallback = function () {};
	client.end().then(endcallback);
}

client.init();
var webUrl = process.env.UrlToTest + '/Web/MyTime';

for (var i=1;i<=30; i++)
{
	log('navigate to url ' + webUrl+ ' attempt '+ i);
	client.url(webUrl)
		.waitForExist('#Username-input', 30000, false, function(err, res, response) {
			if (err || !res) {
				closeAndThrow('failed to navigate to sign in page. ' + err);
			}
		});
	log('try to sign in attempt '+ i);
	client.setValue('#Username-input', 'demo')
		.setValue('#Password-input', 'demo')
		.click('#Signin-button')
		.waitForExist('.user-name', 30000, false, function(err, res, response) {
			if (err || !res) {
				closeAndThrow('failed to sign in.' + err);
			}else{
				log('succeeded to sign in first time.');
			}
		});

	log('try to sign out attempt '+ i);
	client.click('.user-name-link')
		.waitForExist('#signout', 30000, false, function(err, res, response) {
			client.click('#signout');
		});
	
}
closeConnection();


