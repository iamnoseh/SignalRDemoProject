# SignalR Chat Demo - Полная документация

## 📋 Содержание
- [О проекте](#о-проекте)
- [Технологии](#технологии)
- [Архитектура](#архитектура)
- [Установка и запуск](#установка-и-запуск)
- [API Endpoints](#api-endpoints)
  - [Аутентификация](#аутентификация)
  - [Чат](#чат)
  - [Группы](#группы)
  - [Друзья](#друзья)
  - [Файлы](#файлы)
  - [Статус пользователей](#статус-пользователей)
- [SignalR Hub](#signalr-hub)
- [База данных](#база-данных)
- [Примеры использования](#примеры-использования)

---

## О проекте

**SignalR Chat Demo** - это полнофункциональное приложение для обмена сообщениями в реальном времени, построенное на ASP.NET Core и SignalR. Проект включает в себя:

- ✅ Регистрация и аутентификация пользователей (JWT)
- ✅ Глобальный чат
- ✅ Групповые чаты
- ✅ Приватные сообщения (1-на-1)
- ✅ Система друзей (отправка/принятие запросов)
- ✅ Загрузка и отправка файлов (изображения, документы)
- ✅ Профили пользователей (nickname, аватар)
- ✅ Статус пользователей (Online/Offline)
- ✅ Индикаторы набора текста (Typing indicators)
- ✅ Редактирование и удаление сообщений
- ✅ Реакции на сообщения (Reactions) 👍❤️😂
- ✅ Статус прочтения сообщений (Read Receipts) ✔✔

---

## Технологии

### Backend
- **ASP.NET Core 10.0** - Web API
- **SignalR** - Real-time коммуникация
- **Entity Framework Core** - ORM
- **PostgreSQL** - База данных
- **ASP.NET Core Identity** - Управление пользователями
- **JWT Bearer Authentication** - Аутентификация
- **Serilog** - Логирование
- **AspNetCoreRateLimit** - Защита от спама

### Архитектура
- **Clean Architecture** (Domain, Application, Infrastructure, WebApp)
- **CQRS pattern** для разделения команд и запросов
- **Repository pattern** через EF Core
- **Dependency Injection**

---

## Архитектура

```
SignalRDemo/
├── Domain/              # Сущности и бизнес-логика
│   └── Entities/
│       ├── AppUser.cs
│       ├── ChatMessage.cs
│       ├── ChatGroup.cs
│       ├── Friendship.cs
│       └── FriendRequest.cs
├── Application/         # Интерфейсы и DTO
│   ├── Auth/
│   └── Chat/
├── Infrastructure/      # Реализация сервисов и БД
│   ├── Auth/
│   ├── Chat/
│   └── Data/
└── WebApp/             # API Controllers и SignalR Hubs
    ├── Controllers/
    ├── Hubs/
    └── Program.cs
```

---

## Установка и запуск

### Требования
- .NET 10.0 SDK
- PostgreSQL 12+

### Шаги установки

1. **Клонировать репозиторий**
```bash
git clone <repository-url>
cd SignalRDemo
```

2. **Настроить строку подключения к БД**

Отредактируйте `WebApp/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=signalr_chat;Username=postgres;Password=yourpassword"
  }
}
```

3. **Применить миграции**
```bash
dotnet ef database update --project Infrastructure --startup-project WebApp
```

4. **Запустить приложение**
```bash
dotnet run --project WebApp
```

Приложение будет доступно по адресу: `https://localhost:5033`

Swagger UI: `https://localhost:5033/swagger`

---

## API Endpoints

### Аутентификация

#### 1. Регистрация пользователя
```http
POST /api/auth/register
Content-Type: application/json

{
  "userName": "john_doe",
  "email": "john@example.com",
  "password": "Password123"
}
```

**Ответ:**
```json
{
  "data": "User registration succeeded",
  "statusCode": 200
}
```

---

#### 2. Вход в систему
```http
POST /api/auth/login
Content-Type: application/json

{
  "userName": "john_doe",
  "password": "Password123"
}
```

**Ответ:**
```json
{
  "data": {
    "userName": "john_doe",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  },
  "statusCode": 200
}
```

**Использование токена:**
Все последующие запросы требуют заголовок:
```
Authorization: Bearer {token}
```

---

#### 3. Обновить профиль (nickname)
```http
PUT /api/auth/profile
Authorization: Bearer {token}
Content-Type: application/json

{
  "nickname": "JohnTheCool"
}
```

**Ответ:**
```json
{
  "data": true,
  "statusCode": 200
}
```

---

#### 4. Загрузить аватар
```http
POST /api/auth/profile/picture
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: <binary>
```

**Ответ:**
```json
{
  "data": "/uploads/profiles/user-id_guid.jpg",
  "statusCode": 200
}
```

---

### Чат

#### 1. Отправить сообщение в глобальный чат
```http
POST /api/chat/send
Authorization: Bearer {token}
Content-Type: application/json

{
  "message": "Hello everyone!",
  "type": 0,
  "fileUrl": null,
  "fileName": null
}
```

**Типы сообщений (type):**
- `0` - Text (текст)
- `1` - Image (изображение)
- `2` - File (файл)

**Ответ:**
```json
{
  "data": {
    "id": "guid",
    "userId": "user-id",
    "userName": "john_doe",
    "message": "Hello everyone!",
    "type": 0,
    "createdAt": "2025-11-27T12:00:00Z"
  },
  "statusCode": 200
}
```

---

#### 2. Получить историю глобального чата
```http
GET /api/chat/history?pageNumber=1&pageSize=50
Authorization: Bearer {token}
```

**Ответ:**
```json
{
  "data": {
    "items": [...],
    "totalCount": 150,
    "pageNumber": 1,
    "pageSize": 50
  },
  "statusCode": 200
}
```

---

#### 3. Отправить сообщение в группу
```http
POST /api/chat/send-to-group
Authorization: Bearer {token}
Content-Type: application/json

{
  "groupName": "Developers",
  "message": "Hi team!"
}
```

---

#### 4. Отправить приватное сообщение
```http
POST /api/chat/send-to-user
Authorization: Bearer {token}
Content-Type: application/json

{
  "toUserId": "receiver-user-id",
  "message": "Hey, how are you?"
}
```

---

#### 5. Редактировать сообщение
```http
PUT /api/chat/edit/{messageId}
Authorization: Bearer {token}
Content-Type: application/json

{
  "message": "Updated message text"
}
```

**Примечание:** Можно редактировать только свои сообщения.

---

#### 6. Удалить сообщение
```http
DELETE /api/chat/{messageId}
Authorization: Bearer {token}
```

**Примечание:** Сообщение удаляется "мягко" (soft delete), т.е. помечается как удаленное.

---

### Группы

#### 1. Создать группу
```http
POST /api/groups
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Developers",
  "description": "Group for developers"
}
```

---

#### 2. Получить список групп
```http
GET /api/groups
Authorization: Bearer {token}
```

---

#### 3. Получить участников группы
```http
GET /api/groups/{groupName}/members
Authorization: Bearer {token}
```

---

### Друзья

#### 1. Отправить запрос в друзья
```http
POST /api/friends/request/{receiverId}
Authorization: Bearer {token}
```

**Ответ:**
```json
{
  "data": "Friend request sent.",
  "statusCode": 200
}
```

**Возможные ошибки:**
- Пользователь не существует
- Уже друзья
- Запрос уже отправлен

---

#### 2. Принять запрос в друзья
```http
POST /api/friends/accept/{requestId}
Authorization: Bearer {token}
```

**Результат:** Создается дружба, запрос помечается как "Accepted".

---

#### 3. Отклонить запрос в друзья
```http
POST /api/friends/reject/{requestId}
Authorization: Bearer {token}
```

---

#### 4. Получить список друзей
```http
GET /api/friends
Authorization: Bearer {token}
```

**Ответ:**
```json
{
  "data": [
    {
      "id": "user-id-1",
      "userName": "alice",
      "nickname": "Alice Wonder",
      "profilePictureUrl": "/uploads/profiles/..."
    }
  ],
  "statusCode": 200
}
```

---

#### 5. Получить входящие запросы в друзья
```http
GET /api/friends/requests
Authorization: Bearer {token}
```

**Ответ:**
```json
{
  "data": [
    {
      "id": "request-guid",
      "senderId": "user-id",
      "sender": {
        "userName": "bob",
        "nickname": "Bob Builder"
      },
      "status": 0,
      "createdAt": "2025-11-27T10:00:00Z"
    }
  ],
  "statusCode": 200
}
```

---

### Файлы

#### 1. Загрузить файл
```http
POST /api/files/upload
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: <binary>
```

**Ответ:**
```json
{
  "url": "/uploads/guid_filename.jpg",
  "fileName": "original_filename.jpg"
}
```

**Использование:**
1. Загрузите файл через этот endpoint
2. Получите `url` и `fileName`
3. Отправьте сообщение с этими данными:
```json
{
  "message": "Check this out!",
  "type": 1,
  "fileUrl": "/uploads/guid_filename.jpg",
  "fileName": "original_filename.jpg"
}
```

---

### Статус пользователей

#### 1. Получить список онлайн пользователей
```http
GET /api/users/online
Authorization: Bearer {token}
```

**Ответ:**
```json
{
  "data": ["user-id-1", "user-id-2", "user-id-3"],
  "statusCode": 200
}
```

---

#### 2. Проверить статус конкретного пользователя
```http
GET /api/users/{userId}/status
Authorization: Bearer {token}
```

**Ответ:**
```json
{
  "data": true,  // true = онлайн, false = офлайн
  "statusCode": 200
}
```

---

## SignalR Hub

### Подключение к Hub

**URL:** `/chatHub`

**JavaScript пример:**
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub", {
        accessTokenFactory: () => yourJwtToken
    })
    .build();

await connection.start();
```

---

### Hub Methods (вызов с клиента)

#### 1. Отправить сообщение в глобальный чат
```javascript
await connection.invoke("SendMessage", "Hello everyone!");
```

---

#### 2. Присоединиться к группе
```javascript
await connection.invoke("JoinGroup", "Developers");
```

---

#### 3. Покинуть группу
```javascript
await connection.invoke("LeaveGroup", "Developers");
```

---

#### 4. Отправить сообщение в группу
```javascript
await connection.invoke("SendGroupMessage", "Developers", "Hi team!");
```

---

#### 5. Отправить приватное сообщение
```javascript
await connection.invoke("SendPrivateMessage", "receiver-user-id", "Hey!");
```

---

#### 6. Индикатор набора текста (группа)
```javascript
// Начал печатать
await connection.invoke("UserTyping", "Developers");

// Закончил печатать
await connection.invoke("UserStoppedTyping", "Developers");
```

---

#### 7. Индикатор набора текста (приватный чат)
```javascript
// Начал печатать
await connection.invoke("UserTypingPrivate", "receiver-user-id");

// Закончил печатать
await connection.invoke("UserStoppedTypingPrivate", "receiver-user-id");
```

---

#### 8. Редактировать сообщение
```javascript
await connection.invoke("EditMessage", messageId, "New text");
```

---

#### 9. Удалить сообщение
```javascript
await connection.invoke("DeleteMessage", messageId);
```

---

#### 10. Добавить реакцию на сообщение
```javascript
await connection.invoke("ReactToMessage", messageId, "👍");
// Или другие эмодзи: "❤️", "😂", "🔥", etc.
```

---

#### 11. Удалить свою реакцию
```javascript
await connection.invoke("RemoveReaction", messageId);
```

---

#### 12. Отметить сообщение как прочитанное
```javascript
await connection.invoke("MarkMessageAsRead", messageId);
```

**Примечание:** Только получатель приватного сообщения может отметить его как прочитанное.

---

### Hub Events (получение с сервера)

#### 1. Получить сообщение (глобальный чат)
```javascript
connection.on("ReceiveMessage", (userName, message, createdAt) => {
    console.log(`${userName}: ${message}`);
});
```

---

#### 2. Получить сообщение (группа)
```javascript
connection.on("ReceiveGroupMessage", (groupName, userName, message, createdAt) => {
    console.log(`[${groupName}] ${userName}: ${message}`);
});
```

---

#### 3. Получить приватное сообщение
```javascript
connection.on("ReceivePrivateMessage", (fromUserId, fromUserName, message, createdAt) => {
    console.log(`Private from ${fromUserName}: ${message}`);
});
```

---

#### 4. Системное сообщение
```javascript
connection.on("SystemMessage", (message) => {
    console.log(`System: ${message}`);
});
```

---

#### 5. Пользователь онлайн
```javascript
connection.on("UserOnline", (userId, userName) => {
    console.log(`${userName} is now online`);
});
```

---

#### 6. Пользователь офлайн
```javascript
connection.on("UserOffline", (userId, userName) => {
    console.log(`${userName} is now offline`);
});
```

---

#### 7. Пользователь печатает
```javascript
connection.on("UserTyping", (groupOrUserId, userName) => {
    console.log(`${userName} is typing...`);
});
```

---

#### 8. Пользователь перестал печатать
```javascript
connection.on("UserStoppedTyping", (groupOrUserId, userName) => {
    console.log(`${userName} stopped typing`);
});
```

---

#### 9. Сообщение отредактировано
```javascript
connection.on("MessageEdited", (messageId, newMessage, editedAt) => {
    console.log(`Message ${messageId} edited: ${newMessage}`);
});
```

---

#### 10. Сообщение удалено
```javascript
connection.on("MessageDeleted", (messageId) => {
    console.log(`Message ${messageId} deleted`);
});
```

---

#### 11. Реакция на сообщение
```javascript
connection.on("MessageReaction", (messageId, reactionDto) => {
    console.log(`Reaction on message ${messageId}:`, reactionDto);
    // reactionDto содержит: { id, userId, userName, reaction, createdAt }
    // Или { userId, removed: true } если реакция удалена
});
```

---

#### 12. Сообщение прочитано
```javascript
connection.on("MessageRead", (messageId, readByUserId, readAt) => {
    console.log(`Message ${messageId} was read by ${readByUserId} at ${readAt}`);
    // Показать двойную галочку (✔✔) в UI
});
```

---

## База данных

### Таблицы

1. **AspNetUsers** - Пользователи (Identity)
   - Id, UserName, Email, PasswordHash
   - Nickname, ProfilePictureUrl (кастомные поля)

2. **ChatMessages** - Сообщения
   - Id, UserId, UserName, Message
   - Type (Text/Image/File), FileUrl, FileName
   - IsPrivate, ReceiverUserId, GroupName
   - IsEdited, EditedAt, IsDeleted
   - IsRead, ReadAt (для приватных сообщений)

3. **ChatReactions** - Реакции на сообщения
   - Id, MessageId, UserId, Reaction (эмодзи)
   - CreatedAt

4. **ChatGroups** - Группы
   - Id, Name, Description, OwnerUserId

5. **ChatGroupMembers** - Участники групп
   - Id, GroupId, UserId, JoinedAt

6. **Friendships** - Дружба
   - User1Id, User2Id (составной ключ)
   - CreatedAt

7. **FriendRequests** - Запросы в друзья
   - Id, SenderId, ReceiverId
   - Status (Pending/Accepted/Rejected)
   - CreatedAt

---

## Примеры использования

### Полный flow: Регистрация → Чат

```javascript
// 1. Регистрация
const registerResponse = await fetch('/api/auth/register', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
        userName: 'john',
        email: 'john@example.com',
        password: 'Pass123'
    })
});

// 2. Вход
const loginResponse = await fetch('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
        userName: 'john',
        password: 'Pass123'
    })
});
const { data: { token } } = await loginResponse.json();

// 3. Подключение к SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub", {
        accessTokenFactory: () => token
    })
    .build();

connection.on("ReceiveMessage", (userName, message) => {
    console.log(`${userName}: ${message}`);
});

await connection.start();

// 4. Отправить сообщение
await connection.invoke("SendMessage", "Hello everyone!");
```

---

### Отправка изображения

```javascript
// 1. Загрузить файл
const formData = new FormData();
formData.append('file', imageFile);

const uploadResponse = await fetch('/api/files/upload', {
    method: 'POST',
    headers: { 'Authorization': `Bearer ${token}` },
    body: formData
});
const { url, fileName } = await uploadResponse.json();

// 2. Отправить сообщение с изображением
await fetch('/api/chat/send', {
    method: 'POST',
    headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    },
    body: JSON.stringify({
        message: 'Check this photo!',
        type: 1, // Image
        fileUrl: url,
        fileName: fileName
    })
});
```

---

## Безопасность

- ✅ JWT аутентификация для всех endpoints
- ✅ Rate limiting для защиты от спама
- ✅ Валидация входных данных
- ✅ Soft delete для сообщений
- ✅ Проверка прав доступа (можно редактировать/удалять только свои сообщения)

---

## Логирование

Все логи сохраняются в:
- **Консоль** - для разработки
- **Файл** `logs/signalr-{date}.txt` - для продакшена

Уровни логирования настроены в `Program.cs`.

---

## Автор

Создано как демонстрационный проект для изучения SignalR и Clean Architecture.

## Лицензия

MIT
