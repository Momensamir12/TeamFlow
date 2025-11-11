import React from 'react';
import { Folder, Calendar, Users, Archive } from 'lucide-react';

function ProjectCard({ project, onClick }) {
  const formatDate = (dateString) => {
    if (!dateString) return 'Unknown date';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      month: 'short', 
      day: 'numeric', 
      year: 'numeric' 
    });
  };

  const truncateDescription = (text, maxLength = 80) => {
    if (!text) return '';
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength).trim() + '...';
  };

  return (
    <div 
      onClick={onClick}
      className="bg-white rounded-lg shadow-sm border border-gray-200 p-5 hover:shadow-md hover:border-indigo-300 transition-all cursor-pointer"
    >
      {/* Header */}
      <div className="flex items-start justify-between mb-3">
        <div className="flex items-center gap-3 flex-1">
          <div className="p-2 bg-blue-100 rounded-lg">
            <Folder size={20} className="text-blue-600" />
          </div>
          <h3 className="text-lg font-semibold text-gray-900 line-clamp-1">
            {project.name}
          </h3>
        </div>
        
        {project.isArchived && (
          <span className="flex items-center gap-1 bg-yellow-100 text-yellow-700 px-2 py-1 text-xs font-medium rounded">
            <Archive size={12} />
            Archived
          </span>
        )}
      </div>

      {/* Description */}
      {project.description && (
        <p className="text-gray-600 text-sm mb-3 line-clamp-2">
          {truncateDescription(project.description)}
        </p>
      )}

      {/* Metadata */}
      <div className="flex items-center justify-between text-sm text-gray-500 mt-4 pt-3 border-t border-gray-100">
        <div className="flex items-center gap-1">
          <Users size={14} />
          <span>{project.memberCount || 0} members</span>
        </div>
        <div className="flex items-center gap-1">
          <Calendar size={14} />
          <span>{formatDate(project.createdAt)}</span>
        </div>
      </div>
    </div>
  );
}

export default ProjectCard;
