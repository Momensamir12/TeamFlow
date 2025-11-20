# TeamFlow - Collaborative Task Management Platform

A modern, real-time task management and team collaboration platform that helps teams organize work, assign tasks, and collaborate seamlessly.

## Features

### Core Features

####  Task Management
- **Create & Manage Tasks**: Create tasks for personal use or within projects
- **Task Status Tracking**: Track task progress with multiple status states (To Do, In Progress, Done, Blocked)
- **Priority Levels**: Set task priority (Low, Medium, High, Urgent) for better organization
- **Deadlines**: Set and manage task deadlines with visual indicators
- **Task Details**: Add comprehensive descriptions and track task updates in real-time

####  Workspaces & Projects
- **Workspace Management**: Create multiple workspaces for different teams or departments
- **Project Organization**: Organize tasks into projects within workspaces
- **Workspace Members**: Add team members to workspaces with role-based access control
- **Project Members**: Manage team members at the project level for granular control

####  Team Collaboration
- **Task Comments**: Comment on tasks to facilitate team discussions
- **Real-Time Notifications**: Receive instant notifications when tasks are assigned to you using SignalR
- **Task Assignment**: Assign tasks to team members and track their progress
- **Shared Task Views**: View and collaborate on workspace and project tasks in real-time


### Technical Features

####  Security & Authentication
- **JWT Authentication**: Secure token-based authentication with configurable expiration
- **Email Verification Policy**: Enforce email verification for enhanced security
- **Role-Based Authorization**: Implement role-based access control at workspace and project levels
- **Task-Level Authorization**: Fine-grained permission control for task ownership and assignment

#### ⚡ Real-Time Communication
- **SignalR Hub**: Real-time WebSocket connections for instant updates

## Tech Stack

### Backend
- **Framework**: ASP.NET Core 9.0
- **Database**: MySQL (local dev) / SQLite (Azure production)
- **ORM**: Entity Framework Core with Pomelo MySQL provider
- **Real-Time**: SignalR for WebSocket communication
- **Authentication**: JWT Bearer tokens with HS512 algorithm
- **Validation**: FluentValidation
- **Email**: Mock email service for development

### Frontend
- **Framework**: React 19
- **Build Tool**: Vite
- **Styling**: Tailwind CSS v4
- **HTTP Client**: Axios
- **Icons**: Lucide React
- **State Management**: React Hooks

### Infrastructure
- **Backend Hosting**: Azure App Service
- **Frontend Hosting**: Azure Static Web Apps
- **Database (Production)**: SQLite (Azure free tier)
- **Database (Development)**: MySQL 8.0 (Docker container)
- **Container Management**: Docker Compose

## Getting Started

📖 **For detailed setup instructions, see [LOCAL_SETUP.md](./LOCAL_SETUP.md)**

### Quick Start

#### Prerequisites
- .NET 9.0 SDK
- Node.js 16+
- Docker (for MySQL)

#### Option 1: Use Startup Script (Recommended)

**Linux/macOS:**
```bash
chmod +x start-local.sh
./start-local.sh
```

**Windows:**
```cmd
start-local.bat
```

This will:
- Start MySQL container (if not already running)
- Launch backend on `http://localhost:5000`
- Launch frontend on `http://localhost:5173`

#### Option 2: Manual Setup

**1. Start MySQL Container:**
```bash
cd Docker
docker-compose -f mysql-adm-docker-compose.yml up -d
```

**2. Create Database:**
```bash
docker exec -it mysql-db mysql -uroot -psecret -e "CREATE DATABASE IF NOT EXISTS TeamFlowDb_Dev;"
```

**3. Start Backend:**
```bash
cd Backend/TeamFlow.Api
dotnet restore
dotnet ef database update --context SecurityDbContext
dotnet ef database update --context AppDbContext
dotnet run
```

**4. Start Frontend:**
```bash
cd Frontend
npm install
npm run dev
```

The backend will start on `http://localhost:5000` and frontend on `http://localhost:5173`

### Configuration

The application automatically detects the environment:

**Development (main branch):**
- Uses MySQL on `localhost:3306`
- Connection: `Server=localhost;Port=3306;Database=TeamFlowDb_Dev;User=root;Password=secret;`
- Frontend points to `http://localhost:5000/api`

**Production (azure-deployment branch):**
- Uses SQLite with `Data Source=teamflow.db`
- Frontend points to Azure App Service URL
- Auto-applies migrations on startup

### Accessing the Application

- **Frontend**: http://localhost:5173
- **Backend API**: http://localhost:5000/api
- **Swagger UI**: http://localhost:5000/swagger
- **Database Admin (Adminer)**: http://localhost:8080

## Project Structure

```
TeamFlow/
├── Backend/
│   └── TeamFlow.Api/
│       ├── Application/         # Business logic & services
│       ├── Domain/              # Domain models & events
│       ├── Infrastructure/      # Data access & external services
│       ├── WebApi/              # Controllers & middleware
│       └── Migrations/          # EF Core migrations
├── Frontend/
│   └── src/
│       ├── components/          # React components
│       ├── pages/               # Page components
│       ├── api/                 # API client functions
│       ├── services/            # Business services (SignalR, etc.)
│       └── constants/           # Configuration & constants
├── Docker/                      # Docker compose files
├── docs/                        # Documentation
├── start-local.sh              # Linux/macOS startup script
├── start-local.bat             # Windows startup script
└── LOCAL_SETUP.md              # Detailed setup guide
```

## API Endpoints

### Authentication
- `POST /api/users/register` - Register a new user
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

## Deployment

### Azure Deployment (Production)

The `azure-deployment` branch is configured for Azure:

1. **Backend**: Deployed to Azure App Service
   - Uses SQLite database
   - Auto-applies migrations on startup
   - URL: `https://teamflow-edbhf6fmdxe2cae2.polandcentral-01.azurewebsites.net`

2. **Frontend**: Deployed to Azure Static Web Apps
   - Optimized production build
   - URL: `https://frontend2.z36.web.core.windows.net`

