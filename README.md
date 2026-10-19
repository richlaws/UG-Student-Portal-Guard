# UG Student Portal Guard

UG Student Portal Guard is a C# desktop application project designed to help University of Ghana students prepare for and manage their course-registration process.

The project aims to improve the registration experience by providing a structured interface for student verification, academic setup, course selection, and interaction with the university's online portal.

## Project Objectives

- Provide a desktop interface for student verification.
- Organize academic setup information, including department, level, and semester.
- Display courses according to the selected academic information.
- Support course selection based on registration rules.
- Use browser automation to interact with the university portal where permitted.
- Help identify registration restrictions and situations that may prevent registration.

## Technologies Used

- **C#** — application programming language.
- **.NET 8** — application development platform.
- **Avalonia UI** — cross-platform desktop user interface.
- **Entity Framework Core** — database access and object-relational mapping.
- **MySQL** — relational database support.
- **Selenium WebDriver** — browser automation.
- **Chrome** — browser used for portal interaction.

## Main Project Components

### Student Verification

Provides an interface for entering student identification details and initiating verification.

### Academic Setup

Allows academic information such as department, level, and semester to be selected.

### Course Selection

Provides a structured interface for displaying and selecting courses based on the configured academic information and registration rules.

### Portal Interaction

Uses browser automation to interact with the university portal. Actual registration depends on portal availability, account permissions, university rules, and the portal's current state.

### Database

Uses Entity Framework Core to manage application data, including users, courses, registration rules, and registered courses.

## Configuration

The application expects the database connection string to be supplied through the environment variable:

`UG_DATABASE_CONNECTION`

Configure this variable with the appropriate MySQL connection string before running the application.

Do not commit database passwords, private tokens, or other credentials to the repository.

## Development Requirements

The development environment is expected to include:

- .NET SDK compatible with the project.
- A compatible MySQL database.
- Google Chrome for browser automation.
- The NuGet dependencies specified by the project.

## Running the Project

1. Clone the repository.
2. Open a terminal in the project directory.
3. Configure the `UG_DATABASE_CONNECTION` environment variable.
4. Restore the NuGet dependencies.
5. Build the project.
6. Run the application and test the available features.

The exact commands and additional setup requirements may depend on the development environment.

## Current Status

This project is under active development. Features, portal compatibility, and registration workflows are subject to testing and improvement.

Successful course registration depends on the university portal's availability, eligibility requirements, registration period, and applicable institutional restrictions.

## Disclaimer

UG Student Portal Guard is an independent student software project. It is not an official University of Ghana application and is not affiliated with or endorsed by the University of Ghana.

Users should follow the university's official registration procedures and policies.

## Author

Developed as a software project focused on improving the organization and usability of the university course-registration process.
