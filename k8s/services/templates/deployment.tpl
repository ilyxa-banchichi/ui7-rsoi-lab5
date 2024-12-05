{{- define "deployment.template" }}
apiVersion: apps/v1
kind: Deployment
metadata:
  name: {{ .ctx.Release.Name }}-{{.service.name}}-dep
  labels:
    app: {{ .ctx.Release.Name }}-{{.service.name}}
spec:
  replicas: {{.service.replicaCount}}
  selector:
    matchLabels:
      app: {{ .ctx.Release.Name }}-{{.service.name}}
  template:
    metadata:
      name: {{ .ctx.Release.Name }}-{{.service.name}}
      labels:
        app: {{ .ctx.Release.Name }}-{{.service.name}}
    spec:
      containers:
        - name: {{ .ctx.Release.Name }}-{{.service.name}}
          image: {{.service.container}}
          imagePullPolicy: Always
          env:
          {{- if .service.envVars }}
          {{- range .service.envVars }}
          - name: {{ .name }}
            value: {{ .value | quote }}
          {{- end }}
          {{- end }}
      restartPolicy: Always
{{- end}}

