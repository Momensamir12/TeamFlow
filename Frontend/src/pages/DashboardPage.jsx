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
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedWorkspace, setSelectedWorkspace] = useState(null);
  const navigate = useNavigate();
  const location = useLocation();

  // Sync state with location on mount and location changes
  useEffect(() => {
    const stateActiveTab = location.state?.activeTab;
    const stateWorkspace = location.state?.workspace;

    // Update active tab if provided in state
    if (stateActiveTab && stateActiveTab !== activeTab) {
      setActiveTab(stateActiveTab);
    }

    // Only set workspace if it's provided AND we're not explicitly navigating to tasks
    if (stateWorkspace && stateActiveTab !== 'tasks') {
      setSelectedWorkspace(stateWorkspace);
    } else if (stateActiveTab === 'tasks' || stateActiveTab === 'workspaces') {
      // Clear workspace when explicitly navigating to tasks or workspaces tab
      setSelectedWorkspace(null);
    }
  }, [location.state]);

  // Initialize user, tasks, and SignalR on mount
  useEffect(() => {
    const userData = JSON.parse(sessionStorage.getItem('user'));
    setUser(userData);

    // Load tasks on initial mount if on tasks tab
    if (activeTab === 'tasks') {
      loadTasks();
    }

    // Initialize SignalR connection
    console.log('[DashboardPage] Component mounted, starting SignalR connection...');
    signalRService.start();

    // Don't stop the connection on cleanup - it should persist across the session
    // Only stop when user logs out
  }, []);

  // Handle task assignment notifications
  useEffect(() => {
    console.log('[DashboardPage] Setting up notification handler. Current activeTab:', activeTab);
    
    const handleNotification = (taskId) => {
      console.log('[DashboardPage] 📬 Notification handler called for task:', taskId);
      console.log('[DashboardPage] Current activeTab:', activeTab);
      
      // Add notification to bell
      if (window.addNotification) {
        console.log('[DashboardPage] Adding notification to bell');
        window.addNotification({
          type: 'TaskAssigned',
          taskId: taskId,
          message: 'You have been assigned a new task'
        });
      } else {
        console.warn('[DashboardPage] window.addNotification is not available');
      }

      // Refresh tasks if on tasks tab
      if (activeTab === 'tasks') {
        console.log('[DashboardPage] On tasks tab, refreshing task list...');
        loadTasks();
      } else {
        console.log('[DashboardPage] Not on tasks tab, skipping task refresh');
      }
    };

    signalRService.onNotification(handleNotification);

    // Cleanup
    return () => {
      console.log('[DashboardPage] Cleaning up notification handler');
      signalRService.offNotification(handleNotification);
    };
  }, [activeTab]);

  // Load tasks when switching to tasks tab or on initial mount
  useEffect(() => {
    if (activeTab === 'tasks') {
      loadTasks();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeTab]); // loadTasks is stable, no need to include it

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

  const handleTaskCreated = (newTask) => {
    // Add the new task to local state instead of refetching
    console.log('[DashboardPage] New task created:', newTask);
    if (newTask && (newTask.id || newTask.Id)) {
      // Normalize property names from PascalCase to camelCase
      const task = {
        id: newTask.id || newTask.Id,
        title: newTask.title || newTask.Title,
        description: newTask.description || newTask.Description,
        status: newTask.status !== undefined ? newTask.status : newTask.Status,
        deadline: newTask.deadline || newTask.Deadline,
        priority: newTask.priority !== undefined ? newTask.priority : newTask.Priority,
        assigneeId: newTask.assigneeId || newTask.AssigneeId,
        ownerId: newTask.ownerId || newTask.OwnerId,
        assignerUsername: newTask.assignerUsername || newTask.AssignerUsername,
        projectId: newTask.projectId || newTask.ProjectId
      };
      setTasks(prev => {
        console.log('[DashboardPage] Adding task to list. Current count:', prev.length);
        return [task, ...prev];
      });
    } else {
      console.error('[DashboardPage] Invalid task data, refetching:', newTask);
      // Fallback to refetching if no task data returned
      loadTasks();
    }
  };

  const handleLogout = () => {
    sessionStorage.removeItem('accessToken');
    sessionStorage.removeItem('user');
    onLogout();
  };

  const handleWorkspaceSelect = (workspace) => {
    navigate('/dashboard', { state: { workspace, activeTab: 'workspaces' } });
  };

  const handleBackToWorkspaces = () => {
    navigate('/dashboard', { state: { activeTab: 'workspaces' } });
  };

  const handleTabChange = (tab) => {
    navigate('/dashboard', { state: { activeTab: tab } });
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

  // Filter tasks and workspaces based on search query
  const filteredTasks = tasks.filter(task => 
    task.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
    task.description?.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <Layout
      activeTab={activeTab}
      onTabChange={handleTabChange}
      onLogout={handleLogout}
      user={user}
      onOpenProfile={() => setShowProfileModal(true)}
      onOpenPassword={() => setShowPasswordModal(true)}
      searchQuery={searchQuery}
      onSearchChange={setSearchQuery}
    >
      {/* Main Content */}
      {activeTab === 'tasks' ? (
        <>
          <div className="flex items-center justify-between mb-6">
            <div>
              <h2 className="text-3xl font-bold text-gray-900">My Tasks</h2>
              <p className="text-gray-600 mt-1">
                {filteredTasks.length} of {tasks.length} task{tasks.length !== 1 ? 's' : ''}
                {searchQuery && ` matching "${searchQuery}"`}
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
              tasks={filteredTasks} 
              onTaskUpdated={loadTasks}
              searchQuery={searchQuery}
            />
          )}

          {showCreateTask && (
            <AddTaskForm
              onClose={() => setShowCreateTask(false)}
              onTaskCreated={handleTaskCreated}
            />
          )}
        </>
      ) : (
        <div>
          <h2 className="text-3xl font-bold text-gray-900 mb-6">Workspaces</h2>
          <WorkspaceList 
            onWorkspaceSelect={handleWorkspaceSelect} 
            searchQuery={searchQuery}
          />
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