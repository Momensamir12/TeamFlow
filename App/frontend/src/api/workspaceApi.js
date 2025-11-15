import api from './axiosConfig';

// Get all user's workspaces
export const getUserWorkspaces = async () => {
  try {
    const response = await api.get('/workspaces');
    return response.data;
  } catch (error) {
    console.error('Error fetching workspaces:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to fetch workspaces'
    };
  }
};

// Get single workspace by ID
export const getWorkspaceById = async (workspaceId) => {
  try {
    const response = await api.get(`/workspaces/${workspaceId}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching workspace:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to fetch workspace'
    };
  }
};

// Create new workspace
export const createWorkspace = async (workspaceData) => {
  try {
    const response = await api.post('/workspaces', workspaceData);
    return response.data;
  } catch (error) {
    console.error('Error creating workspace:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to create workspace'
    };
  }
};

// Update workspace
export const updateWorkspace = async (workspaceData) => {
  try {
    const response = await api.put('/workspaces', workspaceData);
    return response.data;
  } catch (error) {
    console.error('Error updating workspace:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to update workspace'
    };
  }
};

// Archive workspace
export const archiveWorkspace = async (workspaceId) => {
  try {
    const response = await api.post(`/workspaces/${workspaceId}/archive`);
    return response.data;
  } catch (error) {
    console.error('Error archiving workspace:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to archive workspace'
    };
  }
};

// Unarchive workspace
export const unarchiveWorkspace = async (workspaceId) => {
  try {
    const response = await api.post(`/workspaces/${workspaceId}/unarchive`);
    return response.data;
  } catch (error) {
    console.error('Error unarchiving workspace:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to unarchive workspace'
    };
  }
};

// Join workspace with code
export const joinWorkspace = async (code) => {
  try {
    const response = await api.post(`/workspaces/join?code=${code}`);
    return response.data;
  } catch (error) {
    console.error('Error joining workspace:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to join workspace'
    };
  }
};

// Get workspace invite code
export const getWorkspaceCode = async (workspaceId) => {
  try {
    const response = await api.get(`/workspaces/${workspaceId}/code`);
    return response.data;
  } catch (error) {
    console.error('Error fetching workspace code:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to fetch invite code'
    };
  }
};

// Regenerate workspace invite code
export const regenerateWorkspaceCode = async (workspaceId) => {
  try {
    const response = await api.post(`/workspaces/${workspaceId}/code/regenerate`);
    return response.data;
  } catch (error) {
    console.error('Error regenerating code:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to regenerate code'
    };
  }
};

// Remove member from workspace
export const removeMember = async (workspaceId, memberUserId) => {
  try {
    const response = await api.delete(`/workspaces/${workspaceId}/members/${memberUserId}`);
    return response.data;
  } catch (error) {
    console.error('Error removing member:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to remove member'
    };
  }
};

// Update member role
export const updateMemberRole = async (workspaceId, memberUserId, role) => {
  try {
    const response = await api.put(`/workspaces/${workspaceId}/members/${memberUserId}/role`, { role });
    return response.data;
  } catch (error) {
    console.error('Error updating member role:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to update member role'
    };
  }
};

// Get current user's role in workspace
export const getMyWorkspaceRole = async (workspaceId) => {
  try {
    const response = await api.get(`/workspaces/${workspaceId}/my-role`);
    return response.data;
  } catch (error) {
    console.error('Error fetching workspace role:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to fetch workspace role'
    };
  }
};

// Invitation APIs
export const sendInvitationEmail = async (workspaceId, email, role) => {
  try {
    const response = await api.post('/invitations/send', {
      workspaceId,
      email,
      role
    });
    return response.data;
  } catch (error) {
    console.error('Error sending invitation:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to send invitation'
    };
  }
};

export const validateInvitation = async (token) => {
  try {
    const response = await api.get(`/invitations/validate?token=${token}`);
    return response.data;
  } catch (error) {
    console.error('Error validating invitation:', error);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to validate invitation'
    };
  }
};

export const acceptInvitation = async (token) => {
  try {
    const response = await api.post('/invitations/accept', { token });
    return response.data;
  } catch (error) {
    console.error('Error accepting invitation:', error);
    console.error('Error response:', error.response);
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to accept invitation'
    };
  }
};