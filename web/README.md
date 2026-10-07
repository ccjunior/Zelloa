# Zelloa

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 22.2.1.

## Development servers

This Angular workspace contains two applications, so specify which project to serve. From the `web/` directory, start either application with:

```powershell
npm start -- zelloa-family
```

```powershell
npm start -- zelloa-school --port 4201
```

Family uses port `4200` by default. Run School on port `4201` to match the configured invitation activation URL and local API CORS origin. To run both applications at the same time, use separate terminals:

```powershell
npm start -- zelloa-family --port 4200
```

```powershell
npm start -- zelloa-school --port 4201
```

Then open `http://localhost:4200/` for Family and `http://localhost:4201/` for School. Both development servers reload when source files change.

When running the API with the `http` launch profile (`dotnet run` from `src/Zelloa.Api`), it listens on `http://localhost:5027`. The School development environment targets this local API port. The Docker Compose API instead uses `http://localhost:8080`; switch the School development API URL only when using that container.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
