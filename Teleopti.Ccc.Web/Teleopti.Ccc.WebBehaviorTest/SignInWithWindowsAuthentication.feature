Feature: SignIn with windows authentication
	In order to access the site
	As a user that is not signed in
	I want to be able to sign in by selecting windows authentication

Background:
	Given the application is configured to use windows authentication

Scenario: See windows authentication choice
	Given I am viewing the authentication start page
	Then I should see windows authentication choice
