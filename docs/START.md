# Быстрый запуск проекта

## 1) Требования

- Установлен `.NET SDK 8.0+`
- ОС: Windows/Linux/macOS
- Терминал в корне репозитория

Проверка:

```powershell
dotnet --version
```

## 2) Восстановление и сборка решения

Из корня проекта:

```powershell
dotnet restore Warehouse.sln
dotnet build Warehouse.sln
```

## 3) Запуск backend (API)

В отдельном терминале:

```powershell
dotnet run --project .\backend\src\Warehouse.Api\Warehouse.Api.csproj
```

После запуска проверьте:

- `http://localhost:5071/health`
- `http://localhost:5071/api/warehouse/ping`

Порт уже зафиксирован в `launchSettings.json`, поэтому конфликтов с фронтом быть не должно.

```powershell
curl http://localhost:5071/health
curl http://localhost:5071/api/warehouse/ping
```

## 4) Запуск frontend (UI)

Во втором терминале:

```powershell
dotnet run --project .\frontend\Warehouse.Web\Warehouse.Web.csproj
```

Откройте: `http://localhost:5173`

Порт фронта тоже зафиксирован в `launchSettings.json`.

## 5) Полезные команды для разработки

Запуск с авто-перезагрузкой:

```powershell
dotnet watch --project .\backend\src\Warehouse.Api\Warehouse.Api.csproj run
dotnet watch --project .\frontend\Warehouse.Web\Warehouse.Web.csproj run
```
