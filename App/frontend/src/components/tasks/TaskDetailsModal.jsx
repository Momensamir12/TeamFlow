import React, { useState } from 'react';
import { X, Calendar, User, Trash2, AlertCircle, ChevronDown } from 'lucide-react';
import { updateTask, deleteTask } from '../../api/taskApi';
import { TASK_STATUS, TASK_PRIORITY, getStatusName, getPriorityName, getStatusColor, getPriorityColor } from '../../constants/config';

function TaskDetailsModal({ task, onClose, onTaskUpdated, canEdit, canDelete, projectMembers }) {
  const [formData, setFormData] = useState({
    status: task.status,
    priority: task.priority
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [hasChanges, setHasChanges] = useState(false);
  const [showStatusDropdown, setShowStatusDropdown] = useState(false);
  const [showPriorityDropdown, setShowPriorityDropdown] = useState(false);

  const getAssigneeName = () => {
    if (!task.assigneeId) return 'Unassigned';
    // If projectMembers is empty array, it means this is "My Tasks" view
    if (projectMembers && projectMembers.length === 0) return 'Me';
    const assignee = projectMembers?.find(m => m.userId === task.assigneeId);
    return assignee ? assignee.userName : 'Unknown User';
  };

  const formatDate = (dateString) => {
    if (!dateString) return 'No deadline set';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      month: 'long', 
      day: 'numeric', 
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const handleSave = async () => {
    setError('');
    setLoading(true);

    try {
      const result = await updateTask({
        id: task.id,
        title: task.title,
        description: task.description,
        status: formData.status,
        priority: formData.priority,
        deadline: task.deadline,
        projectId: task.projectId,
        assigneeId: task.assigneeId
      });

      if (result.success) {
        setHasChanges(false);
        onTaskUpdated();
      } else {
        setError(result.message || 'Failed to update task');
      }
    } catch {
      setError('An error occurred while updating the task');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!window.confirm('Are you sure you want to delete this task? This action cannot be undone.')) {
      return;
    }

    setError('');
    setLoading(true);

    try {
      const result = await deleteTask(task.id);

      if (result.success) {
        onTaskUpdated();
        onClose();
      } else {
        setError(result.message || 'Failed to delete task');
      }
    } catch {
      setError('An error occurred while deleting the task');
    } finally {
      setLoading(false);
    }
  };

  const handleStatusChange = (newStatus) => {
    setFormData({ ...formData, status: parseInt(newStatus) });
    setHasChanges(true);
    setShowStatusDropdown(false);
  };

  const handlePriorityChange = (newPriority) => {
    setFormData({ ...formData, priority: parseInt(newPriority) });
    setHasChanges(true);
    setShowPriorityDropdown(false);
  };

  const statusOptions = [
    { value: TASK_STATUS.TODO, label: 'To Do' },
    { value: TASK_STATUS.IN_PROGRESS, label: 'In Progress' },
    { value: TASK_STATUS.DONE, label: 'Done' },
    { value: TASK_STATUS.BLOCKED, label: 'Blocked' }
  ];

  const priorityOptions = [
    { value: TASK_PRIORITY.LOW, label: 'Low' },
    { value: TASK_PRIORITY.MEDIUM, label: 'Medium' },
    { value: TASK_PRIORITY.HIGH, label: 'High' },
    { value: TASK_PRIORITY.URGENT, label: 'Urgent' }
  ];

  return (
    <div 
      className="fixed inset-0 bg-gray-900 bg-opacity-50 flex items-center justify-center p-4"
      style={{ zIndex: 9999 }}
      onClick={onClose}
    >
      <div 
        className="bg-white rounded-lg shadow-2xl max-w-2xl w-full relative"
        style={{ zIndex: 10000 }}
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <h2 className="text-2xl font-bold text-gray-900">Task Details</h2>
          <button 
            onClick={onClose} 
            className="text-gray-400 hover:text-gray-600 transition-colors"
          >
            <X size={24} />
          </button>
        </div>

        {/* Content - wrapped with max-height and overflow */}
        <div style={{ maxHeight: 'calc(90vh - 200px)', overflowY: 'auto' }}>
          <div className="p-6 space-y-6">
            {error && (
              <div className="bg-red-50 border border-red-200 rounded-lg p-3">
                <div className="flex items-center gap-2 text-red-800">
                  <AlertCircle size={20} />
                  <span className="text-sm font-medium">{error}</span>
                </div>
              </div>
            )}

            {/* Title */}
            <div>
              <h3 className="text-xl font-semibold text-gray-900 mb-2">
                {task.title}
              </h3>
              {task.description && (
                <p className="text-gray-600">{task.description}</p>
              )}
            </div>

            {/* Status and Priority */}
            <div className="grid grid-cols-2 gap-4">
              <div className="relative">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Status
                </label>
                {canEdit ? (
                  <div className="relative">
                    <button
                      type="button"
                      onClick={() => setShowStatusDropdown(!showStatusDropdown)}
                      disabled={loading}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent bg-white text-left flex items-center justify-between disabled:opacity-50"
                    >
                      <span>{getStatusName(formData.status)}</span>
                      <ChevronDown size={16} />
                    </button>
                    {showStatusDropdown && (
                      <div className="absolute z-50 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg">
                        {statusOptions.map(option => (
                          <button
                            key={option.value}
                            type="button"
                            onClick={() => handleStatusChange(option.value)}
                            className="w-full px-4 py-2 text-left hover:bg-indigo-50 first:rounded-t-lg last:rounded-b-lg"
                          >
                            {option.label}
                          </button>
                        ))}
                      </div>
                    )}
                  </div>
                ) : (
                  <span className={`inline-flex px-3 py-1.5 text-sm font-medium rounded border ${getStatusColor(task.status)}`}>
                    {getStatusName(task.status)}
                  </span>
                )}
              </div>

              <div className="relative">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Priority
                </label>
                {canEdit ? (
                  <div className="relative">
                    <button
                      type="button"
                      onClick={() => setShowPriorityDropdown(!showPriorityDropdown)}
                      disabled={loading}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent bg-white text-left flex items-center justify-between disabled:opacity-50"
                    >
                      <span>{getPriorityName(formData.priority)}</span>
                      <ChevronDown size={16} />
                    </button>
                    {showPriorityDropdown && (
                      <div className="absolute z-50 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg">
                        {priorityOptions.map(option => (
                          <button
                            key={option.value}
                            type="button"
                            onClick={() => handlePriorityChange(option.value)}
                            className="w-full px-4 py-2 text-left hover:bg-indigo-50 first:rounded-t-lg last:rounded-b-lg"
                          >
                            {option.label}
                          </button>
                        ))}
                      </div>
                    )}
                  </div>
                ) : (
                  <span className={`inline-flex px-3 py-1.5 text-sm font-medium rounded-full ${getPriorityColor(task.priority)}`}>
                    {getPriorityName(task.priority)}
                  </span>
                )}
              </div>
            </div>

            {/* Deadline */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2 flex items-center gap-2">
                <Calendar size={16} />
                Deadline
              </label>
              <p className="text-gray-900">{formatDate(task.deadline)}</p>
            </div>

            {/* Assignee */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2 flex items-center gap-2">
                <User size={16} />
                Assigned To
              </label>
              <p className="text-gray-900">{getAssigneeName()}</p>
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="flex items-center justify-between p-6 border-t border-gray-200 bg-gray-50">
          <div>
            {canDelete && (
              <button
                onClick={handleDelete}
                disabled={loading}
                className="flex items-center gap-2 px-4 py-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors font-medium disabled:opacity-50"
              >
                <Trash2 size={18} />
                Delete Task
              </button>
            )}
          </div>
          
          <div className="flex gap-3">
            {canEdit && hasChanges && (
              <button
                onClick={handleSave}
                disabled={loading}
                className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors font-medium disabled:opacity-50"
              >
                {loading ? 'Saving...' : 'Save'}
              </button>
            )}
            <button
              onClick={onClose}
              disabled={loading}
              className="px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50"
            >
              Close
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

export default TaskDetailsModal;
