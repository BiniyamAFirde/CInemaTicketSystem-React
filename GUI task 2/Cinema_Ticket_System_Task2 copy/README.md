🎬 Cinema Ticket System
A modern, full-featured cinema ticket booking system built with ASP.NET Core MVC, designed for seamless movie ticket management and booking experiences.

📋 Project Information
Course: Graphical User Interfaces (EGUI)
Academic Year: 2024-2025
Institution: Warsaw University of Technology
Faculty: Faculty of Electronics and Information Technology
Student: Biniyam Firde
Repository: GitLab Project


🛠️ Technology Stack







Core Technologies


Backend: ASP.NET Core 9.0 MVC

ORM: Entity Framework Core 9.0

Database: MySQL 8.0

Authentication: ASP.NET Core Identity

Frontend: Bootstrap 5.3, HTML5, CSS3, JavaScript

Containerization: Docker & Docker Compose

Version Control: Git



✨ Features

🎭 User Features


🔐 Account Management: User registration, login, and profile management

🎥 Browse Movies: View latest movies with detailed information
🎞️ View Screenings: Browse available screenings by movie and cinema

💺 Interactive Seat Selection: Real-time seat map interface

📅 Book Tickets: Reserve multiple seats for screenings

📋 Booking History: View and manage all personal bookings

❌ Cancel Bookings: Cancel reservations with automatic seat release


👨‍💼 Administrator Features


🎬 Movie Management: Add, edit, and delete movies

🏛️ Cinema Management: Manage cinema halls and seating configurations

📅 Screening Management: Create, edit, and delete screenings

👥 User Management: View and manage user accounts and roles

📊 System Monitoring: View all bookings and screenings


🔧 Technical Features


🔐 Secure authentication with ASP.NET Core Identity

🔄 Concurrency control to prevent double bookings

🎨 Responsive design with modern UI

🐳 Docker containerization for easy deployment

🗄️ MySQL database with Entity Framework Core

⚡ Real-time seat availability updates



📦 Prerequisites
Before running this project, ensure you have:


.NET 9.0 SDK or later
Docker Desktop
Docker Compose
Git



🚀 Installation & Setup

Option 1: Using Docker (Recommended)
1. Clone the Repository

git clone https://gitlab-stud.elka.pw.edu.pl/25z-egui/mvc/25Z-EGUI-MVC-Firde-Biniyam.git
cd 25Z-EGUI-MVC-Firde-Biniyam


2. Start with Docker Compose

docker-compose up --build -d


3. Access the Application

Open browser: http://localhost or http://localhost:80

Application automatically creates database and seeds initial data

4. Stop the Application

docker-compose down


5. Reset Database (Optional)

docker-compose down -v
docker-compose up --build -d




Option 2: Local Development
1. Clone the Repository

git clone https://gitlab-stud.elka.pw.edu.pl/25z-egui/mvc/25Z-EGUI-MVC-Firde-Biniyam.git
cd 25Z-EGUI-MVC-Firde-Biniyam


2. Configure MySQL Database
Create database:

CREATE DATABASE CinemaTicketSystem CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'cinema_user'@'localhost' IDENTIFIED BY 'cinema_pass';
GRANT ALL PRIVILEGES ON CinemaTicketSystem.* TO 'cinema_user'@'localhost';
FLUSH PRIVILEGES;


3. Update Connection String
Edit appsettings.json:

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=CinemaTicketSystem;Uid=cinema_user;Pwd=cinema_pass;"
  }
}


4. Install Dependencies

dotnet restore


5. Apply Migrations

dotnet ef database update


6. Run Application

dotnet run


7. Access Application

Navigate to: http://localhost:5087




💻 Usage Guide

🔑 Default Admin Credentials
Email: admin@cinema.com
Password: Admin@123
⚠️ Important: Change admin password after first login in production!


For Regular Users
1. Register/Login

Click "Register" to create account
Provide email, password, and personal details
Login with credentials

2. Browse Movies

View latest movies on homepage
Click "Details" for movie information
Click "View Screenings" for showtimes

3. Book Tickets

Select screening
Choose seats on interactive map
Green = Available, Red = Booked, Blue = Your Selection
Confirm booking
View confirmation details

4. Manage Bookings

Navigate to "My Bookings" in user menu
View all reservations
Cancel bookings if needed



For Administrators
1. Access Admin Panel

Login with admin credentials
Access admin features from navigation menu

2. Manage Movies

Add new movies with details (title, genre, duration, etc.)
Edit existing movies
Delete movies

3. Manage Screenings

Create screenings for movies
Set date, time, cinema, and ticket price
Edit or delete screenings

4. Manage Users

View all registered users
Assign/remove admin roles
Delete user accounts



📁 Project Structure

Cinema_Ticket_System/
├── Controllers/                    # MVC Controllers
│   ├── HomeController.cs          # Homepage and movie listing
│   ├── AccountController.cs       # User authentication
│   ├── BookingController.cs       # Ticket booking logic
│   ├── ScreeningController.cs     # Screening management
│   └── UserManagementController.cs # Admin user management
│
├── Models/                        # Data Models
│   ├── Movie.cs                   # Movie entity
│   ├── Cinema.cs                  # Cinema hall entity
│   ├── Screening.cs               # Screening entity
│   ├── Booking.cs                 # Booking entity
│   ├── Seat.cs                    # Seat entity
│   └── ApplicationUser.cs         # User entity
│
├── ViewModels/                    # View Models
│   ├── BookingViewModel.cs
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   └── ScreeningCreateViewModel.cs
│
├── Views/                         # Razor Views
│   ├── Home/                      # Homepage views
│   ├── Account/                   # Login/Register views
│   ├── Booking/                   # Booking views
│   ├── Screening/                 # Screening views
│   └── Shared/                    # Layout and shared views
│       └── _Layout.cshtml         # Main layout
│
├── Data/                          # Database Context
│   └── ApplicationDbContext.cs    # EF Core DbContext
│
├── Migrations/                    # EF Core Migrations
│   └── [Timestamp]_InitialCreate.cs
│
├── wwwroot/                       # Static Files
│   ├── css/
│   │   ├── site.css              # Custom styles
│   │   └── _Layout.cshtml.css    # Layout-specific styles
│   ├── js/
│   └── lib/
│
├── Properties/
│   └── launchSettings.json        # Launch configuration
│
├── docker-compose.yml             # Docker configuration
├── Dockerfile                     # Docker image definition
├── appsettings.json              # Application settings
├── Program.cs                     # Application entry point
└── README.md                      # This file




