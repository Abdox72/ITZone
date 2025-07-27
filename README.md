# ITZone

A comprehensive platform with FastAPI backend services, .NET backend, and Angular frontend.

## Project Structure

```
ITZone/
├── fastapi_services/     # FastAPI backend services
├── platform_backend/     # .NET backend
└── platform_frontend/    # Angular frontend
```

## Components

### FastAPI Services
- Located in `fastapi_services/`
- Python-based backend services
- Uses FastAPI framework
- Includes authentication, audio processing, and other services

### Platform Backend (.NET)
- Located in `platform_backend/`
- .NET 9.0 backend
- RESTful API controllers
- Entity Framework for data access

### Platform Frontend (Angular)
- Located in `platform_frontend/`
- Angular application
- Modern web interface
- Responsive design

## Getting Started

### Prerequisites
- Python 3.10+
- .NET 9.0 SDK
- Node.js and npm
- Git

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd ITZone
```

2. Set up FastAPI services:
```bash
cd fastapi_services
python -m venv venv
# On Windows:
venv\Scripts\activate
# On Unix/MacOS:
source venv/bin/activate
pip install -r requirements.txt
```

3. Set up .NET backend:
```bash
cd platform_backend
dotnet restore
dotnet build
```

4. Set up Angular frontend:
```bash
cd platform_frontend
npm install
```

## Running the Application

### FastAPI Services
```bash
cd fastapi_services
uvicorn main:app --reload
```

### .NET Backend
```bash
cd platform_backend
dotnet run
```

### Angular Frontend
```bash
cd platform_frontend
ng serve
```

## Development

- FastAPI services run on `http://localhost:8000`
- .NET backend runs on `http://localhost:5000`
- Angular frontend runs on `http://localhost:4200`

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Commit and push to your branch
5. Create a Pull Request

## License

[Add your license information here] 