# [The Film Journal]

A simple web-application which gives the user the ability to give raitings to films. 
These raitings are tracked to give the user a recap of which parts they enjoyed and which they didn't. 
These statistics can also be shared with other users.


## Tech-stack

### Frontend
- Framework: React 19.1.1
- Language: TypeScript 5.8.3
- Build Tool: Vite

### Backend (.NET 9.0)
- Framework: ASP.NET Core Web API
- Language: C#
- Database: PostgreSQL

### Infrastructure & DevOps
- **Reverse Proxy:** Nginx
- **Deployment:** Fly.io
- **CI/CD:** GitHub Actions
- **Load Testing:** k6
- **E2E Testing:** TestCafe
- **AI Automation:** Agentic Workflows (Gemini CLI)

## Architecture

Simple three-layer architecture splitting the application into Controllers, Service classes, and Entities. Nginx is used as a reverse proxy to handle API requests and bypass mixed content issues during deployment.

## Features

- **Authentication:** Secure login and user profile management.
- **Movie CRUD:** Full Create, Read, Update, and Delete functionality for movies.
- **Rating System:** Ability for users to rate films and track their preferences.
- **Statistics:** Recap and statistics of user ratings (sharable with others).
- **Friends System (WIP):** Early implementation of social features, including a friend sidebar and backend support for user connections.

## CI/CD & Automation

This project utilizes automated workflows to ensure code quality and maintain documentation.

### Agentic Workflows
Powered by [Gemini CLI](https://geminicli.com/), the following automated processes run daily:
- **Daily Documentation Updater:** Scans recent changes and automatically updates the project documentation.
- **Daily Repo Status:** Generates a daily summary of repository activity, including PRs, issues, and code changes, posted as a GitHub issue.
- **Code Simplifier:** Analyzes recently modified code and creates pull requests with simplifications to improve clarity and maintainability.

### Prerequisites for Automation
To run the Agentic Workflows, the following GitHub Secret is required:
- `GEMINI_API_KEY`: API key for Google Gemini, used by the Gemini CLI to power the agents.

### Testing
- **Unit & Integration Tests:** Automated tests for both Frontend and Backend.
- **End-to-End (E2E) Testing:** TestCafe is used to simulate user interactions and verify core application flows.
- **Load Testing:** k6 is used to verify system performance under load.

## Feature plan

> [!NOTE]
> For each week below write a short description of the features you plan to build for your project this week.
>
> 
> We're doing Feature planning 3 weeks at a time, to track project progression.
[...]

### Week 5
*Kick-off week - no features to be planned here*
DONE!

### Week 6
**Feature 1:** Movie CRUD implementation

**Feature 2:** Early database connection

### Week 7
*Winter vacation - nothing planned.*

### Week 8
**Feature 1:**  Login page, User profile Implementation.  

**Feature 2:** Frontend showcasing movie implementation.

### Week 9
**Feature 1:** Frontend Movie CRUD implementation.

**Feature 2:** Rating system implementation (both frontend and backend)
DONE!

### Week 10
**Feature 1:** Seperate the movies into Watchlist and Seen.

**Feature 2:** Adding friends early implementation
*IN PROGRESS - Basic UI and Backend support added.*

### Week 11
**Feature 1:** Complete friends implementation

**Feature 2:** Seeing friends' ratings.

### Week 12
**Feature 1:** Implementing genres

**Feature 2:** Sorting System for showcasing movies. 

### Week 13
**Feature 1:** Adding comments to ratings

**Feature 2:** Reusing movies, instead of creating new ones every time.

### Week 14
*Easter vacation - nothing planned.*

### Week 15
**Feature 1:** Tag system implementation

**Feature 2:** UI Overhaul

### Week 16
**Feature 1:** [...]

**Feature 2:** [...]

### Week 17
**Feature 1:** [...]

**Feature 2:** [...]
