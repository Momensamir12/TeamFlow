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
// Get user profile
export const getUserProfile = async () => {
  try {
    const response = await axiosInstance.get('/users/profile');
    return {
      success: true,
      data: response.data.data,
      message: response.data.message,
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to fetch profile',
    };
  }
};

// Update user profile
export const updateUserProfile = async (profileData) => {
  try {
    const response = await axiosInstance.put('/users/profile', profileData);
    return {
      success: true,
      message: response.data.message,
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to update profile',
    };
  }
};

// Change password
export const changePassword = async (passwordData) => {
  try {
    const response = await axiosInstance.post('/users/change-password', passwordData);
    return {
      success: true,
      message: response.data.message,
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to change password',
    };
  }
};

// Forgot password
export const forgotPassword = async (email) => {
  try {
    const response = await axiosInstance.post('/users/forgot-password', { email });
    return {
      success: true,
      message: response.data.message,
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to process request',
    };
  }
};

// Reset password
export const resetPassword = async (resetData) => {
  try {
    const response = await axiosInstance.post('/users/reset-password', resetData);
    return {
      success: true,
      message: response.data.message,
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to reset password',
    };
  }
};
