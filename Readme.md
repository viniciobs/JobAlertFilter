# Job Alert Filter

A .NET console application that automatically filters and analyzes job alert emails using an AI model, matching each opportunity against your professional profile.

It connects to your Gmail account via IMAP, reads unread job alert emails, and evaluates each opportunity in one of two modes:

- **Email mode** — analyzes the job summaries embedded directly in the alert email (fast, one AI call per email).
- **LinkedIn mode** — extracts each job's URL from the alert, fetches the full LinkedIn job posting, and analyzes the complete job description.

Both modes use a Large Language Model to produce a structured evaluation per job: title, confidence score, matched criteria, concerns, and an **Apply / Maybe / Skip** recommendation, saved as a Markdown report.

## Features

- **Gmail Integration** — connects via IMAP using a Gmail App Password; processes only unread emails from your configured alert sender.
- **Two analysis modes** — `email` for quick summary-based matching, `linkedin` for full job-posting analysis.
- **Pluggable AI providers** — run a local model with **Ollama** (private, no data leaves your machine) or any **OpenAI-compatible API** (e.g. Groq) for faster inference. AI output is constrained by a fixed JSON schema.
- **Profile-driven matching** — your stack, roles, locations, work modes, must-have/avoid keywords, experience and languages are injected into the prompt. Tuning the filter means editing `appsettings.json`, not code.
- **Polite by design** — configurable delay between job page fetches in LinkedIn mode keeps request rates low for both LinkedIn and the AI API.
- **Fault isolation** — a failed fetch, timeout, or malformed AI response for one job never aborts the batch; failures are logged and skipped.

## Prerequisites

