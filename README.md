# Cinema Ticket Purchasing System

A full-stack web application for online cinema ticket booking built with ASP.NET Core and React.

## 🎯 Features

### User Features
- ✅ User registration and authentication
- ✅ User profile management (with concurrency control)
- ✅ Browse available movie screenings
- ✅ Interactive seat selection
- ✅ Reserve and cancel seat reservations
- ✅ Real-time seat availability display
- ✅ Conflict handling for concurrent reservations

### Admin Features
- ✅ Create and delete movie screenings
- ✅ User management (view and delete users)
- ✅ View all screenings
- ✅ Admin dashboard

### Technical Features
- ✅ ASP.NET Core 8.0 Web API backend
- ✅ React 18 frontend with React Router
- ✅ Entity Framework Core with SQLite
- ✅ Tailwind CSS for styling
- ✅ Concurrency control using RowVersion
- ✅ RESTful API design
- ✅ Password hashing with PBKDF2
- ✅ Proper error handling

## 📋 Requirements

- .NET 8.0 SDK
- Node.js 18+ and npm
- Visual Studio Code (recommended) or Visual Studio 2022

## 🚀 Quick Start

### Backend Setup

1. **Navigate to backend directory:**
   ```bash
   cd CinemaTicketSystem
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Run the backend:**
   ```bash
   dotnet run
   ```

   The API will start at `http://localhost:5155`
   
   Swagger UI: `http://localhost:5155/swagger`

### Frontend Setup

1. **Navigate to frontend directory:**
   ```bash
   cd cinema-frontend
   ```

2. **Install dependencies:**
   ```bash
   npm install
   ```

3. **Start the development server:**
   ```bash
   npm start
   ```

   The app will open at `http://localhost:3000`

## 👥 Test Credentials

### Regular User
- **Email:** john@email.com
- **Password:** User@123

### Administrator
- **Email:** admin@cinema.com
- **Password:** Admin@123

## 📁 Project Structure

### Backend (CinemaTicketSystem/)
```
├── Controllers/
│   ├── AuthController.cs         # Authentication endpoints
│   ├── CinemasController.cs      # Cinema management
│   ├── ReservationsController.cs # Seat reservations
│   ├── ScreeningsController.cs   # Screening management
│   └── UsersController.cs        # User management
├── Data/
│   └── CinemaDbContext.cs        # Database context
├── DTOs/                         # Data Transfer Objects
├── Models/                       # Entity models
├── Services/
│   ├── IPasswordHasher.cs        # Password hashing interface
│   └── PasswordHasher.cs         # Password hashing implementation
└── Program.cs                    # Application entry point
```

### Frontend (cinema-frontend/src/)
```
├── components/
│   ├── AdminPanel.jsx            # Admin dashboard
│   ├── Login.jsx                 # Login page
│   ├── Register.jsx              # Registration page
│   ├── ScreeningList.jsx         # Movie screenings list
│   ├── SeatSelection.jsx         # Seat booking interface
│   └── UserProfile.jsx           # User profile management
├── services/
│   └── api.js                    # API service layer
├── App.js                        # Main application component
├── index.js                      # React entry point
└── index.css                     # Global styles
```

## 🗄️ Database Schema

### Users
- Id, FirstName, LastName, Email (unique), PhoneNumber, PasswordHash, IsAdmin, RowVersion

### Cinemas
- Id, Name, Rows, SeatsPerRow

### Screenings
- Id, CinemaId (FK), MovieTitle, StartDateTime

### Reservations
- Id, ScreeningId (FK), UserId (FK), Row, Seat, ReservedAt, RowVersion
- Unique constraint on (ScreeningId, Row, Seat)

## 🔧 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login

### Users
- `GET /api/users` - Get all users (Admin only)
- `GET /api/users/{id}` - Get user by ID
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

### Cinemas
- `GET /api/cinemas` - Get all cinemas
- `GET /api/cinemas/{id}` - Get cinema by ID

