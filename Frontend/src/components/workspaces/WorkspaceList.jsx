import React, { useState, useEffect } from 'react';
import { Plus, Users, Loader, AlertCircle } from 'lucide-react';
import WorkspaceCard from './WorkspaceCard';
import CreateWorkspaceModal from './CreateWorkspaceModal';
import JoinWorkspaceModal from './JoinWorkspaceModal';
import { getUserWorkspaces } from '../../api/workspaceApi';

function WorkspaceList({ onWorkspaceSelect, searchQuery = '' }) {
  const [workspaces, setWorkspaces] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [filter, setFilter] = useState('active');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showJoinModal, setShowJoinModal] = useState(false);

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

  const filteredWorkspaces = workspaces.filter(workspace => {
    // First filter by archive status
    const matchesFilter = filter === 'archived' ? workspace.isArchived === true : workspace.isArchived === false;
    
    // Then filter by search query
    const matchesSearch = !searchQuery || 
      workspace.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      workspace.description?.toLowerCase().includes(searchQuery.toLowerCase());
    
    return matchesFilter && matchesSearch;
  });

  const handleWorkspaceClick = (workspace) => {
    onWorkspaceSelect(workspace);
  };

  return (
    <div>
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Workspaces</h2>
          <p className="text-gray-600 mt-1">Manage and collaborate in your workspaces</p>
        </div>
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
            onClick={() => setShowJoinModal(true)}
            className="flex items-center gap-2 px-4 py-2 bg-white text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
          >
            <Users size={20} />
            Join Workspace
          </button>
          <button
            onClick={() => setShowCreateModal(true)}
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
                {searchQuery 
                  ? `No workspaces found matching "${searchQuery}"`
                  : filter === 'archived' ? 'No archived workspaces' : 'No workspaces yet'
                }
              </h3>
              <p className="text-gray-600 mb-4">
                {searchQuery
                  ? 'Try adjusting your search terms'
                  : filter === 'archived' 
                    ? 'You don\'t have any archived workspaces'
                    : 'Create your first workspace or join an existing one'
                }
              </p>
              {filter === 'active' && !searchQuery && (
                <div className="flex gap-3 justify-center">
                  <button
                    onClick={() => setShowCreateModal(true)}
                    className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
                  >
                    Create Workspace
                  </button>
                  <button
                    onClick={() => setShowJoinModal(true)}
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
          {searchQuery && ` matching "${searchQuery}"`}
        </div>
      )}

      {/* Modals */}
      {showCreateModal && (
        <CreateWorkspaceModal
          onClose={() => setShowCreateModal(false)}
          onWorkspaceCreated={loadWorkspaces}
        />
      )}

      {showJoinModal && (
        <JoinWorkspaceModal
          onClose={() => setShowJoinModal(false)}
          onWorkspaceJoined={loadWorkspaces}
        />
      )}
    </div>
  );
}

export default WorkspaceList;