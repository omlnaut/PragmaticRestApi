#!/usr/bin/env python3
import requests
import json
import time
import sys
from datetime import datetime

# Configuration
BASE_URL = "http://localhost:8080"  # Using port 8080 from docker-compose.yml
HEADERS = {"Content-Type": "application/json"}
MAX_RETRIES = 5
RETRY_DELAY = 2  # seconds


def print_color(text, color_code):
    """Print colored text to console"""
    print(f"\033[{color_code}m{text}\033[0m")


def print_success(text):
    print_color(f"✓ {text}", "92")


def print_error(text):
    print_color(f"✗ {text}", "91")


def print_info(text):
    print_color(f"ℹ {text}", "94")


def make_request(method, endpoint, data=None, retries=MAX_RETRIES):
    """Make HTTP request with retry logic"""
    url = f"{BASE_URL}/{endpoint}"
    
    print_info(f"Making {method.upper()} request to {url}")
    if data:
        print_info(f"Request data: {json.dumps(data, indent=2)}")
        
    if method.lower() == "get":
        response = requests.get(url, headers=HEADERS)
    elif method.lower() == "post":
        response = requests.post(url, headers=HEADERS, json=data)
    elif method.lower() == "put":
        response = requests.put(url, headers=HEADERS, json=data)
    else:
        print_error(f"Unsupported method: {method}")
        return None

    if response.status_code >= 400:
        print_error(f"Request failed with status {response.status_code}")
        print(response.content)
        print(response.content())
        sys.exit()
    if response.status_code == 204:
        return None
    return response.json()


def create_tags():
    """Create sample tags and return their IDs"""
    print_info("Creating tags...")
    
    tags_data = [
        {"name": "Health", "description": "Habits related to physical and mental wellbeing"},
        {"name": "Productivity", "description": "Habits that improve efficiency and output"},
        {"name": "Learning", "description": "Habits focused on acquiring new knowledge and skills"},
        {"name": "Personal", "description": "Personal development and self-improvement habits"},
        {"name": "Work", "description": "Professional development habits"},
        {"name": "Finance", "description": "Money management and financial habits"},
        {"name": "Social", "description": "Habits related to relationships and social skills"}
    ]
    
    tag_ids = {}
    
    for tag_data in tags_data:
        response = make_request("post", "tags", tag_data)
        if response and "id" in response:
            tag_ids[tag_data["name"]] = response["id"]
            print_success(f"Created tag: {tag_data['name']} (ID: {response['id']})")
        else:
            print_error(f"Failed to create tag: {tag_data['name']}")
    
    return tag_ids


def create_habits():
    """Create sample habits and return their IDs"""
    print_info("Creating habits...")
    
    # Updated with valid target units from the error message
    habits_data = [
        {
            "name": "Daily Exercise",
            "type": "Measurable",
            "description": "Exercise for at least 30 minutes every day",
            "frequency": {
                "timesPerPeriod": 1,
                "type": "Daily"
            },
            "target": {
                "value": 30,
                "unit": "MINUTES"  # Changed to uppercase MINUTES
            }
        },
        {
            "name": "Read Books",
            "type": "Measurable",
            "description": "Read at least 20 pages each day",
            "frequency": {
                "timesPerPeriod": 1,
                "type": "Daily"
            },
            "target": {
                "value": 20,
                "unit": "PAGES"  # Changed to uppercase PAGES
            },
            "milestone": {
                "target": 100,
                "current": 0
            }
        },
        {
            "name": "Meditation",
            "type": "Measurable",
            "description": "Meditate for 10 minutes in the morning",
            "frequency": {
                "timesPerPeriod": 1,
                "type": "Daily"
            },
            "target": {
                "value": 10,
                "unit": "MINUTES"  # Changed to uppercase MINUTES
            }
        },
        {
            "name": "Weekly Planning",
            "type": "Binary",
            "description": "Plan tasks and goals for the week ahead",
            "frequency": {
                "timesPerPeriod": 1,
                "type": "Weekly"
            },
            "target": {
                "value": 1,
                "unit": "SESSIONS"  # Changed to uppercase SESSIONS
            }
        },
        {
            "name": "Coding Practice",
            "type": "Measurable",
            "description": "Work on coding challenges or personal projects",
            "frequency": {
                "timesPerPeriod": 1,
                "type": "Daily"
            },
            "target": {
                "value": 45,
                "unit": "MINUTES"  # Changed to uppercase MINUTES
            }
        },
        {
            "name": "Budget Review",
            "type": "Binary",
            "description": "Review monthly budget and expenses",
            "frequency": {
                "timesPerPeriod": 1,
                "type": "Monthly"
            },
            "target": {
                "value": 1,
                "unit": "TASKS"  # Changed to uppercase TASKS
            }
        },
        {
            "name": "Call Family",
            "type": "Binary",
            "description": "Call parents or siblings to stay connected",
            "frequency": {
                "timesPerPeriod": 1,
                "type": "Weekly"
            },
            "target": {
                "value": 1,
                "unit": "TASKS"  # Changed to uppercase TASKS
            }
        }
    ]
    
    habit_ids = {}
    
    for habit_data in habits_data:
        response = make_request("post", "habits", habit_data)
        if response and "id" in response:
            habit_ids[habit_data["name"]] = response["id"]
            print_success(f"Created habit: {habit_data['name']} (ID: {response['id']})")
        else:
            print_error(f"Failed to create habit: {habit_data['name']}")
    
    return habit_ids


