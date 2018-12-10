Feature: API Response
	As a webclient I should receieve correct headers independent on success or errors

Scenario: Webclient requests a missing api endpoint
	Given Webclient requests a missing api endpoint
	Then The response should contain status 404 and X-Server-Version header
 