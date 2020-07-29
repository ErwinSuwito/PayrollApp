
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

INSERT INTO global_settings VALUES('MinHours', '40');
INSERT INTO global_settings VALUES('DefaultTraineeGroup', '1');
INSERT INTO global_settings VALUES('DefaultGroup', '2');
INSERT INTO global_settings VALUES('BreakDuration', '0:30');
INSERT INTO global_settings VALUES('NeedBreakDuration', '6:00');
INSERT INTO global_settings VALUES('DeptName', 'Technical Assistant');

INSERT INTO rate(rateDesc, rate) VALUES('Default Duty', 5);	

INSERT INTO Location(locationName, enableGM, isDisabled) VALUES('new-sys', 1, 1);