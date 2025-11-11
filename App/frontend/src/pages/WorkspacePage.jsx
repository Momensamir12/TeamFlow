import React, { useState, useEffect } from 'react';
import { ArrowLeft, Folder, Users, Settings, Archive } from 'lucide-react';
import { getWorkspaceById, getMyWorkspaceRole } from '../api/workspaceApi';
import { getWorkspaceProjects } from '../api/projectApi';
import { getWorkspaceRoleName, canCreateProject, canManageWorkspace } from '../constants/config';
import ProjectsTab from '../components/projects/ProjectsTab';
import MembersTab from '../components/workspaces/MembersTab';
import ProjectPage from './ProjectPage';

function WorkspacePage({ workspace, onBack }) {
  const [workspaceData, setWorkspaceData] = useState(workspace);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('projects');
  const [userRole, setUserRole] = useState(null);
  const [selectedProject, setSelectedProject] = useState(null);

  useEffect(() => {
    if (workspace?.id) {
      loadWorkspaceDetails();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [workspace?.id]);

    const loadWorkspaceDetails = async () => {
    if (!workspace?.id) return;
    
    setLoading(true);
    try {
      // Fetch workspace details and user role in parallel
      const [workspaceResult, roleResult] = await Promise.all([
        getWorkspaceById(workspace.id),
        getMyWorkspaceRole(workspace.id)
      ]);

      console.log('Workspace API Result:', workspaceResult);
      console.log('Role API Result:', roleResult);
      
      if (workspaceResult.success) {
        setWorkspaceData(workspaceResult.data);
      }

      if (roleResult.success) {
        const role = roleResult.data;
        console.log('Setting User Role from API:', role);
        setUserRole(role);
      } else {
        console.warn('Failed to fetch user role:', roleResult.message);
        setUserRole(0); // Default to Viewer
      }
    } catch (error) {
      console.error('Error loading workspace:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleProjectSelect = (project) => {
    setSelectedProject(project);
  };

  const handleBackToWorkspace = () => {
    setSelectedProject(null);
    loadWorkspaceDetails(); // Refresh workspace data
  };

  // If a project is selected, show ProjectPage
  if (selectedProject) {
    return (
      <ProjectPage 
        project={selectedProject}
        onBack={handleBackToWorkspace}
      />
    );
  }

  if (!workspace) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <p className="text-gray-600">No workspace selected</p>
          <button
            onClick={onBack}
            className="mt-4 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
          >
            Back to Workspaces
          </button>
        </div>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  if (!workspaceData) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <p className="text-red-600 mb-4">Failed to load workspace</p>
          <button
            onClick={onBack}
            className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
          >
            Back to Workspaces
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white shadow-sm border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-6 py-4">
          <div className="flex items-center gap-4 mb-4">
            <button
              onClick={onBack}
              className="flex items-center gap-2 text-gray-600 hover:text-gray-900 transition-colors"
            >
              <ArrowLeft size={20} />
              Back to Workspaces
            </button>
          </div>

          <div className="flex items-start justify-between">
            <div>
              <div className="flex items-center gap-3 mb-2">
                <h1 className="text-3xl font-bold text-gray-900">
                  {workspaceData.name}
                </h1>
                {workspaceData.isArchived && (
                  <span className="flex items-center gap-1 bg-yellow-100 text-yellow-700 px-3 py-1 text-sm font-medium rounded">
                    <Archive size={14} />
                    Archived
                  </span>
                )}
              </div>
              
              {workspaceData.description && (
                <p className="text-gray-600 mb-2">{workspaceData.description}</p>
              )}
              
              <div className="flex items-center gap-4 text-sm text-gray-500">
                <span className="flex items-center gap-1">
                  <Users size={16} />
                  {workspaceData.members?.length || 0} members
                </span>
                {userRole !== null && (
                  <span className="px-2 py-1 bg-indigo-100 text-indigo-700 rounded text-xs font-medium">
                    {getWorkspaceRoleName(userRole)}
                  </span>
                )}
              </div>
            </div>

            {canManageWorkspace(userRole) && (
              <button className="flex items-center gap-2 px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
                <Settings size={20} />
                Settings
              </button>
            )}
          </div>
        </div>

        {/* Tabs */}
        <div className="max-w-7xl mx-auto px-6">
          <div className="flex gap-4 border-b border-gray-200">
            <button
              onClick={() => setActiveTab('projects')}
              className={`flex items-center gap-2 px-4 py-3 border-b-2 font-medium transition-colors ${
                activeTab === 'projects'
                  ? 'border-indigo-600 text-indigo-600'
                  : 'border-transparent text-gray-600 hover:text-gray-900'
              }`}
            >
              <Folder size={20} />
              Projects
            </button>
            <button
              onClick={() => setActiveTab('members')}
              className={`flex items-center gap-2 px-4 py-3 border-b-2 font-medium transition-colors ${
                activeTab === 'members'
                  ? 'border-indigo-600 text-indigo-600'
                  : 'border-transparent text-gray-600 hover:text-gray-900'
              }`}
            >
              <Users size={20} />
              Members
            </button>
          </div>
        </div>
      </div>

      {/* Tab Content */}
      <main className="max-w-7xl mx-auto px-6 py-8">
        {activeTab === 'projects' ? (
          <ProjectsTab 
            workspaceId={workspaceData.id}
            userRole={userRole}
            onProjectUpdated={loadWorkspaceDetails}
            onProjectClick={handleProjectSelect}
          />
        ) : (
          <MembersTab
            workspaceId={workspaceData.id}
            members={workspaceData.members || []}
            userRole={userRole}
            onMembersUpdated={loadWorkspaceDetails}
          />
        )}
      </main>
    </div>
  );
}

export default WorkspacePage;