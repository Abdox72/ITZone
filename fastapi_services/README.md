# ITZone FastAPI Services

This is a FastAPI backend service for the ITZone application.

## Features

- RESTful API with FastAPI
- SQLAlchemy ORM for database operations
- JWT Authentication
- User management
- Item management
- Pydantic models for request/response validation

## Project Structure

```
fastapi_services/
├── .env                  # Environment variables
├── README.md             # Project documentation
├── requirements.txt      # Python dependencies
├── main.py               # FastAPI application entry point
├── database.py           # Database configuration
├── models.py             # SQLAlchemy models
├── schemas.py            # Pydantic schemas
├── security.py           # Authentication and security
└── routers/              # API route handlers
    ├── __init__.py
    ├── auth.py           # Authentication routes
    ├── users.py          # User management routes
    └── items.py          # Item management routes
```

## Setup and Installation

1. Clone the repository

2. Create a virtual environment:
   ```bash
   python -m venv venv
   ```

3. Activate the virtual environment:
   - Windows:
     ```bash
     .\venv\Scripts\activate
     ```
   - Linux/Mac:
     ```bash
     source venv/bin/activate
     ```

4. Install dependencies:
   ```bash
   pip install -r requirements.txt
   ```

5. Run the application:
   ```bash
   python main.py
   ```
   or
   ```bash
   uvicorn main:app --reload
   ```

## API Documentation

Once the application is running, you can access the API documentation at:

- Swagger UI: http://localhost:8000/docs
- ReDoc: http://localhost:8000/redoc

## Environment Variables

The following environment variables can be configured in the `.env` file:

- `DATABASE_URL`: Database connection string (default: SQLite)
- `SECRET_KEY`: Secret key for JWT token generation
- `ALGORITHM`: Algorithm used for JWT token generation (default: HS256)
- `ACCESS_TOKEN_EXPIRE_MINUTES`: JWT token expiration time in minutes (default: 30)