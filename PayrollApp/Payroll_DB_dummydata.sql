USE master

DROP DATABASE IF EXISTS payroll;

CREATE DATABASE payroll;

USE payroll;

CREATE TABLE rate(
	rateID int PRIMARY KEY IDENTITY(1,1),
	rateDesc nvarchar(100),
	rate FLOAT(1) NOT NULL,
	isDisabled BIT DEFAULT (0)
);

CREATE TABLE usr_group(
	groupID int PRIMARY KEY IDENTITY(1,1),
	groupName nvarchar(60) NOT NULL,
	RateID int NOT NULL,
	ShowAdminSettings BIT DEFAULT 0,
	EnableFaceRec BIT DEFAULT 0
	FOREIGN KEY (RateID) REFERENCES rate(rateID)
);

CREATE TABLE usr(
	UserID nvarchar(60) PRIMARY KEY,
	fullName nvarchar(100),
	fromAD BIT DEFAULT 0,
	isDisabled BIT DEFAULT 0,
	groupID int,
	FOREIGN KEY (groupID) REFERENCES usr_group(groupID)
);

CREATE TABLE locations(	
	locationID int PRIMARY KEY IDENTITY(1,1),
	locationName nvarchar(36) UNIQUE,
	enableGM BIT DEFAULT 0,
	isDisabled BIT DEFAULT 0,
);

CREATE TABLE shifts(
	shiftID int PRIMARY KEY IDENTITY(1,1),
	shiftName nvarchar(20),
	startTime time(1) NOT NULL,
	endTime time(1) NOT NULL,
	locationID int,
	rateID int,
	isDisabled BIT DEFAULT 0,
	FOREIGN KEY (rateID) REFERENCES rate(rateID),
	FOREIGN KEY (locationID) REFERENCES locations(locationID)
);

CREATE TABLE signInOut(
	loginID int PRIMARY KEY IDENTITY(1,1),
	UserID nvarchar(60),
	inTime datetime,
	outTime datetime,
	startShift int,
	endShift int,
	approvedHours float(1),
	claimableAmount float(1),
	FOREIGN KEY (UserID) REFERENCES usr(UserID),
	FOREIGN KEY (startShift) REFERENCES shifts(shiftID),
	FOREIGN KEY (endShift) REFERENCES shifts(shiftID)
);

CREATE TABLE meetings(
	meetingID int PRIMARY KEY IDENTITY(1,1),
	meetingName nvarchar(20),
	locationID int,
	meetingDay int NOT NULL,
	disableMeeting BIT DEFAULT 0,
	FOREIGN KEY (locationID) REFERENCES locations(locationID)
);

CREATE TABLE meeting_group(
	meeting_group_id int PRIMARY KEY IDENTITY(1,1),
	meetingID int,
	usrGroupID int,
	FOREIGN KEY (meetingID) REFERENCES meetings(meetingID),
	FOREIGN KEY (usrGroupID) REFERENCES usr_group(groupID)
);

CREATE TABLE meeting_attendance(
	Attendance_ID int PRIMARY KEY IDENTITY(1,1),
	UserID nvarchar(60),
	meetingID int,
	loginID int,
	FOREIGN KEY (UserID) REFERENCES usr(UserID),
	FOREIGN KEY (meetingID) REFERENCES meetings(meetingID),
	FOREIGN KEY (loginID) REFERENCES signInOut(loginID)
);

CREATE TABLE loginActivity(
	LoginID int PRIMARY KEY IDENTITY(1,1),
	UserID nvarchar(60),
	LoginTime DATETIME,
	LocationID int,
	FOREIGN KEY (UserID) REFERENCES usr(UserID),
	FOREIGN KEY (LocationID) REFERENCES locations(locationID)
);

CREATE TABLE global_settings(
	SettingKey nvarchar(60) PRIMARY KEY,
	SettingValue nvarchar(255)
);

/* DUMMY DATA */

INSERT INTO global_settings VALUES('MinHours', '40');

INSERT INTO rate(rateDesc, rate) VALUES('Default Duty', 5);
INSERT INTO rate(rateDesc, rate) VALUES('Sunday Special Task', 10);
INSERT INTO rate(rateDesc, rate) VALUES('Chiefs Normal Duty or Special Task', 8);

