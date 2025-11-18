export const API_BASE_URL = 'http://localhost:5000/api';
export const SIGNAlR_URL = 'http://localhost:5000/notificationsHub'

// If backend uses integers (0, 1, 2, 3):
export const TASK_STATUS = {
  TODO: 0,
  IN_PROGRESS: 1,
  DONE: 2,
  BLOCKED: 3
};

export const TASK_PRIORITY = {
  LOW: 0,
  MEDIUM: 1,
  HIGH: 2,
  URGENT: 3
};

export const WORKSPACE_ROLE = {
  VIEWER: 0,
  MEMBER: 1,
  ADMIN: 2
};

export const PROJECT_ROLE = {
  VIEWER: 0,
  MEMBER: 1,
  ADMIN: 2
};

// Helper functions for task status/priority
export const getStatusName = (status) => {
  const statusNames = {
    0: 'To Do',
    1: 'In Progress',
    2: 'Done',
    3: 'Blocked'
  };
  return statusNames[status] ?? 'Unknown';
};

export const getPriorityName = (priority) => {
  const priorityNames = {
    0: 'Low',
    1: 'Medium',
    2: 'High',
    3: 'Urgent'
  };
  return priorityNames[priority] ?? 'Unknown';
};

export const getStatusColor = (status) => {
  const colors = {
    0: 'bg-gray-100 text-gray-700 border-gray-300',
    1: 'bg-blue-100 text-blue-700 border-blue-300',
    2: 'bg-green-100 text-green-700 border-green-300',
    3: 'bg-red-100 text-red-700 border-red-300'
  };
  return colors[status] ?? 'bg-gray-100 text-gray-700 border-gray-300';
};

export const getPriorityColor = (priority) => {
  const colors = {
    0: 'bg-gray-100 text-gray-600',
    1: 'bg-yellow-100 text-yellow-700',
    2: 'bg-orange-100 text-orange-700',
    3: 'bg-red-100 text-red-700'
  };
  return colors[priority] ?? 'bg-gray-100 text-gray-600';
};

// Helper functions for roles
export const getWorkspaceRoleName = (role) => {
  const roleNames = {
    0: 'Viewer',
    1: 'Member',
    2: 'Admin'
  };
  return roleNames[role] ?? 'Unknown';
};

export const getProjectRoleName = (role) => {
  const roleNames = {
    0: 'Viewer',
    1: 'Member',
    2: 'Admin'
  };
  return roleNames[role] ?? 'Unknown';
};

export const getRoleColor = (role) => {
  const colors = {
    0: 'bg-gray-100 text-gray-600',
    1: 'bg-blue-100 text-blue-600',
    2: 'bg-purple-100 text-purple-600'
  };
  return colors[role] ?? 'bg-gray-100 text-gray-600';
};

// Permission helpers
export const canCreateProject = (workspaceRole) => {
  return workspaceRole >= WORKSPACE_ROLE.MEMBER;
};

export const canManageWorkspace = (workspaceRole) => {
  return workspaceRole >= WORKSPACE_ROLE.ADMIN;
};

export const canEditProject = (projectRole) => {
  return projectRole >= PROJECT_ROLE.MEMBER;
};

export const canManageProject = (projectRole) => {
  return projectRole >= PROJECT_ROLE.ADMIN;
};

