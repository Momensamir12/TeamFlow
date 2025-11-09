export const API_BASE_URL = 'http://localhost:5000/api';

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

// Helper functions to display names
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