INSERT INTO usr_group(groupName, RateID, ShowAdminSettings, EnableFaceRec) VALUES('Trainee', 1, 0, 0);
INSERT INTO usr_group(groupName, RateID, ShowAdminSettings, EnableFaceRec) VALUES('Technical Assistant', 1, 0, 1);
INSERT INTO usr_group(groupName, RateID, ShowAdminSettings, EnableFaceRec) VALUES('Board Member', 1, 0, 1);
INSERT INTO usr_group(groupName, RateID, ShowAdminSettings, EnableFaceRec) VALUES('Board Member, HR Member', 1, 1, 1);
INSERT INTO usr_group(groupName, RateID, ShowAdminSettings, EnableFaceRec) VALUES('Chief', 1, 1, 1);
INSERT INTO usr_group(groupName, RateID, ShowAdminSettings, EnableFaceRec) VALUES('Intern/Part Time', 1, 1, 1);

INSERT INTO usr VALUES('ushan@cloudmails.apu.edu.my', 'Ahmed Ushan Mohamed', 1, 0, 2);
INSERT INTO usr VALUES('aisha@cloudmails.apu.edu.my', 'Aisha Kurmangali', 1, 0, 4);
INSERT INTO usr VALUES('alfredo@cloudmails.apu.edu.my', 'Alfredo', 1, 0, 4);
INSERT INTO usr VALUES('yi.hong@cloudmails.apu.edu.my', 'Chai Yi Hong', 1, 0, 2);
INSERT INTO usr VALUES('hui.wen@cloudmails.apu.edu.my', 'Chen Hui Wen', 1, 0, 3);
INSERT INTO usr VALUES('sin.yuen@cloudmails.apu.edu.my', 'Chen Sin Yuen', 1, 0, 3);
INSERT INTO usr VALUES('wei.lun@cloudmails.apu.edu.my', 'Cheng Wei Lun', 1, 0, 3);
INSERT INTO usr VALUES('xue.qian@cloudmails.apu.edu.my', 'Chia Xue Qian', 1, 0, 2);
INSERT INTO usr VALUES('erwin.suwito@cloudmails.apu.edu.my', 'Erwin Suwitoandojo', 1, 0, 3);
INSERT INTO usr VALUES('TP045000@mail.apu.edu.my', 'ERWIN SUWITOANDOJO', 1, 0, 1);
INSERT INTO usr VALUES('wai.tuck@cloudmails.apu.edu.my', 'Foong Wai Tuck', 1, 0, 2);
INSERT INTO usr VALUES('ka.wang', 'Ho Ka Wang',  0, 0, 6);

INSERT INTO locations(locationName, enableGM, isDisabled) VALUES('new-sys', 1, 1);
INSERT INTO locations(locationName, enableGM) VALUES('APU', 1);
INSERT INTO locations(locationName, enableGM) VALUES('APIIT', 0);

INSERT INTO meetings(locationID, meetingName, meetingDay)  VALUES(2, 'GM', 2);
INSERT INTO meetings(locationID, meetingName, meetingDay)  VALUES(2, 'BMM', 1);

INSERT INTO meeting_group(meetingID, usrGroupID) VALUES(1, 1);
INSERT INTO meeting_group(meetingID, usrGroupID) VALUES(1, 2);
INSERT INTO meeting_group(meetingID, usrGroupID) VALUES(1, 3);
INSERT INTO meeting_group(meetingID, usrGroupID) VALUES(1, 4);
INSERT INTO meeting_group(meetingID, usrGroupID) VALUES(1, 5);

INSERT INTO meeting_group(meetingID, usrGroupID) VALUES(2, 3);
INSERT INTO meeting_group(meetingID, usrGroupID) VALUES(2, 4);
INSERT INTO meeting_group(meetingID, usrGroupID) VALUES(2, 5);

INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S1', '08:15', '10:30', '2', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S2', '10:30', '12:30', '2', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S3', '12:30', '14:30', '2', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S4', '14:30', '16:30', '2', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S5', '16:30', '18:30', '2', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S6', '18:30', '21:30', '2', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('Saturday', '09:00', '19:00', '2', '1');

INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S1', '08:15', '10:30', '3', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S2', '10:30', '12:30', '3', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S3', '12:30', '14:30', '3', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S4', '14:30', '16:30', '3', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S5', '16:30', '18:30', '3', '1');
