# ITZone - Meeting Platform with Self-Hosted Jitsi Integration

A comprehensive meeting management platform with self-hosted Jitsi video conferencing integration, built with Angular frontend and ASP.NET Core backend.

## 🚀 Features

### Core Features
- **Meeting Management**: Create, schedule, and manage meetings
- **Self-Hosted Jitsi Integration**: Video conferencing with your own Jitsi server
- **Recording & Transcription**: Automatic meeting recording and speech-to-text
- **Meeting Analysis**: AI-powered meeting content analysis
- **Task Management**: Extract and manage tasks from meetings
- **External Integrations**: Connect with Trello, Gmail, Google Calendar

### Advanced Features
- **Real-time Transcription**: Live speech-to-text during meetings
- **Meeting Analytics**: Detailed meeting statistics and insights
- **Automated Task Extraction**: AI-powered task identification
- **Multi-language Support**: Arabic and English interface
- **Responsive Design**: Works on desktop and mobile devices

## 🛠️ Setup Instructions

### Prerequisites
- Node.js 18+ and npm
- .NET 8.0 SDK
- Self-hosted Jitsi server running on `https://localhost:8443`

### 1. Self-Hosted Jitsi Setup

#### Option A: Docker (Recommended)
```bash
# Create docker-compose.yml for Jitsi
version: '3.8'
services:
  jitsi-web:
    image: jitsi/web:latest
    ports:
      - "8443:443"
    environment:
      - ENABLE_AUTH=1
      - ENABLE_GUESTS=1
      - ENABLE_RECORDING=1
      - ENABLE_TRANSCRIPTION=1
      - JICOFO_AUTH_PASSWORD=your-secret
    volumes:
      - ./jitsi-config:/config:Z
      - ./jitsi-web:/web:Z

  jitsi-prosody:
    image: jitsi/prosody:latest
    environment:
      - ENABLE_AUTH=1
      - ENABLE_GUESTS=1
      - JICOFO_AUTH_PASSWORD=your-secret
    volumes:
      - ./jitsi-config:/config:Z

  jitsi-jicofo:
    image: jitsi/jicofo:latest
    environment:
      - ENABLE_AUTH=1
      - JICOFO_AUTH_PASSWORD=your-secret
    volumes:
      - ./jitsi-config:/config:Z

  jitsi-jvb:
    image: jitsi/jvb:latest
    environment:
      - ENABLE_AUTH=1
    volumes:
      - ./jitsi-config:/config:Z
```

