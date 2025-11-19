import api from './axiosConfig';


// Get user's tasks
export const getUserTasks = async () => {
  try {
    const response = await api.get('/users/tasks/my');
    return response.data;
  } catch (error) {
    console.error('Error fetching tasks:', error);
    throw error;
  }
};

// Add new task
export const addTask = async (taskData) => {
  try {
    const response = await api.post('/tasks', taskData);
    return response.data;
  } catch (error) {
    console.error('Error creating task:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to create task'
    };
  }
};

// Update task
export const updateTask = async (taskData) => {
  try {
    const response = await api.put('/tasks', taskData);
    return response.data;
  } catch (error) {
    console.error('Error updating task:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to update task'
    };
  }
};

// Update task status
export const updateTaskStatus = async (taskId, status) => {
  try {
    const response = await api.put('/tasks/status', {
      taskId,
      status
    });
    return response.data;
  } catch (error) {
    console.error('Error updating task status:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to update task status'
    };
  }
};

// Delete task
export const deleteTask = async (taskId) => {
  try {
    const response = await api.delete(`/tasks/${taskId}`);
    return response.data;
  } catch (error) {
    console.error('Error deleting task:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to delete task'
    };
  }
};