-- This script can be used to manually seed the database
-- Or integrated into the application startup

USE CinemaTicketSystem;
GO

-- Seed Cinemas (if not already seeded by migrations)
IF NOT EXISTS (SELECT 1 FROM Cinemas)
BEGIN
    INSERT INTO Cinemas (Name, Rows, SeatsPerRow) VALUES
    ('Cinema Grand', 10, 15),
    ('Studio Cozy', 5, 8),
    ('Mega Screen', 12, 20),
    ('IMAX Theater', 15, 25),
    ('VIP Lounge', 4, 6);
END
GO

-- Create Admin User (Password: Admin@123)
-- Hash generated from: Admin@123
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@cinema.com')
BEGIN
    INSERT INTO Users (FirstName, LastName, Email, PhoneNumber, PasswordHash, IsAdmin)
    VALUES ('System', 'Administrator', 'admin@cinema.com', '+1234567890', 
            'generated_hash_here', 1);
END
GO

-- Create Test User (Password: User@123)
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'john.doe@email.com')
BEGIN
    INSERT INTO Users (FirstName, LastName, Email, PhoneNumber, PasswordHash, IsAdmin)
    VALUES ('John', 'Doe', 'john.doe@email.com', '+1234567891', 
            'generated_hash_here', 0);
END
GO

-- Seed Sample Screenings
IF NOT EXISTS (SELECT 1 FROM Screenings)
BEGIN
    DECLARE @Cinema1 INT = (SELECT Id FROM Cinemas WHERE Name = 'Cinema Grand');
    DECLARE @Cinema2 INT = (SELECT Id FROM Cinemas WHERE Name = 'Studio Cozy');
    DECLARE @Cinema3 INT = (SELECT Id FROM Cinemas WHERE Name = 'Mega Screen');

    INSERT INTO Screenings (CinemaId, MovieTitle, StartDateTime) VALUES
    (@Cinema1, 'The Matrix Resurrections', DATEADD(day, 3, GETDATE())),
    (@Cinema1, 'Inception', DATEADD(day, 4, GETDATE())),
    (@Cinema2, 'Dune: Part Two', DATEADD(day, 3, GETDATE())),
    (@Cinema2, 'Interstellar', DATEADD(day, 5, GETDATE())),
    (@Cinema3, 'Oppenheimer', DATEADD(day, 3, GETDATE())),
    (@Cinema3, 'The Dark Knight', DATEADD(day, 6, GETDATE()));
END
GO