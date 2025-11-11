import React, { useState, useEffect } from 'react';
import { ArrowLeft, Folder, Users, ListTodo, Settings } from 'lucide-react';
import { getProjectDetails } from '../api/projectApi';
import { getProjectRoleName, canEditProject } from '../constants/config';

function ProjectPage({ project, onBack }) {
  const [projectData, setProjectData] = useState(project);
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
      const result = await getProjectDetails(project.id);
      if (result.success) {
        setProjectData(result.data);
        // Find current user's role
        const userStr = sessionStorage.getItem('user');
        if (userStr) {
          const currentUserId = JSON.parse(userStr).id;
          const member = result.data.members?.find(m => m.userId === currentUserId);
          setUserRole(member?.role ?? 0);
        }
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
          <div className="text-center py-12 bg-white rounded-lg shadow">
            <ListTodo size={48} className="mx-auto text-gray-400 mb-4" />
            <h3 className="text-lg font-semibold text-gray-900 mb-2">
              Project Tasks
            </h3>
            <p className="text-gray-600">
              Task management for this project coming soon...
            </p>
          </div>
        ) : (
          <div className="text-center py-12 bg-white rounded-lg shadow">
            <Users size={48} className="mx-auto text-gray-400 mb-4" />
            <h3 className="text-lg font-semibold text-gray-900 mb-2">
              Project Members
            </h3>
            <p className="text-gray-600">
              Member management for this project coming soon...
            </p>
          </div>
        )}
      </main>
    </div>
  );
}

export default ProjectPage;
