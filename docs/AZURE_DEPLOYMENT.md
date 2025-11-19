# Azure App Service Deployment Guide

## Overview
This guide explains how to properly configure the TeamFlow backend application for deployment to Azure App Service.

## Required Configuration Settings

After deploying to Azure App Service, you **must** configure the following application settings in the Azure portal:

### 1. JWT Settings
Navigate to: **Azure Portal → Your App Service → Configuration → Application Settings**

Add the following settings:

| Name | Value | Description |
|------|-------|-------------|
| `JwtSettings__SecretKey` | Your secure secret key | **REQUIRED**: Must be at least 32 characters long. Use a cryptographically secure random string. |
| `JwtSettings__Issuer` | `TeamFlowAppAuth` | JWT token issuer |
| `JwtSettings__Audience` | `back-end` | JWT token audience |
| `JwtSettings__ExpiryMinutes` | `600` | Token expiry time in minutes |

**Important**: The `JwtSettings__SecretKey` is critical for security. Generate a strong random key:
```bash
# Example using PowerShell
[Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Maximum 256 }))

# Example using OpenSSL
openssl rand -base64 64
```

### 2. Email Settings
Configure your email service provider settings:

| Name | Value | Description |
|------|-------|-------------|
| `EmailSettings__SmtpServer` | `smtp.gmail.com` | SMTP server address |
| `EmailSettings__SmtpPort` | `587` | SMTP server port |
| `EmailSettings__Username` | Your email username | Email account username |
| `EmailSettings__Password` | Your app password | **REQUIRED**: Email account app password |
| `EmailSettings__SenderEmail` | Your sender email | **REQUIRED**: Email address for sending |
| `EmailSettings__SenderName` | `TeamFlow` | Display name for sent emails |

**Note for Gmail**: 
- Use an [App Password](https://support.google.com/accounts/answer/185833) instead of your regular password
- Enable 2-factor authentication on your Google account first

### 3. Application Settings (Optional)

| Name | Value | Description |
|------|-------|-------------|
| `AppSettings__BackendUrl` | Your Azure URL | Full URL of your Azure App Service (e.g., `https://teamflow-xxx.azurewebsites.net`) |
| `AppSettings__FrontendUrl` | Your frontend URL | URL of your frontend application for CORS |

**Note**: If `FrontendUrl` is not set in production, the API will temporarily allow requests from any origin. For security, you should configure this with your actual frontend URL.

### 4. Connection Strings

| Name | Value | Description |
|------|-------|-------------|
| `ConnectionStrings__SqliteConnection` | `Data Source=teamflow.db` | SQLite database file path (default is fine) |

## Configuration Steps

1. **Open Azure Portal**
   - Navigate to your App Service resource

2. **Go to Configuration**
   - In the left menu, select **Configuration** under **Settings**

3. **Add Application Settings**
   - Click **+ New application setting** for each required setting
   - Enter the **Name** and **Value** as specified above
   - Click **OK** after each setting

4. **Save Configuration**
   - Click **Save** at the top of the Configuration page
   - Click **Continue** to confirm the restart

5. **Verify Deployment**
   - Wait for the app to restart (usually takes 1-2 minutes)
   - Visit your Azure URL to verify the app is running
   - The root endpoint should now work without errors

## Troubleshooting

### Error: "Invalid request parameters"
This typically means required configuration values are missing. Check that all **REQUIRED** settings are configured:
- `JwtSettings__SecretKey`
- `EmailSettings__Password`
- `EmailSettings__SenderEmail`

### Error: "JWT Secret Key is not configured"
The `JwtSettings__SecretKey` setting is missing or empty. Add it with a secure random value of at least 32 characters.

### CORS Errors
If you're getting CORS errors from your frontend:
- Set `AppSettings__FrontendUrl` to your actual frontend URL
- Make sure the URL doesn't end with a slash
- Example: `https://myapp.com` (correct), `https://myapp.com/` (incorrect)

## Security Best Practices

1. **Never commit secrets** to version control
2. **Use strong random keys** for JWT secret (64+ characters recommended)
3. **Configure CORS properly** - don't leave `FrontendUrl` empty in production
4. **Use App Passwords** for email services, not your main account password
5. **Enable Application Insights** in Azure for monitoring and diagnostics
6. **Set up SSL/TLS** - Azure App Service provides this by default

## Environment Variables Format

When configuring in Azure App Service, use double underscores (`__`) to represent nested JSON keys:

- `JwtSettings:SecretKey` in appsettings.json → `JwtSettings__SecretKey` in Azure
- `EmailSettings:Password` in appsettings.json → `EmailSettings__Password` in Azure

This allows Azure environment variables to override the values in appsettings.Production.json.
