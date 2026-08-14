# Repository Guidance

## Aggregate health checks

When adding a downstream health check, inspect `RegisterHttpClients` and mirror
the current client’s authentication convention in a dedicated named health
client. Add a service-token handler only when the existing client already uses
configured service credentials; do not introduce credentials or configuration
for a downstream whose existing calls are unauthenticated. Do not use
delegated-user token flows from an anonymous health endpoint.
