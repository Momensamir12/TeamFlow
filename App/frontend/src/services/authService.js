import axios from 'axios';
import { API_BASE_URL, TOKEN_KEY } from '../utils/constants';

const API_URL = `${API_BASE_URL}/api/Auth`;

// Create axios instance
const api = axios.create({
  baseURL: API_BASE_URL,
});

// Add token to requests
api.interceptors.request.use(
  (config) => {
    const tokens = getCurrentUserTokens();
    if (tokens?.accessToken) {
      config.headers.Authorization = `Bearer ${tokens.accessToken}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Handle token refresh
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        const tokens = getCurrentUserTokens();
        if (tokens?.refreshToken) {
          const response = await axios.post(`${API_URL}/refresh-token`, {
            refreshToken: tokens.refreshToken
          });
          
          const newTokens = response.data;
          localStorage.setItem(TOKEN_KEY, JSON.stringify(newTokens));
          
          // Retry original request
          originalRequest.headers.Authorization = `Bearer ${newTokens.accessToken}`;
          return api(originalRequest);
        }
      } catch (refreshError) {
        // Refresh failed, logout user
        localStorage.removeItem(TOKEN_KEY);
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }
    
    return Promise.reject(error);
  }
);

// Helper function to get tokens
const getCurrentUserTokens = () => {
  try {
    return JSON.parse(localStorage.getItem(TOKEN_KEY));
  } catch {
    return null;
  }
};

export const authService = {
  // Register user
  register: async (userData) => {
    const response = await axios.post(`${API_URL}/register`, userData);
    return response.data;
  },

  // Login user
  login: async (credentials) => {
    const response = await axios.post(`${API_URL}/login`, credentials);
    if (response.data.accessToken) {
      localStorage.setItem(TOKEN_KEY, JSON.stringify(response.data));
    }
    return response.data;
  },

  // Logout user
  logout: () => {
    localStorage.removeItem(TOKEN_KEY);
  },

  // Get current user tokens
  getCurrentUserTokens,

  // Check if user is authenticated
  isAuthenticated: () => {
    const tokens = getCurrentUserTokens();
    return !!tokens?.accessToken;
  },

  // Get auth header for requests
  getAuthHeader: () => {
    const tokens = getCurrentUserTokens();
    if (tokens?.accessToken) {
      return { Authorization: `Bearer ${tokens.accessToken}` };
    }
    return {};
  }
};

export default api;