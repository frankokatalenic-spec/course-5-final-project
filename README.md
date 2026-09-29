# SecurityApp

A Razor Pages application for user registration and authentication, backed by Entity Framework Core.

## Security Summary

- `ValidationHelper` validates usernames, email addresses, and passwords. Registration applies email and password validation on the server.
- User queries use Entity Framework Core LINQ rather than concatenated raw SQL. A regression test checks that SQL-injection-shaped input is escaped in the MySQL provider's generated query.
- Cookie authentication is used for the web app. The user-delete handler challenges unauthenticated requests and forbids signed-in users who do not have the Admin role. The Admin-only Privacy page also uses role authorization.
- The user list displays a fixed mask instead of stored password hashes.
- The JWT signing key is required configuration; the app no longer uses a hardcoded fallback and fails if the key is missing.
- Copilot assisted with reviewing the authentication flow, identifying the cookie/JWT conflict, and adding security-focused tests. Tests verify behavior; they cannot establish authorship of the pre-existing code.

## Configuration

Keep the database connection string and JWT key out of tracked settings files. For local development, configure User Secrets from the repository root:

```powershell
dotnet user-secrets init --project .\SecurityApp\SecurityApp.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-connection-string>" --project .\SecurityApp\SecurityApp.csproj
dotnet user-secrets set "Jwt:Key" "<your-new-random-key>" --project .\SecurityApp\SecurityApp.csproj
```

Rotate any credentials that were previously committed. User Secrets are for development; use environment variables or a secret manager in production.

## Tests

Run the test project from the repository root:

```powershell
dotnet test .\SecurityApp.Tests\SecurityApp.Tests.csproj
```

The latest test run reported 8 passing tests. The coverage includes input validation, SQL-injection-shaped query input, and denial of delete requests from unauthenticated and non-admin users. The SQL check inspects generated MySQL SQL; it is not a live-database or full HTTP integration test. XSS coverage checks rejection of script-like usernames, not rendered-page behavior.

## Remaining Security Work

- Registration grants the Admin role based on an email suffix without verifying ownership. Assign privileged roles through a trusted administrative process instead.
- The seed-users POST handler is not currently protected by an Admin authorization check.
- Review the full Git history and rotate previously exposed secrets; deleting a secret from the latest settings file does not remove it from earlier commits.
- The most recent test run was performed without restoring after the test dependencies changed. Run `dotnet test` again to verify the final restored dependency graph.
