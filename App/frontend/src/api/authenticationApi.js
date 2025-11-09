import axiosInstance from './axiosConfig';

// Login user
export const loginUser = async (username, password) => {
  try {
    const response = await axiosInstance.post('/auth/login', {
      username,
      password,
    });

    return {
      success: true,
      data: response.data.data,
      message: response.data.message,
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Login failed',
    };
  }
};

// Register user
export const registerUser = async (userData) => {
  try {
    const response = await axiosInstance.post('/users/register', userData);

    return {
      success: true,
      message: response.data.message,
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Registration failed',
    };
  }
};

export const refreshAccessToken = async (refreshToken) => {
  try {
    const response = await axiosInstance.post('/auth/refresh', {
      refreshToken,
    });

    return {
      success: true,
      data: response.data.data,
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Token refresh failed',
    };
  }
};