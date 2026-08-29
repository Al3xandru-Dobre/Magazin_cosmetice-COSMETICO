export const environment = {
  production: false,
  // API ruleaza in Docker (docker compose up). Pentru `dotnet run` local foloseste http://localhost:5080/api
  apiUrl: 'http://localhost:8080/api',
};
