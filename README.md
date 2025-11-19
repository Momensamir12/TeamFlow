# TeamFlow - Collaborative Task Management Platform

A modern, real-time task management and team collaboration platform that helps teams organize work, assign tasks, and collaborate seamlessly.

## Features

### Core Features

#### 📋 Task Management
- **Create & Manage Tasks**: Create tasks for personal use or within projects
- **Task Status Tracking**: Track task progress with multiple status states (To Do, In Progress, Done, Blocked)
- **Priority Levels**: Set task priority (Low, Medium, High, Urgent) for better organization
- **Deadlines**: Set and manage task deadlines with visual indicators
- **Task Details**: Add comprehensive descriptions and track task updates in real-time

#### 👥 Workspaces & Projects
- **Workspace Management**: Create multiple workspaces for different teams or departments
- **Project Organization**: Organize tasks into projects within workspaces
- **Workspace Members**: Add team members to workspaces with role-based access control
- **Project Members**: Manage team members at the project level for granular control

#### 💬 Team Collaboration
- **Task Comments**: Comment on tasks to facilitate team discussions
- **Real-Time Notifications**: Receive instant notifications when tasks are assigned to you using SignalR
- **Task Assignment**: Assign tasks to team members and track their progress
- **Shared Task Views**: View and collaborate on workspace and project tasks in real-time

#### 🔐 User Management
- **User Accounts**: Create and manage user accounts with profile customization
- **Email Verification**: Secure email verification for account setup
- **Workspace Invitations**: Invite team members to workspaces via tokens
- **Password Management**: Reset passwords and change account credentials securely

### Technical Features

#### 🔐 Security & Authentication
- **JWT Authentication**: Secure token-based authentication with configurable expiration
- **Email Verification Policy**: Enforce email verification for enhanced security
- **Role-Based Authorization**: Implement role-based access control at workspace and project levels
- **Task-Level Authorization**: Fine-grained permission control for task ownership and assignment
- **Secure Password Handling**: Bcrypt-based password hashing and validation

#### ⚡ Real-Time Communication
- **SignalR Hub**: Real-time WebSocket connections for instant updates
- **Task Notifications**: Receive live notifications when assigned tasks without page refresh
- **Connection Management**: Automatic reconnection and connection state management
- **User-Specific Broadcasting**: Send notifications to specific users securely

#### 🗄️ Data & Performance
- **Entity Framework Core**: ORM for robust database interactions with MySQL
- **Query Optimization**: Eager loading and aggregation to eliminate N+1 query problems
- **Data Persistence**: Reliable data storage with migration support
- **Transaction Management**: Safe concurrent operations with proper transaction handling

#### 🎯 API Design
- **RESTful Architecture**: Clean, RESTful API endpoints for all operations
- **Global Exception Handling**: Unified error handling with user-friendly messages
- **API Response Standardization**: Consistent response format across all endpoints
- **Validation**: FluentValidation for robust input validation

#### 🎨 Frontend Architecture
- **React.js**: Modern UI framework with hooks and functional components
- **State Management**: Optimistic updates to reduce perceived latency
- **Real-Time Sync**: SignalR client integration for live updates
- **Responsive Design**: Mobile-friendly interface with Tailwind CSS
- **Component Structure**: Modular, reusable components for maintainability

## Tech Stack

### Backend
- **Framework**: ASP.NET Core 9.0
- **Database**: MySQL with Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **Real-Time**: SignalR for WebSocket communication
- **Validation**: FluentValidation
- **Logging**: Microsoft.Extensions.Logging

### Frontend
- **Framework**: React.js
- **Build Tool**: Vite
- **Styling**: Tailwind CSS
- **HTTP Client**: Axios
- **Real-Time**: @microsoft/signalr
- **Icons**: Lucide React
- **Date Picker**: React DatePicker

### Infrastructure
- **Version Control**: Git & GitHub
- **Email Service**: Gmail SMTP
- **Load Testing**: NBomber

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- Node.js 16+
- MySQL 8.0+

### Backend Setup

```bash
cd Backend/TeamFlow.Api
dotnet restore
dotnet appsettings.json  # Configure your settings
dotnet run
```

The API will start on `http://localhost:5000`

### Frontend Setup

```bash
cd Frontend
npm install
npm run dev
```

The frontend will start on `http://localhost:5173`

### Database Setup

Ensure MySQL is running and configure the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TeamFlowDb;User=root;Password=your_password;"
  }
}
```

Run migrations:
```bash
dotnet ef database update
```

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login with credentials
- `POST /api/auth/verify-email` - Verify email address
- `POST /api/auth/resend-verification` - Resend verification email
- `POST /api/auth/forgot-password` - Request password reset
- `POST /api/auth/reset-password` - Reset password with token

### Tasks
- `GET /api/tasks/assigned` - Get user's assigned tasks
- `POST /api/tasks` - Create a new task
- `GET /api/tasks/{id}` - Get task details
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task
- `PUT /api/tasks/{id}/status` - Update task status
- `PUT /api/tasks/{id}/priority` - Update task priority

### Workspaces
- `GET /api/workspaces` - Get user's workspaces
- `POST /api/workspaces` - Create a new workspace
- `GET /api/workspaces/{id}` - Get workspace details
- `PUT /api/workspaces/{id}` - Update workspace
- `POST /api/workspaces/{id}/members` - Add workspace member
- `GET /api/workspaces/{id}/code` - Get workspace invitation code
- `POST /api/workspaces/join` - Join workspace by code

### Projects
- `GET /api/projects/workspace/{workspaceId}` - Get workspace projects
- `POST /api/projects` - Create a new project
- `GET /api/projects/{id}` - Get project details
- `PUT /api/projects/{id}` - Update project
- `DELETE /api/projects/{id}` - Delete project

### Invitations
- `POST /api/invitations/send` - Send workspace invitation
- `POST /api/invitations/accept` - Accept workspace invitation

### Comments
- `GET /api/comments/task/{taskId}` - Get task comments
- `POST /api/comments` - Add comment to task
- `PUT /api/comments/{id}` - Update comment
- `DELETE /api/comments/{id}` - Delete comment

### WebSocket
- `ws://localhost:5000/notificationsHub` - SignalR notification hub

## API Documentation

### Swagger/OpenAPI
The API includes comprehensive Swagger documentation with detailed descriptions for every endpoint.

**Access Swagger UI**: `http://localhost:5000/swagger` (Development environment)

**Features:**
- Interactive API explorer with complete endpoint documentation
- Detailed descriptions for all operations and parameters
- Request/response schema examples for all DTOs
- JWT Bearer token authentication testing
- Real-time API testing capabilities
- Auto-generated from XML code comments

**Alternative Documentation**: The project also includes Scalar API documentation at `http://localhost:5000/scalar` for a modern, alternative documentation experience.

## Performance





---

**TeamFlow** - Making team collaboration effortless
