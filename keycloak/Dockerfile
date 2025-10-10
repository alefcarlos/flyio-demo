FROM quay.io/keycloak/keycloak:26.4.0  AS builder

# https://www.keycloak.org/server/all-config?f=build
ENV KC_HEALTH_ENABLED=true
ENV KC_METRICS_ENABLED=true
ENV KC_EVENT_METRICS_USER_ENABLED=true
ENV KC_TRACING_ENABLED=true
ENV KC_DB=postgres
ENV KC_HTTP_MANAGEMENT_HEALTH_ENABLED=false

WORKDIR /opt/keycloak
RUN /opt/keycloak/bin/kc.sh build

FROM quay.io/keycloak/keycloak:26.4.0
COPY --from=builder /opt/keycloak/ /opt/keycloak/

ENV KC_HTTP_METRICS_HISTOGRAMS_ENABLED=true
ENV KC_HTTP_METRICS_SLOS=5,10,25,50,250,500,1000,2500,5000,10000
ENV KC_EVENT_METRICS_USER_TAGS=realm,clientId
ENV KC_PROXY_HEADERS=forwarded
ENTRYPOINT ["/opt/keycloak/bin/kc.sh"]