🗄️ Database Schema

Main Tables
Movies

Stores movie information (title, genre, duration, release date, etc.)

Cinemas

Cinema halls with seating configuration (rows, seats per row)

Screenings

Movie showtimes at specific cinemas with pricing

Bookings

User ticket reservations with total price and status

Seats

Individual seat allocation and booking status

AspNetUsers

User accounts with authentication

AspNetRoles

User roles (Admin, User)


Pre-seeded Data
7 Sample Movies:

Mission: Impossible - The Final Reckoning
Superman
Sinners
Ballerina
F1: The Movie
How to Train Your Dragon
Jurassic World: Rebirth

4 Cinema Halls:

Grand Cinema Hall (10 rows × 15 seats)
Cozy Theater (8 rows × 12 seats)
Premium Auditorium (12 rows × 20 seats)
Small Screening Room (6 rows × 10 seats)



🏗️ Architecture: MVC Pattern

Model

Defines data structure and business rules
Entity Framework Core maps classes to database tables
Includes validation attributes


View

Razor templates (.cshtml files)
Displays data using @Model

Bootstrap 5 for responsive UI
Client-side validation


Controller

Handles HTTP requests
Processes business logic
Interacts with database via EF Core
Returns views or redirects

Request Flow:

User Request → Routing → Controller → Model (EF Core) → Database
                              ↓
                         View Rendering
                              ↓
                         HTML Response




🔧 Key Technical Features

1. Entity Framework Core (ORM)

// Get all movies
var movies = await _context.Movies.ToListAsync();

// Find specific screening
var screening = await _context.Screenings
    .Include(s => s.Movie)
    .Include(s => s.Cinema)
    .FirstOrDefaultAsync(s => s.Id == id);

// Create booking
_context.Bookings.Add(newBooking);
await _context.SaveChangesAsync();



2. Concurrency Control

Row versioning prevents double bookings
Optimistic concurrency handling
Database constraints ensure data integrity


3. ASP.NET Core Identity

Secure password hashing
Role-based authorization
Session management
Cookie authentication


4. Responsive Design

Bootstrap 5 grid system
Mobile-friendly interface
Interactive seat maps
Real-time updates



🧪 Testing

User Workflows to Test
1. User Registration & Authentication

Register new account
Login with credentials
Edit profile
Logout

2. Browse & Book

View movie list
See screening details
Select seats
Confirm booking
View in "My Bookings"

3. Cancel Bookings

Navigate to "My Bookings"
Cancel reservation
Verify seats released

4. Admin Features

Login as admin
Create movie
Create screening
Manage users
View all bookings

5. Concurrency Testing

Two users book same seat simultaneously
First booking succeeds
Second booking fails with error



🐛 Troubleshooting

Database Connection Issues

# Check MySQL is running
docker ps

# View MySQL logs
docker logs cinema_mysql

# Restart containers
docker-compose restart



Port Already in Use

# Find process using port
lsof -i:80

# Kill process
kill -9 <PID>

# Or change port in docker-compose.yml



Migration Errors

# Remove last migration
dotnet ef migrations remove

# Create new migration
dotnet ef migrations add NewMigration

# Update database
dotnet ef database update



Reset Everything

# Complete reset
docker-compose down -v
docker-compose up --build -d




📚 Course Requirements Completion

✅ Task 1: Basic System (Completed)


✓ User registration and authentication

✓ User profile management

✓ Admin account creation

✓ Cinema hall setup (4 pre-configured halls)

✓ Session-based authentication


✅ Task 2: Advanced Features (Completed)


✓ Create/Edit/Delete screenings

✓ Interactive seat map visualization

✓ Seat reservation system (row, seat number)

✓ Cancel/release reservations

✓ Conflict prevention (database constraints)

✓ Display occupied seats in real-time

✓ Concurrency handling with row versioning



🚀 Future Enhancements
Potential Improvements:


□ Payment gateway integration

□ Email confirmation notifications

□ Movie ratings and reviews

□ Advanced search and filters

□ QR code tickets

□ Mobile application

□ Real-time WebSocket updates

□ Multi-language support

□ Analytics dashboard

□ Loyalty program



📖 References

ASP.NET Core Documentation
Entity Framework Core
MySQL Documentation
Bootstrap 5 Documentation
Docker Documentation



👨‍💻 Author
Biniyam Firde
Warsaw University of Technology
Faculty of Electronics and Information Technology
Academic Year: 2024-2025
Contact: 01205432@pw.edu.pl
GitLab: @25Z-EGUI-MVC-Firde-Biniyam


📄 License
This project is developed for educational purposes as part of the EGUI course at Warsaw University of Technology.
MIT License - Open source and available for educational use.


🙏 Acknowledgments

Warsaw University of Technology
EGUI Course Instructors
ASP.NET Core Community
Bootstrap Framework Team


Project Status: ✅ Complete - All required features implemented and tested.
Last Updated: December 2024

For questions or issues, please create an issue on GitLab or contact the author.