- [.NET 11 SDK](https://dotnet.microsoft.com/download) or later
- An AI backend — either:
  - **Ollama** installed and running locally (default), or
  - An API key for an OpenAI-compatible provider (e.g. Groq)
*   A Google account with [2-Step Verification](https://support.google.com/accounts/answer/185839) enabled.
*   A Gmail [App Password](https://support.google.com/accounts/answer/185833) generated for this application.

## Configuration

1.  **Clone the repository**:
    ```bash
    git clone https://github.com/viniciobs/JobAlertFilter.git
    cd JobAlertFilter
    ```
2. **Configure `Configuration/appsettings`**:

    ```json
    {
        "AppConfiguration": {
            "Email": "your-email@gmail.com",
            "AppPassword": "your-16-char-app-password",
            "SenderEmail": "alerts@jobboard.com",
            "ImapOperationTimeoutSeconds": 30,
            "ProcessSingleEmailTimeoutMinutes": 5,
            "OutputDirectory": "C:/Users/User/Projects/JobAlertFilter/Results",
            "AnalysisTarget": "email",
            "JobPageRequestDelaySeconds": 15
        },
        "Profile": {
            "WorkModes": ["Remote", "Hybrid"],
            "Locations": ["Brazil", "LATAM", "North America"],
            "PrimaryStack": [".NET", "C#", "Azure", "SQL Server"],
            "SecondaryStack": ["React", "TypeScript", "Docker", "Kubernetes"],
            "MinYearsExperience": 5,
            "Languages": ["Portuguese", "English"],
            "Roles": ["Senior Backend Engineer", "Tech Lead", "Staff Engineer"],
            "AvoidKeywords": ["PHP", "Ruby", "WordPress", "Joomla", "5+ years in Java only" ],
            "MustHaveKeywords": ["C#", ".NET"]
        },
        "AIProvider": {
            "Provider": "Ollama",
            "BaseUrl": "http://localhost:11434",
            "Model": "llama3.2:3b",
            "APIKey": "",
            "TimeoutSeconds": 120
        }
    }
    ```
    - For local development, create a `Configuration/appsettings.local.json` file with the same structure. This file is ignored by Git, keeping your credentials secure. The application will load settings from `appsettings.local.json` if it exists.
    - Configure the AI Model: 
        - Set `Provider` to either `ollama` or `groq`
        - Point `BaseUrl` to the provider endpoint (e.g. `https://api.groq.com/openai/v1`)
        - Set the `Model` name
        - Fill `APIKey`

### Setting reference
|Setting|Description|
|-------|-----------|
|SearchFromEmail| Only unread emails from this sender are processed.|
|AnalysisTarget|`email` (analyze summaries in the email) or `linkedin` (fetch and analyze each job posting page).|
|JobPageRequestDelaySeconds|Delay between job page fetches in LinkedIn mode. Higher values = gentler on LinkedIn and the AI API.|
|ProcessSingleEmailTimeoutMinutes|Per-email budget. In LinkedIn mode this covers all jobs in one email (fetch + delay + analysis) — raise it if your alerts contain many jobs.|
|ImapOperationTimeoutSeconds|Timeout for IMAP operations (connect, search, fetch).|
|Profile.*|Your candidate profile used for matching. All fields are required.|
|AIProvider.Provider|`ollama` or `groq`. Other OpenAI-compatible endpoints work with the groq provider as long as they implement /chat/completions.|

## Usage
Run the application from the project root directory:
```bash
dotnet run
```

The application will:

1. Connect to your Gmail account using the provided credentials.
2. Search for unread emails from the configured `SearchFromEmail `.
3. Analyze each email according to `AnalysisTarget`:
    - **email**: flatten the job cards in the email and send them to the AI in a single batch prompt.
    - **linkedin**: extract the job posting URLs from the email, fetch each page, and analyze each job individually.
4. Generate a timestamped `.md` report in the directory specified by `OutputDirectory`, with title, link, confidence score, recommendation, reasoning, matched criteria and concerns for each job.

## How It Works (Brief Technical Overview)
```text
Gmail (IMAP) ──▶ EmailScanner ──▶ IJobAnalyzer ──▶ IAiService ──▶ AnalysisResult[] ──▶ ResultWriter ──▶ report.md
                    (MailKit)       │ email           (Ollama /
                                    │ linkedin        OpenAI-compatible)
                                    ▼
                              FileTemplates/ (prompts)
```

- `Services/EmailScanner.cs` — IMAP connection, authentication and unread-email fetching via `MailKit`, with a per-email processing timeout and per-message fault isolation.
- `Services/EmailAnalyzer.cs` — **email mode**: converts the email's job cards to plain text (HtmlExtensions.ToPlainText), merges it with your profile into `FileTemplates/prompt-template.md`, and sends one batch request to the AI.
- `Services/LinkedInAnalyzer.cs` — **linkedin mode**: extracts deduplicated job posting URLs from the email (HtmlExtensions.ToJobUrls), then for each URL fetches the page, builds a single-job prompt from `FileTemplates/linkedin-prompt-template.md`, and calls the AI — with a configurable delay between jobs and per-job error handling.
- `Services/Providers/LinkedInJobScraper.cs` — fetches guest-accessible LinkedIn job pages using a browser-like user agent, and extracts the posting content from the embedded JSON-LD (schema.org/JobPosting) with CSS-selector fallbacks. Content is truncated to keep prompts small and token usage predictable.
- `Services/Providers/OllamaService.cs` / `Services/Providers/OpenAIService.cs` — send the prompt to the configured provider. Responses are validated against a fixed JSON schema and deserialized into `Models/AnalysisResult.cs`.
- `Services/FileContentLoader.cs` + `FileTemplates/` — prompt and report templates with `{{placeholder}}` substitution; fails fast on unfilled placeholders so misconfiguration surfaces immediately.
- `Services/ResultWriter.cs` — renders all results into a single timestamped Markdown report using `analysis-result-template.md`.
- `Program.cs` — dependency injection, startup validation of all options, and runtime selection of the analyzer (`AnalysisTarget`) and AI provider (`Provider`).

## Notes & Caveats

- **LinkedIn mode and scraping**: LinkedIn job pages are publicly accessible, but automated access goes against LinkedIn's Terms of Service and the site actively blocks bot-like traffic (failed fetches are logged and skipped, never fatal). JobPageRequestDelaySeconds exists to keep request rates low. Use at your own discretion.
- **AI reliability**: output is schema-constrained, but small local models can still produce imperfect judgments — treat ConfidenceScore and Reasoning as guidance, not ground truth.
- **Token usage**: job descriptions are capped at _~8,000_ characters before being sent to the model.


## Future Improvements
- More AI Providers
- More Email Providers
- More Job Providers