USE master

DROP DATABASE IF EXISTS payroll;

CREATE DATABASE payroll;

USE payroll;

CREATE TABLE Rate(
	RateID int PRIMARY KEY IDENTITY(1,1),
	RateDesc nvarchar(100),
	Rate float(1) NOT NULL,
	IsDisabled BIT DEFAULT (0)
);

CREATE TABLE user_group(
	GroupID int PRIMARY KEY IDENTITY(1,1),
	GroupName nvarchar(60) NOT NULL,
	RateID int NOT NULL,
	ShowAdminSettings BIT DEFAULT 0,
	EnableFaceRec BIT DEFAULT 0,
	IsDisabled BIT DEFAULT (0),
	FOREIGN KEY (RateID) REFERENCES rate(rateID)
);

CREATE TABLE Users(
	UserID nvarchar(60) PRIMARY KEY,
	FullName nvarchar(100),
	FromAD BIT DEFAULT 0,
	IsDisabled BIT DEFAULT 0,
	GroupID int,
	FOREIGN KEY (GroupID) REFERENCES user_group(GroupID)
);

CREATE TABLE Location(	
	LocationID int PRIMARY KEY IDENTITY(1,1),
	LocationName nvarchar(36) UNIQUE,
	EnableGM BIT DEFAULT 0,
	IsDisabled BIT DEFAULT 0,
	Shiftless BIT DEFAULT 0
);

CREATE TABLE Shifts(
	ShiftID int PRIMARY KEY IDENTITY(1,1),
	ShiftName nvarchar(20),
	StartTime time(1) NOT NULL,
	EndTime time(1) NOT NULL,
	LocationID int,
	RateID int,
	IsDisabled BIT DEFAULT 0,
	WeekendOnly BIT DEFAULT 0,
	FOREIGN KEY (RateID) REFERENCES Rate(RateID),
	FOREIGN KEY (LocationID) REFERENCES Location(LocationID)
);

CREATE TABLE Meeting(
	MeetingID int PRIMARY KEY IDENTITY(1,1),
	MeetingName nvarchar(20),
	LocationID int,
	MeetingDay int NOT NULL,
	IsDisabled BIT DEFAULT 0,
	RateID int,
	StartTime time,
	FOREIGN KEY (LocationID) REFERENCES Location(LocationID),
	FOREIGN KEY (RateID) REFERENCES Rate(rateID)
);

CREATE TABLE Meeting_Group(
	meeting_group_id int PRIMARY KEY IDENTITY(1,1),
	MeetingID int,
	UserGroupID int,
	FOREIGN KEY (meetingID) REFERENCES Meeting(MeetingID),
	FOREIGN KEY (UserGroupID) REFERENCES user_group(GroupID)
);

CREATE TABLE Activity(
	ActivityID int PRIMARY KEY IDENTITY(1,1),
	UserID nvarchar(60),
	LocationID int,
	InTime DATETIME,
	OutTime DATETIME,
	StartShift int,
	EndShift int,
	MeetingID int,
	SpecialTask BIT DEFAULT 0,
	ApprovedHours float,
	ClaimableAmount float,
	ApplicableRate int,
	ClaimDate date,
	FOREIGN KEY (UserID) REFERENCES Users(UserID),
	FOREIGN KEY (LocationID) REFERENCES Location(LocationID),
	FOREIGN KEY (StartShift) REFERENCES Shifts(ShiftID),
	FOREIGN KEY (endShift) REFERENCES Shifts(ShiftID),
	FOREIGN KEY (MeetingID) REFERENCES Meeting(MeetingID),
	FOREIGN KEY (ApplicableRate) REFERENCES Rate(RateID)
);

CREATE TABLE Global_Settings(
	SettingKey nvarchar(60) PRIMARY KEY,
	SettingValue nvarchar(255)
);

/* DUMMY DATA */

INSERT INTO global_settings VALUES('MinHours', '40');
INSERT INTO global_settings VALUES('DefaultTraineeGroup', '1');
INSERT INTO global_settings VALUES('DefaultGroup', '2');
INSERT INTO global_settings VALUES('BreakDuration', '0:30');
INSERT INTO global_settings VALUES('NeedBreakDuration', '6:00');

INSERT INTO rate(rateDesc, rate) VALUES('Default Duty', 5);
INSERT INTO rate(rateDesc, rate) VALUES('Sunday Special Task', 10);
INSERT INTO rate(rateDesc, rate) VALUES('Chiefs Normal Duty or Special Task', 8);
INSERT INTO rate(rateDesc, rate) VALUES('Zero', 0);

