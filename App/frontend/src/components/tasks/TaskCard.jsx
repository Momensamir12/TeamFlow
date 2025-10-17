import React from 'react';
import { CheckCircle, Clock, AlertCircle, Calendar } from 'lucide-react';

function TaskCard({ task }) {
  const getStatusIcon = (status) => {
    switch(status?.toLowerCase()) {
      case 'done':
        return <CheckCircle className="text-green-500" size={20} />;
      case 'inprogress':
        return <Clock className="text-blue-500" size={20} />;
      case 'blocked':
        return <AlertCircle className="text-red-500" size={20} />;
      default: // Todo
        return <AlertCircle className="text-yellow-500" size={20} />;
    }
  };

  const getPriorityColor = (priority) => {
    switch(priority?.toLowerCase()) {
      case 'urgent':
        return 'bg-purple-100 text-purple-700';
      case 'high':
        return 'bg-red-100 text-red-700';
      case 'medium':
        return 'bg-yellow-100 text-yellow-700';
      case 'low':
        return 'bg-green-100 text-green-700';
      default:
        return 'bg-gray-100 text-gray-700';
    }
  };

  return (
    <div className="bg-white rounded-lg shadow p-4 hover:shadow-md transition-shadow">
      <div className="flex items-start justify-between mb-2">
        <div className="flex items-center gap-2">
          {getStatusIcon(task.status)}
          <h3 className="font-semibold text-gray-900">{task.title}</h3>
        </div>
        <span className={`px-2 py-1 rounded text-xs font-medium ${getPriorityColor(task.priority)}`}>
          {task.priority}
        </span>
      </div>
      
      <p className="text-gray-600 text-sm mb-3">{task.description}</p>
      
      {task.deadline && (
        <div className="flex items-center gap-1 text-sm text-gray-500">
          <Calendar size={16} />
          <span>{new Date(task.deadline).toLocaleDateString()}</span>
        </div>
      )}
    </div>
  );
}

export default TaskCard;