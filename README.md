# Clean Motors
Clean Motors is a comprehensive web-based management system designed specifically for auto repair shops. The application streamlines the entire workflow from customer registration to service completion, providing role-based access for all stakeholders involved in the vehicle maintenance process.
## Features
### Authentication & Security
The system includes a secure user registration and login system with password hashing implementation for data protection. It features role-based access control with three distinct user types to ensure appropriate permissions throughout the application.
### User Roles
**Clients** can register and manage their personal accounts, schedule vehicle maintenance appointments, track the real-time status of their vehicle repairs, and view their complete service history and estimates.
**Mechanics** have access to their assigned vehicle repair tasks and can update work statuses such as In Progress, Waiting for Parts, or Completed. They can add service notes and recommendations, and mark jobs as ready for quality check.
**Administrators** enjoy full system access and control, including the ability to manage user accounts and roles, monitor all ongoing and completed jobs, generate reports and analytics, and configure system settings and service prices.
**Car Washers** can view vehicles scheduled for washing, update car wash status and estimated completion times, and notify clients when their vehicles are ready.
### Job Management
The system allows for creating and tracking repair orders with real-time status updates throughout the service process. It maintains a complete service history for each vehicle and includes an appointment scheduling system for efficient workflow management.
## Technology Stack
The application is built using C# and .NET on the backend, with HTML and CSS powering the frontend. Data is stored in Microsoft SQL Server (MS SQL), and the application follows the MVC architectural pattern. Security is implemented through custom password hashing to protect user data.