#### Option B: Manual Installation
Follow the [official Jitsi installation guide](https://jitsi.org/downloads/) for your platform.

### 2. Backend Setup

```bash
cd platform_backend

# Update appsettings.json with your Jitsi configuration
{
  "JitsiService": {
    "ApiUrl": "https://localhost:8443/api",
    "Domain": "https://localhost:8443",
    "ApplicationId": "your-jitsi-app-id",
    "Secret": "your-jitsi-secret"
  }
}

# Run the backend
dotnet run
```

### 3. Frontend Setup

```bash
cd platform_frontend

# Install dependencies
npm install

# Update environment.ts with Jitsi configuration
export const environment = {
  apiUrl: 'http://localhost:5211/api',
  jitsiService: {
    domain: 'https://localhost:8443',
    apiUrl: 'https://localhost:8443/api',
    applicationId: 'your-jitsi-app-id',
    secret: 'your-jitsi-secret'
  }
};

# Run the frontend
ng serve
```

## 🔧 Configuration

### Jitsi Service Configuration

#### Environment Variables
```bash
# Jitsi API Configuration
JITSI_API_URL=https://localhost:8443/api
JITSI_DOMAIN=https://localhost:8443
JITSI_APPLICATION_ID=your-app-id
JITSI_SECRET=your-secret

# Recording Configuration
JITSI_RECORDING_ENABLED=true
JITSI_TRANSCRIPTION_ENABLED=true
JITSI_RECORDING_PATH=/recordings
```

#### JWT Authentication
The application uses JWT tokens for secure communication with the Jitsi server:

```csharp
// Backend JWT Generation
private string GenerateJWT()
{
    var payload = new
    {
        aud = "jitsi",
        iss = _jitsiApplicationId,
        sub = _jitsiDomain,
        room = "*",
        exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()
    };
    
    return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload)));
}
```

### External Service Integrations

#### Trello Integration
```json
{
  "ExternalServices": {
    "Trello": {
      "ApiKey": "YOUR_TRELLO_API_KEY",
      "ApiUrl": "https://api.trello.com/1"
    }
  }
}
```

#### Gmail Integration
```json
{
  "ExternalServices": {
    "Gmail": {
      "ClientId": "YOUR_GMAIL_CLIENT_ID",
      "ApiUrl": "https://gmail.googleapis.com"
    }
  }
}
```

## 🚀 Enhancement Suggestions

### 1. Advanced Analytics Dashboard
```typescript
// Enhanced analytics service
export interface MeetingAnalytics {
  totalMeetings: number;
  totalDuration: number;
  averageParticipants: number;
  recordingStats: {
    totalRecordings: number;
    totalTranscriptions: number;
    averageAccuracy: number;
  };
  taskExtractionStats: {
    totalTasks: number;
    completedTasks: number;
    averageTasksPerMeeting: number;
  };
}
```

### 2. Real-time Collaboration Features
- **Live Chat**: In-meeting chat functionality
- **Screen Sharing**: Enhanced screen sharing with annotations
- **Whiteboard**: Collaborative whiteboard during meetings
- **File Sharing**: Secure file sharing within meetings

### 3. AI-Powered Enhancements
```typescript
// AI Analysis Service
export interface AIAnalysisResult {
  sentiment: 'positive' | 'neutral' | 'negative';
  keyTopics: string[];
  actionItems: Array<{
    text: string;
    priority: 'high' | 'medium' | 'low';
    assignedTo?: string;
    dueDate?: Date;
  }>;
  decisions: Array<{
    text: string;
    confidence: number;
    participants: string[];
  }>;
  summary: string;
}
```

### 4. Advanced Security Features
- **End-to-End Encryption**: Enhanced security for sensitive meetings
- **Role-Based Access Control**: Granular permissions for meeting participants
- **Audit Logging**: Comprehensive activity logging
- **Compliance Features**: GDPR, HIPAA compliance tools

### 5. Mobile Application
```bash
# Ionic/Angular Mobile App
ionic start itzone-mobile tabs --type=angular
cd itzone-mobile
ionic capacitor add ios
ionic capacitor add android
```

### 6. API Enhancements
```csharp
// WebSocket Support for Real-time Updates
public class MeetingHub : Hub
{
    public async Task JoinMeeting(string meetingId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, meetingId);
    }
    
    public async Task SendMessage(string meetingId, string message)
    {
        await Clients.Group(meetingId).SendAsync("ReceiveMessage", message);
    }
}
```

### 7. Performance Optimizations
- **Caching Strategy**: Redis caching for meeting data
- **CDN Integration**: Static asset delivery optimization
- **Database Optimization**: Indexing and query optimization
- **Load Balancing**: Horizontal scaling support

### 8. Monitoring and Observability
```csharp
// Health Checks
public class JitsiHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        // Check Jitsi service availability
        return HealthCheckResult.Healthy();
    }
}
```

## 📊 API Endpoints

### Jitsi Integration Endpoints
```
POST /api/external-integration/jitsi/create
GET /api/external-integration/jitsi/{meetingId}
POST /api/external-integration/jitsi/{meetingId}/start-recording
POST /api/external-integration/jitsi/{meetingId}/stop-recording
GET /api/external-integration/jitsi/{meetingId}/recording-url
GET /api/external-integration/jitsi/{meetingId}/transcription
```

### Meeting Management Endpoints
```
GET /api/meeting
POST /api/meeting
GET /api/meeting/{id}
PUT /api/meeting/{id}
DELETE /api/meeting/{id}
POST /api/meeting/{id}/complete
POST /api/meeting/{id}/start
```

## 🔒 Security Considerations

1. **JWT Token Management**: Implement proper JWT token rotation
2. **HTTPS Enforcement**: Ensure all communications use HTTPS
3. **Input Validation**: Validate all user inputs
4. **Rate Limiting**: Implement API rate limiting
5. **CORS Configuration**: Proper CORS setup for cross-origin requests

## 🧪 Testing

### Unit Tests
```bash
# Backend Tests
dotnet test

# Frontend Tests
ng test
```

### Integration Tests
```bash
# API Integration Tests
dotnet test --filter "Category=Integration"
```

## 📈 Deployment

### Docker Deployment
```bash
# Build and run with Docker Compose
docker-compose up -d
```

### Azure Deployment
```bash
# Deploy to Azure App Service
az webapp up --name itzone-backend --resource-group myResourceGroup
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the documentation

---

**Note**: This is a comprehensive setup for integrating self-hosted Jitsi with Angular and ASP.NET Core. The enhancement suggestions provide a roadmap for future development and scaling of the application. 