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
      console.warn('No access token found, cannot connect to SignalR');
      return;
    }

    if (this.isConnected) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${SIGNAlR_URL}`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    try {
      await this.connection.start();
      this.isConnected = true;
    } catch (err) {
      console.error('SignalR Connection Error: ', err);
      this.isConnected = false;
    }

    this.connection.onreconnecting(() => {
      this.isConnected = false;
    });

    this.connection.onreconnected(() => {
      this.isConnected = true;
    });

    this.connection.onclose(() => {
      this.isConnected = false;
    });
  }

  async stop() {
    if (this.connection) {
      await this.connection.stop();
      this.isConnected = false;
    }
  }

  onNotification(callback) {
    if (this.connection) {
      this.connection.on('NewTaskAssignedNotification', callback);
    }
  }

  offNotification(callback) {
    if (this.connection) {
      this.connection.off('NewTaskAssignedNotification', callback);
    }
  }
}

export default new SignalRService();
