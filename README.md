# Library Management System

## 📖 Overview
The **Library Management System** is a full-stack, enterprise-ready web application designed to streamline and automate day-to-day library operations. Built with modern .NET technologies, it offers a robust RESTful Web API backend seamlessly integrated with a fast, responsive Blazor Server frontend UI. The system easily handles creating and managing books, member registrations, and tracking book-borrowing lifecycles. 

Additionally, the project is structured to incorporate an **Azure CI/CD pipeline** to allow for automated building, testing, and deployment processes.

---

## ✨ Features
### 📚 Book Management
* **Catalog Books:** Easily add and manage books with metadata such as Titles, Authors, and descriptive information.
* **Rich Book Data:** Store and display visually engaging details via Book Cover Image URLs.
* **User Ratings:** Track book ratings validated to a precise decimal point (e.g., 4.5 Stars).
* **Inventory Control:** Delete old or damaged books from the catalog when they are no longer in rotation.

### 👥 Member Management
* **Member Registration:** Add new members to the library.
* **Membership Lifecycle:** Keep track of membership dates, monitor membership expiry, and identify invalid/expired accounts.
* **Account Deletion:** Clean up the system by removing outdated or inactive members.

### 🔄 Borrowing & Lending System
* **Lending Records:** Borrow books out to active members and maintain a clean digital paper trail of the transaction.
* **Return Tracking:** Process borrow returns seamlessly and monitor which books are currently out of the library on loan.

### 🖥️ User Experience (UI)
* **Responsive Material Design:** A sleek, fully dark/light mode responsive user interface built heavily upon MudBlazor components.
* **Interactive Dialogs & Tables:** Features pop-up dialogs to add/manage data easily without losing page context, and rich data-tables to search and sort through books and members.

---

## 🛠️ Tech Stack & Technologies

### Backend (API)
The backend acts as a robust data-serving layer constructed with the latest .NET standards.
* **Framework:** [.NET 8.0](https://dotnet.microsoft.com/) ASP.NET Core Web API
* **Language:** C#
* **ORM (Object-Relational Mapper):** Entity Framework Core (EF Core 9)
* **Database:** Microsoft SQL Server
* **API Documentation:** Swagger / OpenAPI (Swashbuckle)
* **Architecture:** Patterned using a Service-Oriented approach with Controllers, Services, and DbContext for separation of concerns.

### Frontend (User Interface)
The frontend consumes the API using standard HTTP protocols and is composed of dynamic, server-side rendered components.
* **Framework:** .NET 8.0 [Blazor Server](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor) (Server-Side UI Rendering)
* **Language:** C#, Razor, HTML, CSS
* **UI Component Library:** [MudBlazor](https://mudblazor.com/) (Material Design Component framework for seamless, out-of-the-box beautiful UI)
* **API Communication:** `HttpClient` configured via Dependency Injection (DI)

### DevOps & Hosting
* **Source Control:** Git / GitHub
* **CI/CD:** Azure DevOps CI/CD Pipelines (Continuous Integration & Continuous Deployment) *(Configured via the repo architecture)*
