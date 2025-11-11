import React, { useState, useEffect } from 'react';
import { Plus, Folder, Loader, AlertCircle } from 'lucide-react';
import { getWorkspaceProjects } from '../../api/projectApi';
import { canCreateProject } from '../../constants/config';
import ProjectCard from './ProjectCard';
import CreateProjectModal from './CreateProjectModal';

function ProjectsTab({ workspaceId, userRole, onProjectUpdated, onProjectClick }) {
  const [projects, setProjects] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showCreateModal, setShowCreateModal] = useState(false);

  useEffect(() => {
    loadProjects();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [workspaceId]);

  const loadProjects = async () => {
    setLoading(true);
    setError('');
    
    try {
      const result = await getWorkspaceProjects(workspaceId);
      
      if (result.success) {
        setProjects(result.data || []);
      } else {
        setError(result.message || 'Failed to load projects');
      }
    } catch (err) {
      setError('An error occurred while loading projects');
      console.error('Error loading projects:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleProjectClick = (project) => {
    console.log('Project clicked:', project);
    if (onProjectClick) {
      onProjectClick(project);
    }
  };

  const handleProjectCreated = () => {
    loadProjects();
    onProjectUpdated();
  };

  const canCreate = canCreateProject(userRole);
  console.log('ProjectsTab Debug:', {
    userRole,
    canCreate,
    showCreateModal,
    projectsCount: projects.length
  });

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader size={40} className="animate-spin text-indigo-600" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-lg p-4">
        <div className="flex items-center gap-2 text-red-800">
          <AlertCircle size={20} />
          <span className="font-medium">{error}</span>
        </div>
        <button
          onClick={loadProjects}
          className="mt-2 text-sm text-red-700 hover:text-red-900 underline"
        >
          Try again
        </button>
      </div>
    );
  }

  return (
    <div>
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Projects</h2>
          <p className="text-gray-600 mt-1">
            {projects.length} project{projects.length !== 1 ? 's' : ''}
          </p>
        </div>
        
        {canCreateProject(userRole) && (
          <button
            onClick={() => {
              console.log('New Project button clicked');
              setShowCreateModal(true);
            }}
            className="flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
          >
            <Plus size={20} />
            New Project
          </button>
        )}
      </div>

      {/* Projects Grid */}
      {projects.length === 0 ? (
        <div className="text-center py-12 bg-white rounded-lg shadow">
          <Folder size={48} className="mx-auto text-gray-400 mb-4" />
          <h3 className="text-lg font-semibold text-gray-900 mb-2">
            No projects yet
          </h3>
          <p className="text-gray-600 mb-4">
            {canCreateProject(userRole)
              ? 'Create your first project to get started'
              : 'No projects have been created in this workspace yet'
            }
          </p>
          {canCreateProject(userRole) && (
            <button
              onClick={() => setShowCreateModal(true)}
              className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
            >
              Create Project
            </button>
          )}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {projects.map(project => (
            <ProjectCard
              key={project.id}
              project={project}
              onClick={() => handleProjectClick(project)}
            />
          ))}
        </div>
      )}

      {/* Create Project Modal */}
      {showCreateModal && (
        <CreateProjectModal
          workspaceId={workspaceId}
          onClose={() => setShowCreateModal(false)}
          onProjectCreated={handleProjectCreated}
        />
      )}
    </div>
  );
}

export default ProjectsTab;