INSERT INTO user_group(groupName, RateID, ShowAdminSettings, EnableFaceRec) VALUES('Trainee', 4, 0, 0);
INSERT INTO user_group(groupName, RateID, ShowAdminSettings, EnableFaceRec) VALUES('Technical Assistant', 1, 0, 1);
INSERT INTO user_group(groupName, RateID, ShowAdminSettings, EnableFaceRec) VALUES('Board Member', 1, 0, 1);
INSERT INTO user_group(groupName, RateID, ShowAdminSettings, EnableFaceRec) VALUES('Board Member, HR Member', 1, 1, 1);
INSERT INTO user_group(groupName, RateID, ShowAdminSettings, EnableFaceRec) VALUES('Chief', 1, 1, 1);
INSERT INTO user_group(groupName, RateID, ShowAdminSettings, EnableFaceRec) VALUES('Intern/Part Time', 1, 1, 1);

INSERT INTO Users VALUES('ushan@cloudmails.apu.edu.my', 'Ahmed Ushan Mohamed', 1, 0, 2);
INSERT INTO Users VALUES('aisha@cloudmails.apu.edu.my', 'Aisha Kurmangali', 1, 0, 4);
INSERT INTO Users VALUES('alfredo@cloudmails.apu.edu.my', 'Alfredo', 1, 0, 4);
INSERT INTO Users VALUES('yi.hong@cloudmails.apu.edu.my', 'Chai Yi Hong', 1, 0, 2);
INSERT INTO Users VALUES('hui.wen@cloudmails.apu.edu.my', 'Chen Hui Wen', 1, 0, 3);
INSERT INTO Users VALUES('sin.yuen@cloudmails.apu.edu.my', 'Chen Sin Yuen', 1, 0, 3);
INSERT INTO Users VALUES('wei.lun@cloudmails.apu.edu.my', 'Cheng Wei Lun', 1, 0, 3);
INSERT INTO Users VALUES('xue.qian@cloudmails.apu.edu.my', 'Chia Xue Qian', 1, 0, 2);
INSERT INTO Users VALUES('erwin.suwito@taportalteams.onmicrosoft.com', 'Erwin Suwitoandojo', 1, 0, 4);
INSERT INTO Users VALUES('TP045000@mail.apu.edu.my', 'ERWIN SUWITOANDOJO', 1, 0, 1);
INSERT INTO Users VALUES('wai.tuck@cloudmails.apu.edu.my', 'Foong Wai Tuck', 1, 0, 2);
INSERT INTO Users VALUES('ka.wang', 'Ho Ka Wang',  0, 0, 6);

INSERT INTO Location(locationName, enableGM, isDisabled) VALUES('new-sys', 1, 1);
INSERT INTO Location(locationName, enableGM) VALUES('APU', 1);
INSERT INTO Location(locationName, enableGM) VALUES('APIIT', 0);

INSERT INTO Meeting(locationID, meetingName, meetingDay, rateID, StartTime)  VALUES(2, 'GM', 2, 1, '18:15:00');
INSERT INTO Meeting(locationID, meetingName, meetingDay, rateID, StartTime)  VALUES(2, 'BMM', 1, 1, '18:30:00');

INSERT INTO meeting_group(meetingID, UserGroupID) VALUES(1, 1);
INSERT INTO meeting_group(meetingID, UserGroupID) VALUES(1, 2);
INSERT INTO meeting_group(meetingID, UserGroupID) VALUES(1, 3);
INSERT INTO meeting_group(meetingID, UserGroupID) VALUES(1, 4);
INSERT INTO meeting_group(meetingID, UserGroupID) VALUES(1, 5);

INSERT INTO meeting_group(meetingID, UserGroupID) VALUES(2, 3);
INSERT INTO meeting_group(meetingID, UserGroupID) VALUES(2, 4);
INSERT INTO meeting_group(meetingID, UserGroupID) VALUES(2, 5);

INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID, isDisabled) VALUES('Special Task', '00:00', '23:59:59', '2', '1', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID, isDisabled) VALUES('Normal sign in', '00:00', '23:59:59', '2', '1', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S1', '08:15', '10:30', '2', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S2', '10:30', '12:30', '2', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S3', '12:30', '14:30', '2', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S4', '14:30', '16:30', '2', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S5', '16:30', '18:30', '2', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S6', '18:30', '21:30', '2', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID, WeekendOnly) VALUES('Saturday', '09:00', '19:00', '2', '1', '1');

INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID, isDisabled) VALUES('Special Task', '00:00', '23:59:59', '3', '1', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID, isDisabled) VALUES('Normal sign in', '00:00', '23:59:59', '2', '1', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S1', '08:15', '10:30', '3', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S2', '10:30', '12:30', '3', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S3', '12:30', '14:30', '3', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S4', '14:30', '16:30', '3', '1');
INSERT INTO shifts(shiftName, startTime, endTime, locationID, rateID) VALUES('S5', '16:30', '18:30', '3', '1');
