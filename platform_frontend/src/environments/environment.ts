export const environment = {
  apiUrl: 'https://localhost:5211/api',
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