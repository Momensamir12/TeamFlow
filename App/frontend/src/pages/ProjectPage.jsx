import React, { useState, useEffect } from 'react';
import { ArrowLeft, Folder, Users, ListTodo, Settings } from 'lucide-react';
import { getProjectDetails, getMyProjectRole } from '../api/projectApi';
import { getWorkspaceById } from '../api/workspaceApi';
import { getProjectRoleName, canEditProject } from '../constants/config';
import ProjectMembersTab from '../components/projects/ProjectMembersTab';
import ProjectTasksTab from '../components/projects/ProjectTasksTab';

function ProjectPage({ project, onBack }) {
  const [projectData, setProjectData] = useState(project);
  const [workspaceMembers, setWorkspaceMembers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('tasks');
  const [userRole, setUserRole] = useState(null);

  useEffect(() => {
    if (project?.id) {
      loadProjectDetails();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [project?.id]);

  const loadProjectDetails = async () => {
    if (!project?.id) return;
    
    setLoading(true);
    try {
      // Fetch project details and user role in parallel
      const [projectResult, roleResult] = await Promise.all([
        getProjectDetails(project.id),
        getMyProjectRole(project.id)
      ]);

      console.log('Project API Result:', projectResult);
      console.log('Role API Result:', roleResult);
      
      // Check if user has access to this project
      if (!projectResult.success && projectResult.message?.includes("don't have access")) {
        alert("You don't have access to this project");
        onBack();
        return;
      }
      
      if (projectResult.success) {
        setProjectData(projectResult.data);
        
        // Fetch workspace members if we have workspaceId
        if (projectResult.data.workspaceId) {
          const workspaceResult = await getWorkspaceById(projectResult.data.workspaceId);
          if (workspaceResult.success) {
            setWorkspaceMembers(workspaceResult.data.members || []);
          }
        }
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
      console.error('Error loading project:', error);
    } finally {
      setLoading(false);
    }
  };

  if (!project) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <p className="text-gray-600">No project selected</p>
          <button
            onClick={onBack}
            className="mt-4 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
          >
            Back to Projects
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

  if (!projectData) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <p className="text-red-600 mb-4">Failed to load project</p>
          <button
            onClick={onBack}
            className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
          >
            Back to Projects
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
              Back to Workspace
            </button>
          </div>

          <div className="flex items-start justify-between">
            <div>
              <div className="flex items-center gap-3 mb-2">
                <div className="p-2 bg-blue-100 rounded-lg">
                  <Folder size={24} className="text-blue-600" />
                </div>
                <h1 className="text-3xl font-bold text-gray-900">
                  {projectData.name}
                </h1>
              </div>
              
              {projectData.description && (
                <p className="text-gray-600 mb-2">{projectData.description}</p>
              )}
              
              <div className="flex items-center gap-4 text-sm text-gray-500">
                <span className="flex items-center gap-1">
                  <Users size={16} />
                  {projectData.memberCount || 0} members
                </span>
                {userRole !== null && (
                  <span className="px-2 py-1 bg-blue-100 text-blue-700 rounded text-xs font-medium">
                    {getProjectRoleName(userRole)}
                  </span>
                )}
              </div>
            </div>

            {canEditProject(userRole) && (
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
              onClick={() => setActiveTab('tasks')}
              className={`flex items-center gap-2 px-4 py-3 border-b-2 font-medium transition-colors ${
                activeTab === 'tasks'
                  ? 'border-indigo-600 text-indigo-600'
                  : 'border-transparent text-gray-600 hover:text-gray-900'
              }`}
            >
              <ListTodo size={20} />
              Tasks
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
        {activeTab === 'tasks' ? (
          <ProjectTasksTab
            projectId={projectData.id}
            projectMembers={projectData.members || []}
            userRole={userRole}
            onTasksUpdated={loadProjectDetails}
          />
        ) : (
          <ProjectMembersTab
            projectId={projectData.id}
            members={projectData.members || []}
            workspaceMembers={workspaceMembers}
            userRole={userRole}
            onMembersUpdated={loadProjectDetails}
          />
        )}
      </main>
    </div>
  );
}

export default ProjectPage;
