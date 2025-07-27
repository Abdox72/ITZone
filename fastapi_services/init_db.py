import os
import sys
from sqlalchemy.orm import Session

# Add the current directory to the path so we can import our modules
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from database import SessionLocal, engine
import models
from security import get_password_hash

# Create tables
models.Base.metadata.create_all(bind=engine)


def init_db():
    db = SessionLocal()
    try:
        # Check if we already have users
        user_count = db.query(models.User).count()
        if user_count == 0:
            print("Creating sample users...")
            # Create sample users
            admin_user = models.User(
                email="admin@example.com",
                username="admin",
                hashed_password=get_password_hash("adminpassword"),
                is_active=True
            )
            db.add(admin_user)
            
            test_user = models.User(
                email="user@example.com",
                username="testuser",
                hashed_password=get_password_hash("userpassword"),
                is_active=True
            )
            db.add(test_user)
            
            db.commit()
            print("Sample users created successfully!")
            
            # Create sample items
            print("Creating sample items...")
            item1 = models.Item(
                title="Sample Item 1",
                description="This is a description for sample item 1",
                owner_id=1  # admin user
            )
            db.add(item1)
            
            item2 = models.Item(
                title="Sample Item 2",
                description="This is a description for sample item 2",
                owner_id=2  # test user
            )
            db.add(item2)
            
            db.commit()
            print("Sample items created successfully!")
        else:
            print("Database already contains data. Skipping initialization.")
    except Exception as e:
        print(f"Error initializing database: {e}")
    finally:
        db.close()


if __name__ == "__main__":
    print("Initializing the database...")
    init_db()
    print("Database initialization completed.")