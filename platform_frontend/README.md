# Jitsi Meeting Platform Frontend

This is an Angular application for creating and managing Jitsi meetings with automatic recording and analysis capabilities.

## Features

- Create Jitsi meetings with custom room names
- Schedule meetings with start and end times
- Add participants via email addresses
- Enable/disable automatic recording
- Start and stop recording during meetings
- Process recordings automatically for analysis
- Modern RTL (Right-to-Left) UI for Arabic language support

## Prerequisites

- Node.js (version 18 or higher)
- Angular CLI (version 17 or higher)
- .NET 8.0 SDK (for backend)

## Installation

1. Install dependencies:
```bash
npm install
```

2. Install Font Awesome icons:
```bash
npm install @fortawesome/angular-fontawesome @fortawesome/fontawesome-free @fortawesome/fontawesome-svg-core @fortawesome/free-solid-svg-icons
```

## Running the Application

1. Start the backend server (from the `platform_backend` directory):
```bash
dotnet run
```
The backend will run on `http://localhost:5211`

2. Start the Angular development server:
```bash
ng serve
```
The frontend will run on `http://localhost:5211`

3. Open your browser and navigate to `http://localhost:5211`

## Usage

1. Navigate to the "Jitsi ميتنج" (Jitsi Meeting) page
2. Fill in the meeting details:
   - Room name (optional)
   - Start time
   - End time
   - Participant emails (comma-separated)
   - Enable recording option
3. Click "إنشاء الميتنج" (Create Meeting)
4. Once created, you can:
   - Join the meeting using the provided link
   - Start/stop recording
   - Process recordings for analysis

## API Endpoints

The application communicates with the following backend endpoints:

- `POST /api/external-integration/jitsi/create` - Create a new Jitsi meeting
- `GET /api/external-integration/jitsi/{id}` - Get meeting details
- `POST /api/external-integration/jitsi/{id}/start-recording` - Start recording
- `POST /api/external-integration/jitsi/{id}/stop-recording` - Stop recording
- `GET /api/external-integration/jitsi/{id}/recording-url` - Get recording URL
- `POST /api/external-integration/recordings/{id}/process` - Process recording

## Configuration

The API URL is configured in `src/environments/environment.ts`:
```typescript
export const environment = {
  apiUrl: 'http://localhost:5211/api',
  googleClientId: 'YOUR_GOOGLE_CLIENT_ID_HERE'
};
```

## Development

### Project Structure
```
src/
├── app/
│   ├── components/
│   │   └── jitsi-meeting/
│   │       └── jitsi-meeting.component.ts
│   ├── services/
│   │   └── external-integration.service.ts
│   └── models/
│       └── external-integration.model.ts
├── environments/
│   └── environment.ts
└── styles.scss
```

### Key Components

- **JitsiMeetingComponent**: Main component for creating and managing Jitsi meetings
- **ExternalIntegrationService**: Service for communicating with the backend API
- **External Integration Models**: TypeScript interfaces for data structures

## Troubleshooting

1. **CORS Errors**: Ensure the backend CORS policy allows requests from `http://localhost:5211`
2. **API Connection Issues**: Verify the backend is running on port 5211
3. **Font Awesome Icons Not Showing**: Make sure Font Awesome CSS is properly imported in `styles.scss`

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

This project is licensed under the MIT License. 