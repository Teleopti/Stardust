import { ComponentFixture, getTestBed, TestBed } from '@angular/core/testing';

/**
 * This is a workaround for Angular to preserve the TestBed module
 * between each test. This is not always desirable, but will speed up test runs.
 */
export const configureTestSuite = () => {
	const testBedApi: any = getTestBed();
	const originReset = TestBed.resetTestingModule;

	beforeAll(() => {
		TestBed.resetTestingModule();
		TestBed.resetTestingModule = () => TestBed;
	});

	afterEach(() => {
		testBedApi._activeFixtures.forEach((fixture: ComponentFixture<any>) => fixture.destroy());
		testBedApi._instantiated = false;
	});

	afterAll(() => {
		TestBed.resetTestingModule = originReset;
		TestBed.resetTestingModule();
	});
};
