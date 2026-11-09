-- RaceDay_SampleData.sql  (OPTIONAL demo data)
-- Run AFTER you have registered at least one Organiser and one Participant in the app.
-- Creates 4 events owned by the first Organiser, categories for each, and one
-- enrolment + result for the first Participant on the completed event.
USE RaceDayDb;

DECLARE @org INT = (SELECT TOP 1 UserID FROM dbo.Users WHERE Role = 'Organiser'   ORDER BY UserID);
DECLARE @par INT = (SELECT TOP 1 UserID FROM dbo.Users WHERE Role = 'Participant' ORDER BY UserID);

IF @org IS NULL OR @par IS NULL
BEGIN
    RAISERROR('Register at least one Organiser and one Participant through the app first.', 16, 1);
    RETURN;
END

IF NOT EXISTS (SELECT 1 FROM dbo.Events WHERE EventName = 'Cape Town 10K')
BEGIN
    INSERT INTO dbo.Events (EventName, Description, EventDate, Location, DistanceKm, EventTypeID, OrganiserID, MaxParticipants, Status)
    VALUES
    ('Cape Town 10K',          'Scenic city 10K run',          DATEADD(DAY, 30, SYSUTCDATETIME()), 'Cape Town',    10.00, 1, @org, 500, 'Open'),
    ('Durban Beach Walk',      'Relaxed beachfront walk',      DATEADD(DAY, 45, SYSUTCDATETIME()), 'Durban',        5.00, 2, @org, 300, 'Open'),
    ('Joburg Cycle Challenge', '40km road cycling challenge',  DATEADD(DAY, 60, SYSUTCDATETIME()), 'Johannesburg', 40.00, 3, @org, 400, 'Open'),
    ('Soweto 21K',             'Half marathon through Soweto', DATEADD(DAY, -30, SYSUTCDATETIME()), 'Soweto',     21.10, 1, @org, 600, 'Completed');

    INSERT INTO dbo.Categories (EventID, CategoryName, MinAge, MaxAge, EntryFee, MaxEntries)
    SELECT EventID, 'Open', 16, NULL, 150.00, 300 FROM dbo.Events WHERE OrganiserID = @org;
    INSERT INTO dbo.Categories (EventID, CategoryName, MinAge, MaxAge, EntryFee, MaxEntries)
    SELECT EventID, 'Junior', 10, 15, 80.00, 100 FROM dbo.Events WHERE OrganiserID = @org;

    DECLARE @evt INT = (SELECT EventID FROM dbo.Events WHERE EventName = 'Soweto 21K');
    DECLARE @cat INT = (SELECT TOP 1 CategoryID FROM dbo.Categories WHERE EventID = @evt AND CategoryName = 'Open');

    INSERT INTO dbo.Enrolments (ParticipantID, EventID, CategoryID, Status) VALUES (@par, @evt, @cat, 'Confirmed');
    DECLARE @enr INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Results (EnrolmentID, FinishTime, Position, Notes, CapturedBy)
    VALUES (@enr, '01:52:30', 12, 'Sample result', @org);
END
