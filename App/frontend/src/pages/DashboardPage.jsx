import React, { useState, useEffect } from 'react';
import { LogOut, Plus } from 'lucide-react';
import TaskList from '../components/tasks/TaskList';
import AddTaskForm from '../components/tasks/AddTaskForm';
import { getUserTasks } from '../api/taskApi';

function DashboardPage({ onLogout }) {
  const [tasks, setTasks] = useState([]);
  const [showAddTask, setShowAddTask] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadTasks();
  }, []);

  const loadTasks = async () => {
    setLoading(true);
    try {
      const token = sessionStorage.getItem('accessToken');
      const result = await getUserTasks(token);
      
      if (result.success && result.data) {
        setTasks(result.data);
      } else {
        setError('Failed to load tasks');
      }
    } catch (err) {
      setError('Failed to connect to server');
    } finally {
      setLoading(false);
    }
  };

  const handleTaskAdded = () => {
    setShowAddTask(false);
    loadTasks(); // Refresh task list
  };

  const handleLogout = () => {
    sessionStorage.removeItem('accessToken');
    sessionStorage.removeItem('refreshToken');
    onLogout();
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            <h1 className="text-2xl font-bold text-gray-900">My Tasks</h1>
            <button
              onClick={handleLogout}
              className="flex items-center gap-2 px-4 py-2 text-gray-700 hover:text-gray-900 hover:bg-gray-100 rounded-lg transition-colors"
            >
              <LogOut size={20} />
              Logout
            </button>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Add Task Button */}
        <div className="mb-6">
          <button
            onClick={() => setShowAddTask(!showAddTask)}
            className="flex items-center gap-2 px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors font-medium"
          >
            <Plus size={20} />
            {showAddTask ? 'Cancel' : 'Add New Task'}
          </button>
        </div>

        {/* Add Task Form */}
        {showAddTask && (
          <div className="mb-6">
            <AddTaskForm onTaskAdded={handleTaskAdded} onCancel={() => setShowAddTask(false)} />
          </div>
        )}

        {/* Error Message */}
        {error && (
          <div className="mb-6 p-4 bg-red-100 border border-red-400 text-red-700 rounded-lg">
            {error}
          </div>
        )}

        {/* Tasks List */}
        {loading ? (
          <div className="text-center py-12">
            <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
            <p className="mt-4 text-gray-600">Loading tasks...</p>
          </div>
        ) : tasks.length === 0 ? (
          <div className="text-center py-12 bg-white rounded-lg shadow">
            <p className="text-gray-600 text-lg">No tasks yet. Create your first task!</p>
          </div>
        ) : (
          <TaskList tasks={tasks} />
        )}
      </main>
    </div>
  );
}

export default DashboardPage;