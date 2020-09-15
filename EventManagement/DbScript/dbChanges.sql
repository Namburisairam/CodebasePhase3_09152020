IF NOT EXISTS (  
SELECT * 
  FROM   INFORMATION_SCHEMA.COLUMNS 
where TABLE_NAME = 'Event' and COLUMN_NAME = 'Message'
)
begin
ALTER TABLE Event
    ADD Message nvarchar(max)
end


IF NOT EXISTS (  
SELECT * 
  FROM   INFORMATION_SCHEMA.COLUMNS 
where TABLE_NAME = 'Theme' and COLUMN_NAME = 'Message'
)
begin
ALTER TABLE Theme
    ADD Message nvarchar(max)
end

IF NOT EXISTS (  
SELECT * 
  FROM   INFORMATION_SCHEMA.COLUMNS 
where TABLE_NAME = 'QRCode' and COLUMN_NAME = 'CodePath'
)
begin
ALTER TABLE QRCode
    ADD CodePath nvarchar(1000)
end

IF NOT EXISTS (  
SELECT * 
  FROM   INFORMATION_SCHEMA.COLUMNS 
where TABLE_NAME = 'AppUser' and COLUMN_NAME = 'UserRole'
)
begin
ALTER TABLE AppUser
    ADD UserRole int
end

IF NOT EXISTS (  
SELECT * 
  FROM   INFORMATION_SCHEMA.COLUMNS 
where TABLE_NAME = 'Event' and COLUMN_NAME = 'ManagerUserId'
)
begin
ALTER TABLE Event
    ADD ManagerUserId int foreign key references Appuser(id)
end



IF NOT EXISTS (  
SELECT * 
  FROM   INFORMATION_SCHEMA.COLUMNS 
where TABLE_NAME = 'Event' and COLUMN_NAME = 'EnableThemes'
)

   create table Config(
   ID int IDENTITY(1,1) PRIMARY KEY,
   SupportEmail varchar(50) null,
   isActive varchar(50)
  
  )

 CREATE TABLE [dbo].[Notifications](
	[ID] [int] IDENTITY(1,1) NOT NULL ,
	[EventID] [int] NOT NULL ,
	[Text] [varchar](500) NULL,
	[AddedON] [datetime] NULL,
	[AddedBY] [int] NULL,
	[Status] [bit] NULL,
	[AttendesID] [int] NOT NULL,
	[ReadDate] [datetime] NULL,
	[Isread] [bit] NULL,
	PRIMARY KEY (ID),
    FOREIGN KEY (EventID) REFERENCES Event(id),
	FOREIGN KEY (AttendesID) REFERENCES Attendes(id))

begin
ALTER TABLE Event ADD EnableThemes bit
ALTER TABLE Event	ADD EnableActivity bit
	ALTER TABLE Event add EnableQrCode bit
	ALTER TABLE Event add EnableAttendees bit
	ALTER TABLE Event add EnableFloormap bit
	ALTER TABLE Event add EnablePhotos bit
	ALTER TABLE Event add EnableSponsor bit 
	ALTER TABLE Event add EnableComments bit
	ALTER TABLE Event add EnableGoogleApi bit
	ALTER TABLE Event ADD EnableSocialScreen bit NULL
	ALTER TABLE dbo.event ADD allow_bookmark  bit NULL 
	ALTER TABLE activites ADD GalacticActivityId  int NULL 
	ALTER TABLE Event add EnableSearchScreen bit
	ALTER TABLE Config add Subject varchar(100);
	ALTER TABLE Config add EmailBody varchar(1000);
	ALTER TABLE Activites ADD Name varchar(100)
	ALTER TABLE Config add ScreenTitle varchar(100);
	ALTER TABLE event add StartDate datetime;
		ALTER TABLE event add EndDate datetime;
	ALTER TABLE event add WebURL varchar(200);
		ALTER TABLE notifications add description varchar(1000);
	ALTER TABLE dbo.event ADD EventImage  varchar(200) NULL 
	ALTER TABLE usersession ADD DeviceToken varchar(200) NULL
	alter table usersession alter column devicetoken varchar(800)
		ALTER TABLE attendes add LinkedinURL varchar(1000)
	ALTER TABLE event add Destination varchar(1000)
		ALTER TABLE event add ClientName varchar(150)
	ALTER TABLE Notifications add ReadDate datetime ;
	ALTER TABLE Notifications add Isread bit;

end