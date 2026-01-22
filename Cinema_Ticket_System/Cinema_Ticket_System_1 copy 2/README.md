# Cinema Ticket Booking System

A web-based cinema ticket reservation system built with ASP.NET Core MVC, enabling users to browse screenings, reserve seats in real-time, and manage bookings with administrative controls.

## Project Information

**Course:** Graphical User Interface (EGUI)  
**Academic Year:** 2025-2026  
**Institution:** Warsaw University of Technology  
**Faculty:** Faculty of Electronics and Information Technology  
**Student:** Yonatan Firde  
**Project Repository:** [GitLab Repository](https://gitlab-stud.elka.pw.edu.pl/25z-egui/mvc/25Z-EGUI-MVC-Firde-Yonatan.git)

## Technology Stack

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=flat-square&logo=.net&logoColor=white)
![MySQL](https://img.shields.io/badge/MySQL-5.7.24-4479A1?style=flat-square&logo=mysql&logoColor=white)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-512BD4?style=flat-square)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=flat-square&logo=bootstrap&logoColor=white)

## System Architecture

- **Framework:** ASP.NET Core 9.0 MVC
- **ORM:** Entity Framework Core
- **Authentication:** ASP.NET Core Identity
- **Database:** MySQL 5.7.24
- **HTML5** with Razor syntax
- **CSS3** with Bootstrap 5.3

## Features

### User Functionalities
- **Account Management:** User registration, login, and profile management
- **Screening Browser:** View available movie screenings with real-time seat availability
- **Seat Selection:** Interactive seat map interface
- **Booking Management:** Create, view, and cancel reservations
- **Booking History:** Track all past and upcoming bookings

### Administrative Functionalities
- **Screening Management:** Create, edit, and delete movie screenings
- **Cinema Management:** Manage cinema halls and seating configurations
- **User Management:** View and manage user accounts
- **Role-Based Access Control:** Separate permissions for users and administrators

## Project Structure
```
CinemaTicketSystem/
├── Controllers/              # MVC Controllers
│   ├── AccountController.cs
│   ├── BookingController.cs
│   ├── HomeController.cs
│   ├── MovieController.cs
│   ├── ScreeningController.cs
│   └── UserManagementController.cs
├── Models/                   # Domain models
│   ├── ApplicationUser.cs
│   ├── Booking.cs
│   ├── Cinema.cs
│   ├── ErrorViewModel.cs
│   ├── Movie.cs
│   ├── Screening.cs
│   ├── Seat.cs
│   └── Ticket.cs
├── ViewModels/              # View-specific models
│   ├── BookingViewModel.cs
│   ├── LoginViewModel.cs
│   ├── ProfileEditViewModel.cs
│   ├── ProfileViewModel.cs
│   ├── RegisterViewModel.cs
│   └── ScreeningCreateViewModel.cs
├── Views/                   # Razor views
│   ├── Account/
│   ├── Admin/
│   ├── Booking/
│   ├── Home/
│   ├── Movie/
│   ├── Screening/
│   ├── Shared/
│   └── UserManagement/
├── Data/                    # Database context
│   ├── ApplicationDbContext.cs
│   └── Migrations/
├── wwwroot/                 # Static resources
│   ├── css/
│   ├── js/
│   └── lib/
├── Properties/
│   └── launchSettings.json
├── appsettings.json         # Application configuration
├── Program.cs               # Application entry point
└── README.md
```

## Installation and Setup

### Prerequisites
- **.NET SDK 9.0** or higher
- **MySQL Server 5.7.24** or higher
- **Visual Studio 2022** or **Visual Studio Code**
- **Git**

### Step 1: Clone the Repository
```bash
git clone https://gitlab-stud.elka.pw.edu.pl/25z-egui/mvc/25Z-EGUI-MVC-Firde-Yonatan.git
cd CinemaTicketSystem
```

### Step 2: Database Configuration

#### Create MySQL Database
```bash
mysql -u root -p
```
```sql
CREATE DATABASE CinemaDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'cinema_user'@'localhost' IDENTIFIED BY 'cinema_pass123';
GRANT ALL PRIVILEGES ON CinemaDb.* TO 'cinema_user'@'localhost';
FLUSH PRIVILEGES;
EXIT;
```

#### Configure Connection String
Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=CinemaDb;Uid=cinema_user;Pwd=cinema_pass123;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### Step 3: Install Dependencies and Run Migrations
```bash
# Restore NuGet packages
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the application
dotnet run
```

### Step 4: Access the Application
- **Application URL:** `https://localhost:5087` or `http://localhost:5087`
- **Default Admin Credentials:**
  - Email: `admin@cinema.com`
  - Password: `Admin@123`

## Database Schema

### Core Tables
- **AspNetUsers:** User accounts and authentication
- **AspNetRoles:** User roles (Admin, User)
- **Cinemas:** Cinema hall information
- **Movies:** Movie details
- **Screenings:** Movie screening schedules
- **Seats:** Seating configuration per cinema
- **Bookings:** User reservations
- **Tickets:** Individual tickets per booking

### Entity Relationships
- Cinema **1:N** Screening
- Movie **1:N** Screening
- Screening **1:N** Booking
- User **1:N** Booking
- Booking **1:N** Ticket
- Seat **1:N** Ticket

## Usage Guide

### For Users

#### 1. Registration
- Navigate to the registration page
- Provide email, password, full name, and phone number
- Submit to create account

#### 2. Browse Screenings
- Access "Screenings" from navigation menu
- View available movies with dates and times
- Check real-time seat availability

#### 3. Book Tickets
- Select desired screening
- Choose seats from interactive seat map
- Confirm booking details
- View confirmation in "My Bookings"

#### 4. Manage Bookings
- Access "My Bookings" page
- View all active and past reservations
- Cancel bookings if necessary

### For Administrators

#### 1. Access Admin Panel
- Login with administrative credentials
- Navigate to Admin Panel from menu

#### 2. Create Screening
- Select "Create Screening"
- Choose cinema hall
- Enter movie title
- Set date and time
- Submit to create

#### 3. Manage System
- View all screenings and bookings
- Edit or delete screenings
- Manage user accounts and roles
- Monitor system activity

## Future Enhancements
- Implementation of reservation/cancellation of seat reservation at a screening
- Conflict handling (reservation of the same seat by several concurrent users)
- Implementation of displaying occupied seats at a selected screening
- Added parallelism when editing a user/deleting a user
## Troubleshooting

### Database Connection Issues
```bash
# Test MySQL connection
mysql -u cinema_user -p -h localhost CinemaDb

# Reset database if needed
dotnet ef database drop --force
dotnet ef database update
```

### Port Already in Use
Edit `Properties/launchSettings.json` to change the port number.

### Migration Errors
```bash
# Remove all migrations
dotnet ef migrations remove

# Create new migration
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## References and Resources
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [MySQL Documentation](https://dev.mysql.com/doc/)
- [Bootstrap Documentation](https://getbootstrap.com/docs/)

## License
This project is developed for academic purposes. All rights reserved © 2025 Yonatan Firde.

---

**For questions or issues, please contact:** yonatanawlachew1@gmail.com