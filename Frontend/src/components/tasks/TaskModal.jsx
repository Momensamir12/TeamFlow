import React, { useState } from 'react';
import { X, Calendar, User, Folder, Clock, AlertCircle } from 'lucide-react';
import { TASK_STATUS, getStatusName, getPriorityName, getStatusColor, getPriorityColor } from '../../constants/config';
import { updateTaskStatus } from '../../api/taskApi';

function TaskModal({ task, onClose, onTaskUpdated }) {
  const [isUpdating, setIsUpdating] = useState(false);
  const [error, setError] = useState('');

  const formatDate = (dateString) => {
    if (!dateString) return 'No deadline set';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      weekday: 'long',
      year: 'numeric',
      month: 'long', 
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const handleStatusChange = async (newStatus) => {
    setIsUpdating(true);
    setError('');
    
    try {
      const result = await updateTaskStatus(task.id, newStatus);
      
      if (result.success) {
        onTaskUpdated();
        onClose();
      } else {
        setError(result.message || 'Failed to update status');
      }
    } catch (err) {
      setError('Failed to update task status');
      console.error('Error updating status:', err);
    } finally {
      setIsUpdating(false);
    }
  };

  const StatusButton = ({ status, label }) => {
    const isActive = task.status === status;
    const colorClass = isActive ? getStatusColor(status) : 'bg-gray-100 text-gray-600 border-gray-300';
    
    return (
      <button
        onClick={() => handleStatusChange(status)}
        disabled={isUpdating || isActive}
        className={`px-4 py-2 rounded-lg border font-medium transition-colors ${colorClass} ${
          isActive ? 'cursor-default' : 'hover:opacity-80'
        } ${isUpdating ? 'opacity-50 cursor-not-allowed' : ''}`}
      >
        {label}
      </button>
    );
  };

  return (
    <div 
      className="fixed inset-0 backdrop-blur-md flex items-center justify-center z-50 p-4"
      onClick={onClose}
    >
      <div 
        className="bg-white rounded-lg shadow-2xl max-w-2xl w-full max-h-[90vh] overflow-y-auto"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="flex items-start justify-between p-6 border-b border-gray-200">
          <div className="flex-1">
            <h2 className="text-2xl font-bold text-gray-900 mb-2">
              {task.title}
            </h2>
            <div className="flex items-center gap-2">
              <span className={`px-3 py-1 text-sm font-medium rounded-full ${getPriorityColor(task.priority)}`}>
                {getPriorityName(task.priority)}
              </span>
              <span className={`px-3 py-1 text-sm font-medium rounded-lg border ${getStatusColor(task.status)}`}>
                {getStatusName(task.status)}
              </span>
            </div>
          </div>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors"
          >
            <X size={24} />
          </button>
        </div>

        {/* Content */}
        <div className="p-6 space-y-6">
          {/* Description */}
          {task.description && (
            <div>
              <h3 className="text-sm font-semibold text-gray-700 mb-2">Description</h3>
              <p className="text-gray-600 whitespace-pre-wrap">{task.description}</p>
            </div>
          )}

          {/* Details Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {/* Deadline */}
            <div className="flex items-start gap-3 p-4 bg-gray-50 rounded-lg">
              <Calendar size={20} className="text-gray-600 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-gray-700">Deadline</p>
                <p className="text-sm text-gray-600 mt-1">{formatDate(task.deadline)}</p>
              </div>
            </div>

            {/* Assignee */}
            <div className="flex items-start gap-3 p-4 bg-gray-50 rounded-lg">
              <User size={20} className="text-gray-600 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-gray-700">Assigned To</p>
                <p className="text-sm text-gray-600 mt-1">
                  {task.assignerUsername || 'Unassigned'}
                </p>
              </div>
            </div>

            {/* Project */}
            <div className="flex items-start gap-3 p-4 bg-gray-50 rounded-lg">
              <Folder size={20} className="text-gray-600 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-gray-700">Project</p>
                <p className="text-sm text-gray-600 mt-1">
                  {task.projectId ? `Project ID: ${task.projectId.substring(0, 8)}...` : 'No project'}
                </p>
              </div>
            </div>

            {/* Created */}
            <div className="flex items-start gap-3 p-4 bg-gray-50 rounded-lg">
              <Clock size={20} className="text-gray-600 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-gray-700">Created</p>
                <p className="text-sm text-gray-600 mt-1">
                  {new Date(task.createdAt).toLocaleDateString()}
                </p>
              </div>
            </div>
          </div>

          {/* Status Update Section */}
          <div>
            <h3 className="text-sm font-semibold text-gray-700 mb-3">Update Status</h3>
            {error && (
              <div className="mb-3 p-3 bg-red-50 text-red-700 rounded-lg flex items-center gap-2">
                <AlertCircle size={16} />
                <span className="text-sm">{error}</span>
              </div>
            )}
            <div className="grid grid-cols-2 md:grid-cols-4 gap-2">
              <StatusButton status={TASK_STATUS.TODO} label="To Do" />
              <StatusButton status={TASK_STATUS.IN_PROGRESS} label="In Progress" />
              <StatusButton status={TASK_STATUS.DONE} label="Done" />
              <StatusButton status={TASK_STATUS.BLOCKED} label="Blocked" />
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="flex justify-end gap-3 p-6 border-t border-gray-200">
          <button
            onClick={onClose}
            className="px-4 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  );
}

export default TaskModal;