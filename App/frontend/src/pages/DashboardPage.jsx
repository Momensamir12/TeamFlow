import React, { useState, useEffect } from 'react';
import { LogOut, CheckSquare, Users } from 'lucide-react';
import TaskList from '../components/tasks/TaskList';
import AddTaskForm from '../components/tasks/AddTaskForm';
import WorkspaceList from '../components/workspaces/WorkspaceList';
import WorkspacePage from './WorkspacePage';
import { getUserTasks } from '../api/taskApi';

function DashboardPage({ onLogout }) {
  const [user, setUser] = useState(null);
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showCreateTask, setShowCreateTask] = useState(false);
  const [activeTab, setActiveTab] = useState('tasks');
  const [selectedWorkspace, setSelectedWorkspace] = useState(null);

  useEffect(() => {
    const userData = JSON.parse(sessionStorage.getItem('user'));
    setUser(userData);
    
    if (activeTab === 'tasks') {
      loadTasks();
    }
  }, [activeTab]);

  const loadTasks = async () => {
    setLoading(true);
    try {
      const result = await getUserTasks();
      if (result.success) {
        setTasks(result.data || []);
      }
    } catch (error) {
      console.error('Error loading tasks:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = () => {
    sessionStorage.removeItem('accessToken');
    sessionStorage.removeItem('user');
    onLogout();
  };

  const handleWorkspaceSelect = (workspace) => {
    setSelectedWorkspace(workspace);
  };

  const handleBackToWorkspaces = () => {
    setSelectedWorkspace(null);
  };

  // If a workspace is selected, show WorkspacePage
  if (selectedWorkspace) {
    return (
      <WorkspacePage 
        workspace={selectedWorkspace}
        onBack={handleBackToWorkspaces}
      />
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-6 py-4">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">TeamFlow</h1>
              <p className="text-sm text-gray-600">Welcome back, {user?.firstName}!</p>
            </div>
            <button
              onClick={handleLogout}
              className="flex items-center gap-2 px-4 py-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
            >
              <LogOut size={20} />
              Logout
            </button>
          </div>
        </div>
      </header>

      {/* Navigation Tabs */}
      <div className="bg-white border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-6">
          <div className="flex gap-4">
            <button
              onClick={() => setActiveTab('tasks')}
              className={`flex items-center gap-2 px-4 py-3 border-b-2 font-medium transition-colors ${
                activeTab === 'tasks'
                  ? 'border-indigo-600 text-indigo-600'
                  : 'border-transparent text-gray-600 hover:text-gray-900'
              }`}
            >
              <CheckSquare size={20} />
              My Tasks
            </button>
            <button
              onClick={() => setActiveTab('workspaces')}
              className={`flex items-center gap-2 px-4 py-3 border-b-2 font-medium transition-colors ${
                activeTab === 'workspaces'
                  ? 'border-indigo-600 text-indigo-600'
                  : 'border-transparent text-gray-600 hover:text-gray-900'
              }`}
            >
              <Users size={20} />
              Workspaces
            </button>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-6 py-8">
        {activeTab === 'tasks' ? (
          <>
            <div className="flex items-center justify-between mb-6">
              <div>
                <h2 className="text-2xl font-bold text-gray-900">My Tasks</h2>
                <p className="text-gray-600 mt-1">
                  {tasks.length} task{tasks.length !== 1 ? 's' : ''} total
                </p>
              </div>
              <button
                onClick={() => setShowCreateTask(true)}
                className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
              >
                + New Task
              </button>
            </div>

            {loading ? (
              <div className="text-center py-12">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto"></div>
              </div>
            ) : (
              <TaskList 
                tasks={tasks} 
                onTaskUpdated={loadTasks}
              />
            )}

            {showCreateTask && (
              <AddTaskForm
                onClose={() => setShowCreateTask(false)}
                onTaskAdded={loadTasks}
              />
            )}
          </>
        ) : (
          <WorkspaceList onWorkspaceSelect={handleWorkspaceSelect} />
        )}
      </main>
    </div>
  );
}

export default DashboardPage;