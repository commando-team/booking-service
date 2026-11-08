# booking-service

## QA-проверки

Тесты API находятся в `BookingService/API.Tests` и разделены на два слоя:

- `Unit` — бизнес-правила бронирований и ресурсов без внешних сервисов;
- `Integration` — HTTP-маршруты API, JWT-защита и формат `ProblemDetails`.

Локальный запуск:

```bash
dotnet test BookingService/API.Tests/API.Tests.csproj --configuration Release
```

Integration-тесты используют окружение `Testing`: RabbitMQ заменён на in-memory транспорт, MongoDB не требуется для проверяемых маршрутов. В CI дополнительно сохраняются TRX-результаты и Cobertura coverage.
