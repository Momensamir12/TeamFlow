import * as signalR from '@microsoft/signalr';
import { SIGNAlR_URL } from '../constants/config';

class SignalRService {
  constructor() {
    this.connection = null;
    this.isConnected = false;
  }

  async start() {
    const token = sessionStorage.getItem('accessToken');
    
    if (!token) {
      console.warn('[SignalR] No access token found, cannot connect to SignalR');
      return;
    }

    if (this.isConnected) {
      console.log('[SignalR] Already connected, skipping connection');
      return;
    }

    console.log('[SignalR] Starting connection to:', SIGNAlR_URL);

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${SIGNAlR_URL}`, {
        accessTokenFactory: () => {
          console.log('[SignalR] Providing access token for connection');
          return token;
        },
        transport: signalR.HttpTransportType.WebSockets,
        withCredentials: true,
        skipNegotiation: false
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Setup event handlers BEFORE starting connection
    this.connection.onreconnecting((error) => {
      this.isConnected = false;
      console.warn('[SignalR] 🔄 Reconnecting...', error);
    });

    this.connection.onreconnected((connectionId) => {
      this.isConnected = true;
      console.log('[SignalR] ✅ Reconnected successfully. ConnectionId:', connectionId);
    });

    this.connection.onclose((error) => {
      this.isConnected = false;
      console.warn('[SignalR] ❌ Connection closed', error);
    });

    try {
      await this.connection.start();
      this.isConnected = true;
      console.log('[SignalR] ✅ Connection established successfully. ConnectionId:', this.connection.connectionId);
    } catch (err) {
      console.error('[SignalR] ❌ Connection Error:', err);
      this.isConnected = false;
    }
  }

  async stop() {
    if (this.connection) {
      console.log('[SignalR] Stopping connection...');
      await this.connection.stop();
      this.isConnected = false;
      console.log('[SignalR] Connection stopped');
    }
  }

  onNotification(callback) {
    if (this.connection) {
      console.log('[SignalR] Registering handler for NewTaskAssignedNotification');
      this.connection.on('NewTaskAssignedNotification', (taskId) => {
        console.log('[SignalR] 📬 Received NewTaskAssignedNotification for taskId:', taskId);
        callback(taskId);
      });
    } else {
      console.error('[SignalR] Cannot register notification handler - connection is null');
    }
  }

  offNotification(callback) {
    if (this.connection) {
      console.log('[SignalR] Unregistering handler for NewTaskAssignedNotification');
      this.connection.off('NewTaskAssignedNotification', callback);
    }
  }
}

export default new SignalRService();
