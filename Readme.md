# Overview
This app connects to Gmail via `IMAP` using `MailKit` and searches for unread emails from the sender configured in `Configuration/appsettings.json`.

## Configuration
To use it with Gmail you must follow these steps:    
1. Turn on **2 step verification** for your Google account by following the instructions [here](https://support.google.com/accounts/answer/185839);
2. Create an app password [here](https://myaccount.google.com/apppasswords);
3. Edit `Configuration/appsettings.json` replacing `Email` with the email address configured for the app and `AppPassword` with the generated app password.

### Local configuration
Alternatively, you can create a `Configuration/appsettings.local.json` file and add your credentials there.
The application automatically loads this file, and it is ignored by Git to prevent your credentials from being committed to the repository.