### Screenings
- `GET /api/screenings` - Get all screenings
- `GET /api/screenings/{id}` - Get screening by ID
- `POST /api/screenings` - Create screening (Admin only)
- `DELETE /api/screenings/{id}` - Delete screening (Admin only)

### Reservations
- `GET /api/reservations/screening/{screeningId}/seatmap` - Get seat map
- `POST /api/reservations?userId={userId}` - Create reservation
- `DELETE /api/reservations/seat?screeningId={id}&row={row}&seat={seat}&userId={userId}` - Cancel reservation

## ⚙️ Concurrency Control

The system implements optimistic concurrency control using `RowVersion` (timestamp) on:

1. **User Updates**: Prevents conflicting user profile modifications
2. **Seat Reservations**: Prevents double-booking with unique constraint + error handling

### How it works:
- Each entity has a `RowVersion` field that changes on every update
- Update operations check the `RowVersion` before saving
- If `RowVersion` has changed, a `DbUpdateConcurrencyException` is thrown
- Frontend displays appropriate error messages

## 🎨 UI Features

- **Responsive Design**: Works on desktop and mobile
- **Modern UI**: Clean interface with Tailwind CSS
- **Real-time Updates**: Seat availability updates after each action
- **Color-coded Seats**:
  - 🟢 Green: Available
  - 🔵 Blue: Your reservations
  - 🔴 Red: Occupied by others
- **Error Handling**: Clear error messages for all operations
- **Loading States**: Visual feedback during async operations

## 🔒 Security Features

- Password hashing with PBKDF2 (10,000 iterations)
- CORS policy for API access
- Admin-only endpoints protection
- User data validation
- SQL injection prevention (EF Core parameterized queries)

## 📦 Production Deployment

### Backend
```bash
dotnet publish -c Release -o ./publish
```

### Frontend
```bash
npm run build
```

The build folder can be served by any static file server or integrated with the backend.

### Single Server Deployment
1. Build the React app
2. Copy the `build` folder to the backend's `wwwroot` folder
3. Configure backend to serve static files
4. Deploy as a single application

## 🐛 Troubleshooting

### Backend won't start
- Ensure .NET 8.0 SDK is installed: `dotnet --version`
- Check port 5155 isn't in use
- Delete `cinema.db` and restart to recreate database

### Frontend won't start
- Clear npm cache: `npm cache clean --force`
- Delete `node_modules` and `package-lock.json`
- Reinstall: `npm install`

### CORS errors
- Ensure backend is running on port 5155
- Check `proxy` setting in package.json
- Verify CORS policy in Program.cs

## 📝 Task Implementation Checklist

### Task 3: ✅ Completed
- [x] User registration implementation
- [x] User profile editing with concurrency control
- [x] Admin can view/edit all users
- [x] Parallelism handling (RowVersion)
- [x] Create/delete screenings
- [x] Entity Framework with SQLite

### Task 4: ✅ Completed
- [x] Seat reservation/cancellation
- [x] Concurrent reservation conflict handling
- [x] Display occupied seats (seat map)
- [x] Unique constraint on seat position
- [x] Real-time seat availability
- [x] Production-ready code structure

## 👨‍💻 Development

### Adding New Screening
1. Login as admin (admin@cinema.com / Admin@123)
2. Go to Admin Panel
3. Select "Screenings Management" tab
4. Fill in the form with cinema, movie title, and date/time
5. Click "Create Screening"

### Testing Concurrent Reservations
1. Open app in two different browsers
2. Login with different users
3. Navigate to the same screening
4. Try to book the same seat simultaneously
5. One user will succeed, the other will get an error message


## 🙋‍♂️ Support

For issues or questions:
1. Check the troubleshooting section
2. Review the API documentation at `/swagger`
3. Check browser console for errors
4. Verify backend logs in the terminal

---

**Note**: This is a demonstration project for a university assignment showcasing ASP.NET Core + React full-stack development with proper concurrency handling and modern web development practices.

## License

This project is for educational purposes.

## Authors

Biniyam Awalachew Firde