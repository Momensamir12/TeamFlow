import api from './axiosConfig';

// Create new project
export const createProject = async (projectData) => {
  try {
    const response = await api.post('/projects', projectData);
    return response.data;
  } catch (error) {
    console.error('Error creating project:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to create project'
    };
  }
};

// Update project
export const updateProject = async (projectData) => {
  try {
    const response = await api.put('/projects', projectData);
    return response.data;
  } catch (error) {
    console.error('Error updating project:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to update project'
    };
  }
};

// Get project details
export const getProjectDetails = async (projectId) => {
  try {
    const response = await api.get(`/projects/${projectId}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching project:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to fetch project'
    };
  }
};

// Get workspace projects
export const getWorkspaceProjects = async (workspaceId) => {
  try {
    const response = await api.get(`/projects/workspace/${workspaceId}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching projects:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to fetch projects'
    };
  }
};

// Add member to project
export const addProjectMember = async (memberData) => {
  try {
    const response = await api.post('/projects/members', memberData);
    return response.data;
  } catch (error) {
    console.error('Error adding member:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to add member'
    };
  }
};

// Remove member from project
export const removeProjectMember = async (projectId, memberUserId) => {
  try {
    const response = await api.delete(`/projects/${projectId}/members/${memberUserId}`);
    return response.data;
  } catch (error) {
    console.error('Error removing member:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to remove member'
    };
  }
};