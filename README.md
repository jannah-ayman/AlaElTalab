# 3la El Talab

AlaElTalab is a full-stack web application built with ASP.NET Core MVC. It connects customers with service providers, enabling users to register, book services, and leave feedback. The system includes role-based access control, and dynamic service status tracking.

---

## Features

* Role-based authentication (Customer & Service Provider)
* Booking system with real-time status updates (Pending → Confirmed → In Progress → Completed)
* Profile creation with secure image upload
* Rating system for completed bookings
* Separate dashboards for each user type
* Secure data validation and access restrictions
* Built with clean MVC architecture

---

## Technologies Used

* ASP.NET Core MVC
* ASP.NET Identity
* Entity Framework Core
* Razor Pages
* SQL Server
* Bootstrap / CSS
* IFormFile for file uploads

---

## Getting Started

1. **Clone the repository:**

   ```bash
   git clone https://github.com/jannah-ayman/AlaElTalab
   cd AlaElTalab
```

2. **Set up the database:**

   Open `appsettings.json` and update the connection string:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=AlaElTalabDb;Trusted_Connection=True;"
   }
   ```

   Then apply the migrations:

   ```bash
   dotnet ef database update
   ```

3. **Run the application:**

   ```bash
   dotnet run
   ```

   Open your browser and navigate to:

   ```
   https://localhost:5001
   ```

---

## User Roles

### ▸ Customers

* Register and log in
* Book services
* View and track bookings
* Rate service providers
* Manage profile information

### ▸ Service Providers

* Assigned manually to "ServiceProvider" role
* View incoming bookings
* Update booking statuses
* View ratings and profile data

---

## Profile Image Uploads

* Supported formats: `.jpg`, `.jpeg`, `.png`
* Max file size: 5MB
* Files are stored in `wwwroot/uploads/`

Validation is enforced both client-side and server-side.

---

## Team Members

* **Jannah Ayman**
* **Rawan Sotohy**
* **Sohila Ahmed**
* **Arwa Hassan**

```
