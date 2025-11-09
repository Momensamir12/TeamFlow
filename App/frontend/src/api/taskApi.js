import axiosInstance from './axiosConfig';



// Get user's tasks
export const getUserTasks = async () => {
  try {
    const response = await axiosInstance.get('/users/tasks/my');
    return {
      success: true,
      data: response.data.data,
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to fetch tasks',
    };
  }
};

// Add new task
export const addTask = async (taskData) => {
  try {
    const response = await axiosInstance.post('/tasks', taskData);
    return {
      success: true,
      data: response.data.data,
      message: response.data.message,
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to add task',
    };
  }
};

// Update task
export const updateTask = async (taskId, taskData) => {
  try {
    const response = await axiosInstance.put(`/tasks/${taskId}`, taskData);
    return {
      success: true,
      data: response.data.data,
      message: response.data.message,
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to update task',
    };
  }
};

// Delete task
export const deleteTask = async (taskId) => {
  try {
    const response = await axiosInstance.delete(`/tasks/${taskId}`);
    return {
      success: true,
      message: response.data.message,
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to delete task',
    };
  }
};