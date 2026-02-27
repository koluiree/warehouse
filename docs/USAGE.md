# Как пользоваться системой

## 1. Запуск

Откройте 2 терминала в корне проекта.

### Backend

```powershell
dotnet run --project .\backend\src\Warehouse.Api\Warehouse.Api.csproj
```

API доступен на `http://localhost:5071`.

### Frontend

```powershell
dotnet run --project .\frontend\Warehouse.Web\Warehouse.Web.csproj
```

Сайт доступен на `http://localhost:5173`.

## 2. Вход в систему

Страница входа: `http://localhost:5173/auth`

### Администратор

- Логин: `admin`
- Пароль: `Admin12345`

### Требования для регистрации

- Логин: 4-32 символа, только латиница/цифры/`_`
- Пароль: минимум 8 символов, хотя бы 1 буква и 1 цифра

## 3. Рабочий сценарий (с нуля)

1. Войдите в систему.
2. Перейдите в `Остатки`.
3. Создайте склад (блок "Создать склад").
4. Добавьте продукт (блок "Добавить продукт").
5. Выполните пополнение (выберите склад, продукт, количество, нажмите `Пополнение`).
6. Перейдите в `Заявки и выдача`.
7. Создайте заявку (`Новая заявка`), выбрав продукт и количество.
8. Одобрите заявку и начните выдачу (`Одобрить` -> `Начать выдачу`).
9. В блоке деталей заявки выберите позицию и выполните `Выдать выбранную позицию`.
10. Проверьте журнал на странице `История перемещений`.

## 4. Если порт занят

### Порт backend `5071`

```powershell
$procId = (Get-NetTCPConnection -LocalPort 5071 -State Listen).OwningProcess
Stop-Process -Id $procId -Force
```

### Порт frontend `5173`

```powershell
$procId = (Get-NetTCPConnection -LocalPort 5173 -State Listen).OwningProcess
Stop-Process -Id $procId -Force
```
