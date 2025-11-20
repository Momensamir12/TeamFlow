# TeamFlow Local Development Setup

## Prerequisites

### 1. Docker (for MySQL)
Make sure Docker is installed and running on your machine.

### 2. MySQL Database
Start the MySQL container using docker-compose:

```bash
cd Docker
docker-compose -f mysql-adm-docker-compose.yml up -d
```

This will start:
- MySQL server on port **3306**
- Adminer (database management UI) on port **8080**

### 3. Database Setup
Access Adminer at `http://localhost:8080` and create the development database:

- **Server:** `db` (or `localhost` from host machine)
- **Username:** `root`
- **Password:** `secret`
- **Database:** Create `TeamFlowDb_Dev`

Or use the MySQL CLI:
```bash
docker exec -it mysql-db mysql -uroot -psecret -e "CREATE DATABASE IF NOT EXISTS TeamFlowDb_Dev;"
```

### 4. Install Dependencies

#### Backend
```bash
cd Backend/TeamFlow.Api
dotnet restore
```

#### Frontend
```bash
cd Frontend
npm install
```

## Running the Application

### Option 1: Use the Startup Script (Recommended)

#### Linux/macOS:
```bash
chmod +x start-local.sh
./start-local.sh
```

#### Windows:
```cmd
start-local.bat
```

The script will:
- Check and install frontend dependencies if needed
- Start the backend on `http://localhost:5000`
- Start the frontend on `http://localhost:5173`

Press `Ctrl+C` to stop both services.

### Option 2: Run Manually

#### Terminal 1 - Backend:
```bash
cd Backend/TeamFlow.Api
dotnet run
```

#### Terminal 2 - Frontend:
```bash
cd Frontend
npm run dev
```

## Configuration

### Main Branch (Local Development)
- **Database:** MySQL (Docker container)
- **Connection String:** `Server=localhost;Port=3306;Database=TeamFlowDb_Dev;User=root;Password=secret;`
- **Backend URL:** `http://localhost:5000`
- **Frontend URL:** `http://localhost:5173`

### Azure-Deployment Branch (Production)
- **Database:** SQLite (`teamflow.db`)
- **Backend URL:** Azure App Service
- **Frontend URL:** Azure Static Web App

## Accessing the Application

- **Frontend:** http://localhost:5173
- **Backend API:** http://localhost:5000/api
- **Swagger UI:** http://localhost:5000/swagger
- **Adminer (Database UI):** http://localhost:8080

## Troubleshooting

### Port Already in Use
If ports 5000 or 5173 are in use:

```bash
# Kill process on port 5000
lsof -ti:5000 | xargs kill -9

# Kill process on port 5173
lsof -ti:5173 | xargs kill -9
```

### MySQL Connection Issues
1. Verify MySQL container is running:
   ```bash
   docker ps | grep mysql
   ```

2. Check if database exists:
   ```bash
   docker exec -it mysql-db mysql -uroot -psecret -e "SHOW DATABASES;"
   ```

3. Verify connection from host:
   ```bash
   mysql -h localhost -P 3306 -u root -psecret
   ```

