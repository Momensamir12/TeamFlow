import React from 'react';
import { Calendar, Clock, AlertCircle, CheckCircle, Circle, Loader, Edit2 } from 'lucide-react';
import { getStatusName, getPriorityName, getStatusColor, getPriorityColor, TASK_STATUS } from '../../constants/config';

function TaskCard({ task, onClick, onEdit, onUpdate, canEdit }) {
  const getStatusIcon = (status) => {
    switch (status) {
      case TASK_STATUS.TODO:
        return <Circle size={16} className="text-gray-500" />;
      case TASK_STATUS.IN_PROGRESS:
        return <Loader size={16} className="text-blue-500" />;
      case TASK_STATUS.DONE:
        return <CheckCircle size={16} className="text-green-500" />;
      case TASK_STATUS.BLOCKED:
        return <AlertCircle size={16} className="text-red-500" />;
      default:
        return <Circle size={16} className="text-gray-500" />;
    }
  };

  const formatDate = (dateString) => {
    if (!dateString) return 'No deadline';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      month: 'short', 
      day: 'numeric', 
      year: 'numeric' 
    });
  };

  return (
    <div 
      className="bg-white rounded-lg shadow-sm border border-gray-200 p-3 hover:shadow-md hover:border-indigo-300 transition-all cursor-pointer"
      onClick={onClick}
    >
      <div className="flex items-center gap-3">
        {/* Status Icon */}
        <div className="flex-shrink-0">
          {getStatusIcon(task.status)}
        </div>
        
        {/* Main Content */}
        <div className="flex-1 min-w-0">
          <h3 className="text-sm font-medium text-gray-900 truncate">
            {task.title}
          </h3>
          {task.description && (
            <p className="text-xs text-gray-600 mt-1 truncate">
              {task.description}
            </p>
          )}
        </div>
        
        {/* Right Side - Due Date */}
        <div className="flex-shrink-0 flex items-center gap-2">
          <div className="text-xs text-gray-500">
            {formatDate(task.deadline)}
          </div>
          
          {/* Priority Badge */}
          <span className={`px-2 py-1 text-xs font-medium rounded ${getPriorityColor(task.priority)}`}>
            {getPriorityName(task.priority)}
          </span>
          
          {/* Edit Button */}
          {canEdit && onEdit && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                onEdit(task);
              }}
              className="p-1 text-gray-400 hover:bg-indigo-50 hover:text-indigo-600 rounded transition-colors"
              title="Edit task"
            >
              <Edit2 size={14} />
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

export default TaskCard;