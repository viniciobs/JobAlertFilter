# Job Alert Filter

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)

A .NET console application that automatically filters and analyzes job alert emails from your Gmail inbox using a local Large Language Model (LLM) via [Ollama](https://ollama.com/).

It connects to your Gmail account via IMAP, reads unread emails from a specific sender, and uses an AI model to extract key information (like job title, company, salary, and a summary of requirements) to help you quickly identify the best opportunities.

## Features

*   **Gmail Integration**: Securely connects to Gmail using an App Password.
*   **AI-Powered Analysis**: Uses a locally running Ollama model to parse and summarize email content.
*   **Smart Filtering**: Focuses only on unread emails from your specified job alert sender.
*   **Local & Secure**: Your credentials are stored locally and are never committed to the repository.

## Prerequisites

*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or later.
*   [Ollama](https://ollama.com/download) installed and running on your machine.
*   A Google account with [2-Step Verification](https://support.google.com/accounts/answer/185839) enabled.
*   A Gmail [App Password](https://support.google.com/accounts/answer/185833) generated for this application.

## Configuration

1.  **Clone the repository**:
    ```bash
    git clone https://github.com/viniciobs/JobAlertFilter.git
    cd JobAlertFilter
    ```
2. **Configure Gmail and App Settings**:

    - Open the `Configuration/appsettings.json` file.
    - Replace the placeholder values with your information:
    ```json
    {
        "AppConfiguration": {
            "Email": "your-email@gmail.com",
            "AppPassword": "your-16-char-app-password",
            "SenderEmail": "alerts@jobboard.com", // The specific sender to filter
            "ImapOperationTimeoutSeconds": 30, // Timeout (seconds) for retrieving emails only
            "ProcessSingleEmailTimeoutMinutes": 5, // Timeout (minutes) for processing each email
            "OutputDirectory": "C:/Users/User/Projects/JobAlertFilter/Results",
            "AnalysisTarget": "email | linkedin" 
        },
        "Profile": {
            "WorkModes": ["Remote"],
            "Locations": ["Brazil", "LATAM", "Global Remote"],
            "PrimaryStack": [".NET", "C#", "Azure", "SQL Server"],
            "SecondaryStack": ["React", "TypeScript", "Docker", "Kubernetes"],
            "MinYearsExperience": 5,
            "Languages": ["Portuguese", "English"],
            "Roles": ["Senior Backend Engineer", "Tech Lead", "Staff Engineer"],
            "AvoidKeywords": ["PHP", "Ruby", "WordPress", "Joomla", "5+ years in Java only" ],
            "MustHaveKeywords": ["C#", ".NET"]
        },
        "OllamaSettings": {
            "Endpoint": "http://localhost:11434",
            "ModelName": "llama3.2" // Or your preferred model
        }
    }
    ```
    - For local development, create a `Configuration/appsettings.local.json` file with the same structure. This file is ignored by Git, keeping your credentials secure. The application will load settings from `appsettings.local.json` if it exists.
    - Configure the AI Model: Ensure the model you specified in `ModelName` is pulled in Ollama:
    ```bash
    ollama pull llama3.2
    ```

## Usage
Run the application from the project root directory:
```bash
dotnet run
```

The application will:

1. Connect to your Gmail account using the provided credentials.
2. Search for unread emails from the configured `SenderEmail`.
3. For each new email, send its content to the Ollama API for analysis.
4. Generate a `.md` file containing the analysis results (e.g., job title, url and reasoning) and save it to the directory specified by `OutputDirectory` in `appsettings.json`.

## How It Works (Brief Technical Overview)
- `Services/GmailService.cs`: Handles IMAP connection, authentication, and email fetching using the `MailKit` library.
- `Services/AnalysisService.cs`: Constructs a prompt using the email content and templates from the `Prompts/` folder, sends it to the Ollama API, and parses the JSON response into a structured model (`Models/AnalysisResult.cs`).
`Program.cs`: Orchestrates the workflow and handles dependency injection.

## Future Improvements
- Get further details from the Job Posting page. It currently gets the details from the email.
- Work on avoiding hitting AI APIs too frequently.