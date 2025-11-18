import api from './axiosConfig';

export const taskCommentApi = {
  getTaskComments: async (taskId) => {
    try {
      const response = await api.get(`/tasks/${taskId}/comments`);
      if (response.data.success) {
        return { success: true, data: response.data.data };
      }
      return { success: false, message: response.data.message };
    } catch (error) {
      return { success: false, message: error.message };
    }
  },

  createComment: async (taskId, content) => {
    try {
      const response = await api.post(`/tasks/${taskId}/comments`, {
        taskId,
        content
      });
      if (response.data.success) {
        return { success: true, data: response.data.data };
      }
      return { success: false, message: response.data.message };
    } catch (error) {
      return { success: false, message: error.message };
    }
  },

  updateComment: async (taskId, commentId, content) => {
    try {
      const response = await api.put(`/tasks/${taskId}/comments/${commentId}`, {
        content
      });
      if (response.data.success) {
        return { success: true, data: response.data.data };
      }
      return { success: false, message: response.data.message };
    } catch (error) {
      return { success: false, message: error.message };
    }
  },

  deleteComment: async (taskId, commentId) => {
    try {
      const response = await api.delete(`/tasks/${taskId}/comments/${commentId}`);
      if (response.data.success) {
        return { success: true };
      }
      return { success: false, message: response.data.message };
    } catch (error) {
      return { success: false, message: error.message };
    }
  }
};
