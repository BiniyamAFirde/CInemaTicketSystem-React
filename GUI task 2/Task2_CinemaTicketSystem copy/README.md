# 🎬 Cinema Ticket System

> A modern, feature-rich web-based cinema ticket reservation system built with ASP.NET Core MVC, enabling users to browse screenings, reserve seats in real-time, and manage bookings with comprehensive administrative controls.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=for-the-badge&logo=.net&logoColor=white)](https://docs.microsoft.com/en-us/aspnet/core/)
[![MySQL](https://img.shields.io/badge/MySQL-8.0-4479A1?style=for-the-badge&logo=mysql&logoColor=white)](https://www.mysql.com/)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)](https://getbootstrap.com/)

---

## 📋 Project Information

| **Field** | **Details** |
|-----------|-------------|
| **Course** | Graphical User Interface (EGUI) |
| **Academic Year** | 2025-2026 |
| **Institution** | Warsaw University of Technology |
| **Faculty** | Faculty of Electronics and Information Technology |
| **Developer** | Biniyam Awalachew Firde |
| **Repository** | [GitLab Repository](https://gitlab-stud.elka.pw.edu.pl/25z-egui/mvc/25Z-EGUI-MVC-Firde-Yonatan.git) |

---

## 🚀 Technology Stack

### Backend
- **Framework:** ASP.NET Core 9.0 MVC
- **ORM:** Entity Framework Core
- **Authentication:** ASP.NET Core Identity
- **Database:** MySQL 8.0

### Frontend
- **Markup:** HTML5 with Razor Syntax
- **Styling:** CSS3 with Bootstrap 5.3
- **Icons:** Font Awesome 6.4
- **JavaScript:** Vanilla JS with Bootstrap Bundle

---

## ✨ Key Features

### 👤 User Features
- **🔐 Account Management**
  - Secure user registration with email validation
  - Login/logout functionality with session management
  - Profile management and editing capabilities

- **🎥 Screening Browser**
  - View all available movie screenings
  - Real-time seat availability tracking
  - Filter by movie, date, and cinema

- **🪑 Interactive Seat Selection**
  - Visual seat map interface
  - Color-coded seat status (Available, Reserved, Occupied)
  - Multiple seat selection support

- **🎫 Booking Management**
  - Create new reservations
  - View booking history
  - Cancel upcoming bookings
  - Receive booking confirmations

### 🛠️ Administrative Features
- **📽️ Screening Management**
  - Create, edit, and delete movie screenings
  - Set pricing and schedules
  - Manage cinema hall assignments

- **🏢 Cinema Management**
  - Configure cinema halls
  - Define seating layouts (rows × seats)
  - Manage multiple cinema locations

- **👥 User Management**
  - View all registered users
  - Manage user roles and permissions
  - Monitor user activity

- **🔒 Role-Based Access Control**
  - Separate permissions for users and administrators
  - Secure admin panel access
  - Protected administrative routes

---

## 📁 Project Structure

```
CinemaTicketSystem/
│
├── 📂 Controllers/              # MVC Controllers
│   ├── AccountController.cs         # User authentication & profile
│   ├── BookingController.cs         # Ticket booking operations
│   ├── HomeController.cs            # Homepage & navigation
│   ├── MovieController.cs           # Movie management (Admin)
│   ├── ScreeningController.cs       # Screening CRUD operations
│   └── UserManagementController.cs  # User administration
│
├── 📂 Models/                   # Domain Models
│   ├── ApplicationUser.cs           # Extended user model
│   ├── Booking.cs                   # Booking entity
│   ├── Cinema.cs                    # Cinema hall entity
│   ├── Movie.cs                     # Movie entity
│   ├── Screening.cs                 # Screening entity
│   ├── Seat.cs                      # Seat configuration
│   └── Ticket.cs                    # Individual ticket
│
├── 📂 ViewModels/               # View-Specific Models
│   ├── BookingViewModel.cs          # Booking display model
│   ├── LoginViewModel.cs            # Login form model
│   ├── ProfileViewModel.cs          # User profile display
│   ├── RegisterViewModel.cs         # Registration form
│   └── ScreeningCreateViewModel.cs  # Screening creation form
│
├── 📂 Views/                    # Razor Views
│   ├── 📁 Account/                  # Authentication views
│   ├── 📁 Booking/                  # Booking views
│   ├── 📁 Home/                     # Homepage views
│   ├── 📁 Movie/                    # Movie management views
│   ├── 📁 Screening/                # Screening views
│   ├── 📁 Shared/                   # Layout & partials
│   └── 📁 UserManagement/           # User admin views
│
├── 📂 Data/                     # Database Layer
│   ├── ApplicationDbContext.cs      # EF Core context
│   └── 📁 Migrations/               # Database migrations
│
├── 📂 wwwroot/                  # Static Assets
│   ├── 📁 css/                      # Stylesheets
│   ├── 📁 js/                       # JavaScript files
│   └── 📁 lib/                      # Client libraries
│
├── 📂 Properties/
│   └── launchSettings.json          # Application launch config
│
├── 📄 appsettings.json          # Application configuration
├── 📄 Program.cs                # Application entry point
└── 📄 README.md                 # Documentation
```

---

## 🔧 Installation & Setup

### Prerequisites

Before you begin, ensure you have the following installed:

- [.NET SDK 9.0+](https://dotnet.microsoft.com/download)
- [MySQL Server 8.0+](https://dev.mysql.com/downloads/mysql/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

---

### Step 1️⃣: Clone the Repository

```bash
git clone https://gitlab-stud.elka.pw.edu.pl/25z-egui/mvc/25Z-EGUI-MVC-Firde-Yonatan.git
cd CinemaTicketSystem
```

---

### Step 2️⃣: Database Configuration

#### Create MySQL Database

```bash
mysql -u root -p
```

Execute the following SQL commands:

```sql
CREATE DATABASE CinemaDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'cinema_user'@'localhost' IDENTIFIED BY 'cinema_pass123';
GRANT ALL PRIVILEGES ON CinemaDb.* TO 'cinema_user'@'localhost';
FLUSH PRIVILEGES;
EXIT;
```

#### Configure Connection String

Open `appsettings.json` and update the connection string:

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

---

### Step 3️⃣: Install Dependencies & Run Migrations

```bash
# Restore NuGet packages
dotnet restore

# Apply database migrations
dotnet ef database update

# Build the project
dotnet build

# Run the application
dotnet run
```

---

### Step 4️⃣: Access the Application

🌐 **Application URL:** 
- HTTPS: `https://localhost:5087`
- HTTP: `http://localhost:5087`

🔑 **Default Admin Credentials:**
- **Email:** `admin@cinema.com`
- **Password:** `Admin@123`

---

## 🗄️ Database Schema

### Entity Relationship Diagram

```
┌─────────────┐       ┌─────────────┐       ┌─────────────┐
│   Cinema    │       │    Movie    │       │    User     │
├─────────────┤       ├─────────────┤       ├─────────────┤
│ Id          │       │ Id          │       │ Id          │
│ Name        │───┐   │ Title       │───┐   │ Email       │
│ Rows        │   │   │ Genre       │   │   │ Name        │
│ SeatsPerRow │   │   │ Duration    │   │   │ Phone       │
└─────────────┘   │   └─────────────┘   │   └─────────────┘
                  │                     │           │
                  └──────┐     ┌────────┘           │
                         │     │                    │
                  ┌──────▼─────▼──────┐             │
                  │    Screening      │             │
                  ├───────────────────┤             │
                  │ Id                │             │
                  │ MovieId (FK)      │             │
                  │ CinemaId (FK)     │             │
                  │ DateTime          │             │
                  │ TicketPrice       │             │
                  └─────────┬─────────┘             │
                            │                       │
                            └───────┐       ┌───────┘
                                    │       │
                             ┌──────▼───────▼──┐
                             │    Booking      │
                             ├─────────────────┤
                             │ Id              │
                             │ UserId (FK)     │
                             │ ScreeningId(FK) │
                             │ TotalPrice      │
                             │ BookingDate     │
                             └────────┬────────┘
                                      │
                              ┌───────▼───────┐
                              │    Ticket     │
                              ├───────────────┤
                              │ Id            │
                              │ BookingId(FK) │
                              │ SeatId (FK)   │
                              │ Price         │
                              └───────────────┘
```

### Core Tables

| Table | Purpose | Key Relationships |
|-------|---------|-------------------|
| **AspNetUsers** | User accounts & authentication | 1:N with Bookings |
| **AspNetRoles** | User roles (Admin, User) | N:M with Users |
| **Cinemas** | Cinema hall information | 1:N with Screenings |
| **Movies** | Movie catalog | 1:N with Screenings |
| **Screenings** | Movie showing schedules | N:1 with Cinema & Movie |
| **Seats** | Seating configurations | 1:N with Tickets |
| **Bookings** | User reservations | N:1 with User & Screening |
| **Tickets** | Individual ticket records | N:1 with Booking & Seat |

---

## 📖 Usage Guide

### For Regular Users

#### 1. Registration & Login
1. Navigate to the **Register** page
2. Fill in your details (email, password, full name, phone)
3. Submit to create your account
4. Login with your credentials

#### 2. Browse Screenings
1. Click on **Screenings** in the navigation menu
2. Browse available movies with dates and times
3. Check real-time seat availability
4. View pricing information

#### 3. Book Tickets
1. Select your desired screening
2. Choose seats from the interactive seat map
3. Review your selection and total price
4. Confirm booking
5. View confirmation in **My Bookings**

#### 4. Manage Bookings
1. Access **My Bookings** from your profile dropdown
2. View all active and past reservations
3. Cancel upcoming bookings if needed

---

### For Administrators

#### 1. Access Admin Panel
1. Login with administrative credentials
2. Access admin features from the navigation menu

#### 2. Create Screening
1. Navigate to **Add Screening**
2. Select cinema hall
3. Choose or enter movie title
4. Set date, time, and ticket price
5. Submit to create screening

#### 3. Manage System
1. View and edit all screenings
2. Delete screenings if necessary
3. Manage user accounts and roles via **Manage Users**
4. Monitor system activity

---

## 🔮 Future Enhancements

- [ ] **Payment Gateway Integration** - Online payment processing
- [ ] **Email Notifications** - Booking confirmations via email
- [ ] **Advanced Filtering** - Search by genre, date range, price
- [ ] **Mobile App** - Native iOS/Android applications
- [ ] **Analytics Dashboard** - Revenue and occupancy statistics
- [ ] **Multi-language Support** - Internationalization (i18n)
- [ ] **QR Code Tickets** - Digital ticket verification
- [ ] **Loyalty Program** - Reward frequent customers
- [ ] **Social Integration** - Share bookings on social media
- [ ] **API Documentation** - RESTful API for third-party integration

---

## 🐛 Troubleshooting

### Database Connection Issues

```bash
# Test MySQL connection
mysql -u cinema_user -p -h localhost CinemaDb

# Reset database if needed
dotnet ef database drop --force
dotnet ef database update
```

### Port Already in Use

Edit `Properties/launchSettings.json` to change the port:

```json
"applicationUrl": "https://localhost:7001;http://localhost:5001"
```

### Migration Errors

```bash
# Remove problematic migrations
dotnet ef migrations remove

# Create fresh migration
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Build Errors

```bash
# Clean solution
dotnet clean

# Restore packages
dotnet restore

# Rebuild
dotnet build
```

---

## 📚 References & Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Guide](https://docs.microsoft.com/en-us/ef/core/)
- [MySQL Documentation](https://dev.mysql.com/doc/)
- [Bootstrap 5 Documentation](https://getbootstrap.com/docs/5.3/)
- [Font Awesome Icons](https://fontawesome.com/icons)

---

## 📄 License

This project is developed for academic purposes as part of the Graphical User Interface course at Warsaw University of Technology.

**© 2025 Biniyam Awalachew Firde. All rights reserved.**

---

## 📧 Contact

For questions, suggestions, or issues:

**Email:** [yonatanawlachew1@gmail.com](mailto:yonatanawlachew1@gmail.com)

---

<div align="center">

**Made with ❤️ for Warsaw University of Technology**

⭐ Star this repository if you found it helpful!

</div>