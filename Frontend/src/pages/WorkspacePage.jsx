import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, Folder, Users, Archive } from 'lucide-react';
import { getWorkspaceById, getMyWorkspaceRole } from '../api/workspaceApi';
import { getWorkspaceProjects } from '../api/projectApi';
import { getWorkspaceRoleName, canCreateProject, canManageWorkspace } from '../constants/config';
import ProjectsTab from '../components/projects/ProjectsTab';
import MembersTab from '../components/workspaces/MembersTab';
import ProjectPage from './ProjectPage';
import Layout from '../components/common/Layout';

function WorkspacePage({ workspace, onBack, user, onLogout, onOpenProfile, onOpenPassword }) {
  const navigate = useNavigate();
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
      
      if (workspaceResult.success) {
        setWorkspaceData(workspaceResult.data);
      }

      if (roleResult.success) {
        const role = roleResult.data;
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

  const handleSidebarTabChange = (tab) => {
    if (tab === 'tasks') {
      // Navigate back to dashboard with tasks tab, clearing workspace state
      navigate('/dashboard', { state: { activeTab: 'tasks' } });
    } else if (tab === 'workspaces') {
      // Navigate back to dashboard with workspaces tab, clearing workspace state
      navigate('/dashboard', { state: { activeTab: 'workspaces' } });
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
        user={user}
        onLogout={onLogout}
        onOpenProfile={onOpenProfile}
        onOpenPassword={onOpenPassword}
      />
    );
  }

  if (!workspace) {
    return (
      <Layout
        activeTab="workspaces"
        onTabChange={handleSidebarTabChange}
        onLogout={onLogout}
        user={user}
        onOpenProfile={onOpenProfile}
        onOpenPassword={onOpenPassword}
      >
        <div className="flex items-center justify-center min-h-96">
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
      </Layout>
    );
  }

  if (loading) {
    return (
      <Layout
        activeTab="workspaces"
        onTabChange={handleSidebarTabChange}
        onLogout={onLogout}
        user={user}
        onOpenProfile={onOpenProfile}
        onOpenPassword={onOpenPassword}
      >
        <div className="flex items-center justify-center min-h-96">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
        </div>
      </Layout>
    );
  }

  if (!workspaceData) {
    return (
      <Layout
        activeTab="workspaces"
        onTabChange={handleSidebarTabChange}
        onLogout={onLogout}
        user={user}
        onOpenProfile={onOpenProfile}
        onOpenPassword={onOpenPassword}
      >
        <div className="flex items-center justify-center min-h-96">
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
      </Layout>
    );
  }

  return (
    <Layout
      activeTab="workspaces"
      onTabChange={handleSidebarTabChange}
      onLogout={onLogout}
      user={user}
      onOpenProfile={onOpenProfile}
      onOpenPassword={onOpenPassword}
    >
      {/* Header */}
      <div className="bg-white shadow-sm border-b border-gray-200 -mx-6 lg:-mx-8 -mt-6 lg:-mt-8 px-6 lg:px-8 pt-6 lg:pt-8">
        <div className="max-w-7xl mx-auto">
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
      <div className="max-w-7xl mx-auto py-8">
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
      </div>
    </Layout>
  );
}

export default WorkspacePage;