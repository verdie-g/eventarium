# Eventarium

Eventarium turns live development activity (commits, issues, pull requests, reviews, comments) into an animated,
real-time visualization.

## OpenTelemetry

Set `OTEL_EXPORTER_OTLP_ENDPOINT` (or a signal-specific `OTEL_EXPORTER_OTLP_*_ENDPOINT`) to export logs, traces,
and metrics over OTLP. Standard OTLP environment variables configure the protocol, headers, compression, and timeouts.

`OTEL_SERVICE_NAME` overrides the default service name, `eventarium`. Set `service.instance.id` in
`OTEL_RESOURCE_ATTRIBUTES` to override the default instance ID, `1`.
