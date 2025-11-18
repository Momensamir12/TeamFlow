import React, { useState, useEffect } from 'react';
import { Plus, Users, Loader, AlertCircle } from 'lucide-react';
import WorkspaceCard from '../components/workspaces/WorkspaceCard';
import { getUserWorkspaces } from '../api/workspaceApi';

function WorkspacesPage() {
  // State management
  const [workspaces, setWorkspaces] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [filter, setFilter] = useState('active'); // 'active' or 'archived'

  // Fetch workspaces on component mount
  useEffect(() => {
    loadWorkspaces();
  }, []);

  const loadWorkspaces = async () => {
    setLoading(true);
    setError('');
    
    try {
      const result = await getUserWorkspaces();
      
      if (result.success) {
        setWorkspaces(result.data || []);
      } else {
        setError(result.message || 'Failed to load workspaces');
      }
    } catch (err) {
      setError('An error occurred while loading workspaces');
      console.error('Error loading workspaces:', err);
    } finally {
      setLoading(false);
    }
  };

  // Filter workspaces based on active/archived status
  const filteredWorkspaces = workspaces.filter(workspace => {
    if (filter === 'archived') {
      return workspace.isArchived === true;
    }
    return workspace.isArchived === false;
  });

  // Handle workspace card click
  const handleWorkspaceClick = (workspace) => {
    // TODO: Open workspace details modal
  };

  // Handle create workspace button
  const handleCreateWorkspace = () => {
    // TODO: Open create workspace modal
  };

  // Handle join workspace button
  const handleJoinWorkspace = () => {
    // TODO: Open join workspace modal
  };

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      <div className="max-w-7xl mx-auto">
        
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Workspaces</h1>
          <p className="text-gray-600">Manage and collaborate in your workspaces</p>
        </div>

        {/* Action Buttons */}
        <div className="flex items-center justify-between mb-6">
          <div className="flex gap-2">
            <button
              onClick={() => setFilter('active')}
              className={`px-4 py-2 rounded-lg font-medium transition-colors ${
                filter === 'active'
                  ? 'bg-indigo-600 text-white'
                  : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'
              }`}
            >
              Active
            </button>
            <button
              onClick={() => setFilter('archived')}
              className={`px-4 py-2 rounded-lg font-medium transition-colors ${
                filter === 'archived'
                  ? 'bg-indigo-600 text-white'
                  : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'
              }`}
            >
              Archived
            </button>
          </div>

          <div className="flex gap-3">
            <button
              onClick={handleJoinWorkspace}
              className="flex items-center gap-2 px-4 py-2 bg-white text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
            >
              <Users size={20} />
              Join Workspace
            </button>
            <button
              onClick={handleCreateWorkspace}
              className="flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
            >
              <Plus size={20} />
              Create Workspace
            </button>
          </div>
        </div>

        {/* Loading State */}
        {loading && (
          <div className="flex items-center justify-center py-12">
            <Loader size={40} className="animate-spin text-indigo-600" />
          </div>
        )}

        {/* Error State */}
        {error && !loading && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
            <div className="flex items-center gap-2 text-red-800">
              <AlertCircle size={20} />
              <span className="font-medium">{error}</span>
            </div>
            <button
              onClick={loadWorkspaces}
              className="mt-2 text-sm text-red-700 hover:text-red-900 underline"
            >
              Try again
            </button>
          </div>
        )}

        {/* Workspaces Grid */}
        {!loading && !error && (
          <>
            {filteredWorkspaces.length === 0 ? (
              <div className="text-center py-12 bg-white rounded-lg shadow">
                <Users size={48} className="mx-auto text-gray-400 mb-4" />
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
                  {filter === 'archived' ? 'No archived workspaces' : 'No workspaces yet'}
                </h3>
                <p className="text-gray-600 mb-4">
                  {filter === 'archived' 
                    ? 'You don\'t have any archived workspaces'
                    : 'Create your first workspace or join an existing one'
                  }
                </p>
                {filter === 'active' && (
                  <div className="flex gap-3 justify-center">
                    <button
                      onClick={handleCreateWorkspace}
                      className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
                    >
                      Create Workspace
                    </button>
                    <button
                      onClick={handleJoinWorkspace}
                      className="px-4 py-2 bg-white text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                    >
                      Join Workspace
                    </button>
                  </div>
                )}
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {filteredWorkspaces.map(workspace => (
                  <WorkspaceCard
                    key={workspace.id}
                    workspace={workspace}
                    onClick={() => handleWorkspaceClick(workspace)}
                  />
                ))}
              </div>
            )}
          </>
        )}

        {/* Workspace count */}
        {!loading && filteredWorkspaces.length > 0 && (
          <div className="mt-6 text-center text-sm text-gray-500">
            Showing {filteredWorkspaces.length} {filter} workspace{filteredWorkspaces.length !== 1 ? 's' : ''}
          </div>
        )}

      </div>
    </div>
  );
}

export default WorkspacesPage;