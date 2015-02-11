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
	log('shutdown client.');
	client.end(function(){
		log('shutdown selenium server');
		request('http://localhost:4444/selenium-server/driver/?cmd=shutDownSeleniumServer',function (error, response, body) {
			throw new Error(msg);
		});
	});
};	

client.init();
log('navigate to url ' + process.env.UrlToTest);
client.url(process.env.UrlToTest)
	.waitForExist('#Username-input', 60000, false, function(err, res, response) {
		if (err || !res) {
			closeAndThrow('failed to navigate to sign in page.');
		}
	});
log('try to sign in');
client.setValue('#Username-input', 'demo')
	.setValue('#Password-input', 'demo')
	.click('#Signin-button')
	.waitForExist('.user-name', 60000, false, function(err, res, response) {
		if (err || !res) {
			closeAndThrow('failed to sign in.');
		}
		log('sign in succeeded');
	});
log('navigate to health check');
client.url(process.env.UrlToTest + '/HealthCheck')
	.waitForEnabled('#Start-Check', 60000, false, function(err, res, response) {
		if (err || !res) {
			closeAndThrow('failed to navigate to health check page.');
		}
	});
log('check service bus and broker');
client.pause(3000);
client.click('#Start-Check')
	.waitForExist('#Bus-Results', 600000, false, function(err, res, response) {
		if (err || !res) {
			closeAndThrow('service bus doesnot work well after trying 10 minutes.');
		}
		log('service bus and broker work well');
	});
	
log('shutdown client.');
client.end();
log('shutdown selenium server');
client.call(function(){
	request('http://localhost:4444/selenium-server/driver/?cmd=shutDownSeleniumServer');
});