def create_habit_tag_relationships(habit_ids, tag_ids):
    """Create relationships between habits and tags"""
    print_info("Creating habit-tag relationships...")
    
    # Define which tags should be associated with which habits
    relationships = [
        ("Daily Exercise", ["Health", "Personal"]),
        ("Read Books", ["Learning", "Personal"]),
        ("Meditation", ["Health", "Personal"]),
        ("Weekly Planning", ["Productivity", "Work"]),
        ("Coding Practice", ["Learning", "Work", "Productivity"]),
        ("Budget Review", ["Finance", "Personal"]),
        ("Call Family", ["Social", "Personal"])
    ]
    
    for habit_name, tag_names in relationships:
        if habit_name not in habit_ids:
            print_error(f"Habit not found: {habit_name}")
            continue
        
        habit_id = habit_ids[habit_name]
        
        tag_ids = {
            "TagIds": [tag_ids.get(tag_name) for tag_name in tag_names if tag_name in tag_ids]
        }
            
        response = make_request("put", f"habits/{habit_id}/tags", tag_ids)
        print_success(f"Connected {habit_name} with tags {tag_names}")


def verify_data():
    """Verify that data was created correctly"""
    print_info("Verifying created data...")
    
    # Get all habits with tags
    habits_response = make_request("get", "habits?includeTags=true")
    if habits_response and "data" in habits_response:
        print_success(f"Found {len(habits_response['data'])} habits with their tags")
        
        # Print habit details for verification
        for habit in habits_response["data"]:
            print_info(f"Habit: {habit.get('name')} (ID: {habit.get('id')})")
            if "tags" in habit and habit["tags"]:
                for tag in habit["tags"]:
                    print_info(f"  - Tag: {tag}")
    else:
        print_error("Failed to retrieve habits")
    
    # Get all tags
    tags_response = make_request("get", "tags")
    if tags_response and "data" in tags_response:
        print_success(f"Found {len(tags_response['data'])} tags")
    else:
        print_error("Failed to retrieve tags")


def main():
    """Main function to seed the database"""
    print_info("Starting database seeding process...")
    
    # Check if API is accessible
    print_info("Checking API availability...")
    response = make_request("get", "tags")
    
    if response is None:
        print_error(f"API is not accessible at {BASE_URL}. Exiting.")
        sys.exit(1)
    
    print_success("API is accessible")
    
    # Create tags
    tag_ids = create_tags()
    
    # Create habits
    habit_ids = create_habits()
    
    # Create relationships between habits and tags
    create_habit_tag_relationships(habit_ids, tag_ids)
    
    # Verify data was created correctly
    verify_data()
    
    print_info("Database seeding completed successfully!")


if __name__ == "__main__":
    main()
