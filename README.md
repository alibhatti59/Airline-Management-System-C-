# FlyWise: Integrated Airline Management System ✈️

**FlyWise** is a professional desktop application developed to automate and streamline the logistics of airline operations. Built with a **3-Tier Architecture**, the system ensures a clean separation between the user interface, business logic, and database management, providing a scalable solution for aviation logistics.

## 🚀 Key Features
* **Role-Based Access Control (RBAC):** Customized dashboards for **Airline Staff** (Administrative tools) and **Passengers** (Self-service booking).
* **Flight Management:** Real-time scheduling module to manage flight codes, origins, destinations, and total seating capacity.
* **Passenger CRM:** Secure registration and management of passenger demographics and identity records.
* **Automated Ticketing:** Dynamic ticket generation that links passenger data to flights with automatic database synchronization.
* **Cancellation System:** Integrated module to process ticket cancellations and instantly update seat availability.

## 🖥️ System Modules & Forms
The application is organized into specialized modules, each represented by dedicated Windows Forms for a clean and intuitive user experience:

* **Login & Register**: Secure entry points featuring credential validation against an SQL database and new user onboarding.
* **Home Dashboard**: A centralized navigation hub that dynamically adjusts visibility based on user roles (Admin vs. Passenger).
* **Manage Flights**: A dedicated logistics engine for creating, updating, and tracking flight schedules and capacities.
* **Manage Passengers**: A secure CRM module to handle passenger registrations and demographic records.
* **Book Tickets**: An automated transaction system that links passengers to flights and generates unique ticket identifiers.
* **View Tickets / Cancellation**: A comprehensive record viewer for active bookings, including integrated cancellation logic with real-time seat recovery.

## 🏗️ Technical Architecture
The project is structured into three distinct layers to maintain high standards of software engineering:
* **Presentation Layer:** Developed using C# Windows Forms (WinForms) with a focus on responsive, centered layouts for a professional user experience.
* **Business Logic Layer:** Handles core system processes including credential validation, seat availability checks, and booking logic.
* **Data Access Layer:** Utilizes ADO.NET to interact with a centralized **SQL Server LocalDB**, ensuring data persistence and relational integrity.

## 🛠️ Tech Stack
* **Language:** C# (.NET Framework)
* **Database:** Microsoft SQL Server (LocalDB)
* **IDE:** Microsoft Visual Studio
* **UI/UX:** WinForms with custom anchoring and centering

## ⚙️ Installation
1. Clone the repository: `git clone https://github.com/alibhatti59/Airline-Management-System-C#.git`.
2. Open the project in **Visual Studio** using the `.sln` file.
3. Attach the `AirlineDatabase.mdf` file to your local SQL instance.
4. Build and Run (F5).

## 🖼️ System Gallery

| **User Interface** | **Project Overview** |
| :---: | :---: |
| ![Login Screen](screenshots/login.png) | ![Registration Form](screenshots/registration.png) |
| *Professional Centered Login* | *Passenger Registration Portal* |

| **Manage Flights** | **Book Tickets** |
| :---: | :---: |
| ![Flight Management](screenshots/flights.png) |  ![Ticket Booking](screenshots/booking.png) |
| *Logistics: Add/Update/Delete Flights* | *Automated Seat & Amount Calculation* |

