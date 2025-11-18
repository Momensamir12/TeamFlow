import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import Layout from '../components/common/Layout';
import TaskList from '../components/tasks/TaskList';
import AddTaskForm from '../components/tasks/AddTaskForm';
import WorkspaceList from '../components/workspaces/WorkspaceList';
import WorkspacePage from './WorkspacePage';
import { getUserTasks } from '../api/taskApi';
import signalRService from '../services/signalRService';
import UserProfileModal from '../components/auth/UserProfileModal';
import ChangePasswordModal from '../components/auth/ChangePasswordModal';

function DashboardPage({ onLogout }) {
  const [user, setUser] = useState(null);
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showCreateTask, setShowCreateTask] = useState(false);
  const [activeTab, setActiveTab] = useState('tasks');
  const [showProfileModal, setShowProfileModal] = useState(false);
  const [showPasswordModal, setShowPasswordModal] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const selectedWorkspace = location.state?.workspace;
  const initialTab = location.state?.activeTab || 'tasks';

  useEffect(() => {
    // Set active tab from location state if provided
    if (location.state?.activeTab) {
      setActiveTab(location.state.activeTab);
    }

    const userData = JSON.parse(sessionStorage.getItem('user'));
    setUser(userData);
    
    if (activeTab === 'tasks') {
      loadTasks();
    }

    // Initialize SignalR connection
    signalRService.start();

    // Listen for task assignment notifications
    const handleNotification = (taskId) => {
      // Add notification to bell
      if (window.addNotification) {
        window.addNotification({
          type: 'TaskAssigned',
          taskId: taskId,
          message: 'You have been assigned a new task'
        });
      }

      // Refresh tasks if on tasks tab
      if (activeTab === 'tasks') {
        loadTasks();
      }
    };

    signalRService.onNotification(handleNotification);

    // Cleanup
    return () => {
      signalRService.offNotification(handleNotification);
    };
  }, [activeTab, location.state?.activeTab]);

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
    navigate('/dashboard', { state: { workspace } });
  };

  const handleBackToWorkspaces = () => {
    navigate('/dashboard');
  };

  // If a workspace is selected, show WorkspacePage
  if (selectedWorkspace) {
    return (
      <WorkspacePage 
        workspace={selectedWorkspace}
        onBack={handleBackToWorkspaces}
        user={user}
        onLogout={handleLogout}
        onOpenProfile={() => setShowProfileModal(true)}
        onOpenPassword={() => setShowPasswordModal(true)}
      />
    );
  }

  return (
    <Layout
      activeTab={activeTab}
      onTabChange={setActiveTab}
      onLogout={handleLogout}
      user={user}
      onOpenProfile={() => setShowProfileModal(true)}
      onOpenPassword={() => setShowPasswordModal(true)}
    >
      {/* Main Content */}
      {activeTab === 'tasks' ? (
        <>
          <div className="flex items-center justify-between mb-6">
            <div>
              <h2 className="text-3xl font-bold text-gray-900">My Tasks</h2>
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
              onTaskCreated={loadTasks}
            />
          )}
        </>
      ) : (
        <div>
          <h2 className="text-3xl font-bold text-gray-900 mb-6">Workspaces</h2>
          <WorkspaceList onWorkspaceSelect={handleWorkspaceSelect} />
        </div>
      )}

      {/* Modals */}
      <UserProfileModal 
        isOpen={showProfileModal} 
        onClose={() => setShowProfileModal(false)} 
      />
      <ChangePasswordModal 
        isOpen={showPasswordModal} 
        onClose={() => setShowPasswordModal(false)} 
      />
    </Layout>
  );
}

export default DashboardPage;