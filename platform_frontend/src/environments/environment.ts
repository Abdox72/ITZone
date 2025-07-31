export const environment = {
  apiUrl: 'http://localhost:5211/api',
  googleClientId: '700910860503-k5ucu00uk2q8cjsch2gktevfm0nfq32l.apps.googleusercontent.com'
  googleClientId: 'YOUR_GOOGLE_CLIENT_ID_HERE',
  jitsiService: {
    domain: 'https://localhost:8443',
    apiUrl: 'https://localhost:8443/api',
    websocketUrl: 'wss://localhost:8443/websocket',
    applicationId: 'your-jitsi-app-id',
    secret: 'your-jitsi-secret'
  },
  externalServices: {
    trello: {
      apiKey: 'YOUR_TRELLO_API_KEY',
      apiUrl: 'https://api.trello.com/1'
    },
    gmail: {
      clientId: 'YOUR_GMAIL_CLIENT_ID',
      apiUrl: 'https://gmail.googleapis.com'
    },
    googleCalendar: {
      clientId: 'YOUR_GOOGLE_CALENDAR_CLIENT_ID',
      apiUrl: 'https://www.googleapis.com/calendar/v3'
    }
  }
}; 