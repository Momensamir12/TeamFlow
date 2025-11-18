import React from 'react';
import { Users, Calendar, Archive, Building2 } from 'lucide-react';

function WorkspaceCard({ workspace, onClick }) {
  
  const formatDate = (dateString) => {
    if (!dateString) return 'Unknown date';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      month: 'short', 
      day: 'numeric', 
      year: 'numeric' 
    });
  };

  const truncateDescription = (text, maxLength = 100) => {
    if (!text) return '';
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength).trim() + '...';
  };

  return (
    <div 
      onClick={onClick}
      className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 hover:shadow-md hover:border-indigo-300 transition-all cursor-pointer"
    >
      {/* Header with icon and name */}
      <div className="flex items-start justify-between mb-3">
        <div className="flex items-center gap-3 flex-1">
          <div className="p-2 bg-indigo-100 rounded-lg">
            <Building2 size={24} className="text-indigo-600" />
          </div>
          <h3 className="text-lg font-semibold text-gray-900">
            {workspace.name}
          </h3>
        </div>
        
        {/* Archive badge if workspace is archived */}
        {workspace.isArchived && (
          <span className="flex items-center gap-1 bg-yellow-100 text-yellow-700 px-2 py-1 text-xs font-medium rounded">
            <Archive size={14} />
            Archived
          </span>
        )}
      </div>

      {/* Description */}
      {workspace.description && (
        <p className="text-gray-600 text-sm mb-4">
          {truncateDescription(workspace.description)}
        </p>
      )}

      {/* Metadata row - member count and created date */}
      <div className="flex items-center gap-4 text-sm text-gray-500">
        <div className="flex items-center gap-1">
          <Users size={16} />
          <span>{workspace.memberCount || 0} member{workspace.memberCount !== 1 ? 's' : ''}</span>
        </div>
        <div className="flex items-center gap-1">
          <Calendar size={16} />
          <span>{formatDate(workspace.createdAt)}</span>
        </div>
      </div>
    </div>
  );
}

export default WorkspaceCard;