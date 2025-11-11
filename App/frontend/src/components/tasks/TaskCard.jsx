import React from 'react';
import { Calendar, Clock, AlertCircle, CheckCircle, Circle, Loader } from 'lucide-react';
import { getStatusName, getPriorityName, getStatusColor, getPriorityColor, TASK_STATUS } from '../../constants/config';

function TaskCard({ task, onClick }) {
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
      onClick={onClick}
      className="bg-white rounded-lg shadow-sm border border-gray-200 p-4 hover:shadow-md hover:border-indigo-300 transition-all cursor-pointer"
    >
      <div className="flex items-start justify-between mb-3">
        <h3 className="text-lg font-semibold text-gray-900 flex-1">
          {task.title}
        </h3>
        <span className={`px-2 py-1 text-xs font-medium rounded-full ${getPriorityColor(task.priority)}`}>
          {getPriorityName(task.priority)}
        </span>
      </div>

      {task.description && (
        <p className="text-gray-600 text-sm mb-3 line-clamp-2">
          {task.description}
        </p>
      )}

      <div className="space-y-2 text-sm">
        <div className="flex items-center gap-2 text-gray-600">
          <Calendar size={16} />
          <span>{formatDate(task.deadline)}</span>
        </div>

        <div className="flex items-center gap-2">
          {getStatusIcon(task.status)}
          <span className={`px-2 py-1 text-xs font-medium rounded border ${getStatusColor(task.status)}`}>
            {getStatusName(task.status)}
          </span>
        </div>
      </div>
    </div>
  );
}

export default TaskCard;