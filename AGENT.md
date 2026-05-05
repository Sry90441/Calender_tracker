Agent instructions for TaskPlan_Calendar

Purpose
- Persist project decisions and agent rules so the assistant and contributors follow the same constraints.

Project choices
- Stack: ASP.NET Core (Razor Pages) + EF Core + SQLite
- Frontend: Razor Pages + Bootstrap (responsive)
- Authentication: ASP.NET Core Identity (local accounts)

Initial features & pages
- Login page: simple email + password, Login and Sign up buttons, and a "Create Test User 1" button for quick testing (no external providers now).
- Index (home) page: responsive layout with a left sidebar (buttons: A, B, C, D), a dashboard-like home view showing today's todos and calendar entries, and a quick add todo for today.

Architecture & code quality
- Prioritize clean code and modularity: separate folders for Pages, Models, Services, Data, and ViewModels. Use dependency injection and small services.
- Keep Razor Pages thin; put business logic in services.
- Write EF Core migrations; use SQLite for easy cross-platform hosting.

Developer rules
- Do NOT auto-commit or push changes. Ask for explicit confirmation before making any git commit or push.
- Keep changes surgical and focused; update README when adding public features.
- Run tests and build after changes when relevant.

Dev setup commands (local)
- dotnet restore
- dotnet ef migrations add Initial
- dotnet ef database update
- dotnet run

Notes
- This file aims to be the single-source reminder for the assistant and contributors. Update it if project choices change.
