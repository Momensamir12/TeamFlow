import React, { useState, useEffect } from 'react';
import { ListTodo, Plus, Loader, AlertCircle } from 'lucide-react';
import { getProjectTasks } from '../../api/projectApi';
import TaskCard from '../tasks/TaskCard';
import AddTaskForm from '../tasks/AddTaskForm';
import TaskDetailsModal from '../tasks/TaskDetailsModal';

function ProjectTasksTab({ projectId, projectMembers, userRole, onTasksUpdated }) {
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showCreateTask, setShowCreateTask] = useState(false);
  const [editingTask, setEditingTask] = useState(null);
  const [selectedTask, setSelectedTask] = useState(null);

  // Get current user ID
  let currentUserId = null;
  try {
    const userStr = sessionStorage.getItem('user');
    if (userStr) {
      const userData = JSON.parse(userStr);
      currentUserId = userData?.id || null;
    }
  } catch (error) {
    console.error('Error parsing user data:', error);
  }

  useEffect(() => {
    loadTasks();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [projectId]);

  const loadTasks = async () => {
    setLoading(true);
    setError('');
    
    try {
      const result = await getProjectTasks(projectId);
      
      if (result.success) {
        setTasks(result.data || []);
      } else {
        setError(result.message || 'Failed to load tasks');
      }
    } catch (err) {
      setError('An error occurred while loading tasks');
      console.error('Error loading tasks:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleTaskUpdated = () => {
    loadTasks();
    if (onTasksUpdated) {
      onTasksUpdated();
    }
  };

  const handleEditTask = (task) => {
    // Allow editing for both owner and assignee
    if (task.assigneeId === currentUserId || task.ownerId === currentUserId) {
      setEditingTask(task);
      setShowCreateTask(true);
    } else {
      alert('You can only edit tasks you own or are assigned to');
    }
  };

  const handleTaskClick = (task) => {
    setSelectedTask(task);
  };

  const handleCloseDetailsModal = () => {
    setSelectedTask(null);
  };

  const handleCloseForm = () => {
    setShowCreateTask(false);
    setEditingTask(null);
  };

  const canEdit = (task) => {
    // Both assignee and owner can edit status/priority
    return task.assigneeId === currentUserId || task.ownerId === currentUserId;
  };

  const canDelete = (task) => {
    return task.ownerId === currentUserId;
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader size={40} className="animate-spin text-indigo-600" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-lg p-4">
        <div className="flex items-center gap-2 text-red-800">
          <AlertCircle size={20} />
          <span className="font-medium">{error}</span>
        </div>
        <button
          onClick={loadTasks}
          className="mt-2 text-sm text-red-700 hover:text-red-900 underline"
        >
          Try again
        </button>
      </div>
    );
  }

  return (
    <div>
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Project Tasks</h2>
          <p className="text-gray-600 mt-1">
            {tasks.length} task{tasks.length !== 1 ? 's' : ''}
          </p>
        </div>
        
        <button
          onClick={() => setShowCreateTask(true)}
          className="flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
        >
          <Plus size={20} />
          New Task
        </button>
      </div>

      {/* Tasks List */}
      {tasks.length === 0 ? (
        <div className="text-center py-12 bg-white rounded-lg shadow">
          <ListTodo size={48} className="mx-auto text-gray-400 mb-4" />
          <h3 className="text-lg font-semibold text-gray-900 mb-2">
            No tasks yet
          </h3>
          <p className="text-gray-600 mb-4">
            Create your first task to get started
          </p>
          <button
            onClick={() => setShowCreateTask(true)}
            className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
          >
            Create Task
          </button>
        </div>
      ) : (
        <div className="space-y-4">
          {tasks.map(task => (
            <TaskCard
              key={task.id}
              task={task}
              onClick={() => handleTaskClick(task)}
              onEdit={handleEditTask}
              onUpdate={handleTaskUpdated}
              canEdit={canEdit(task)}
            />
          ))}
        </div>
      )}

      {/* Task Details Modal */}
      {selectedTask && (
        <TaskDetailsModal
          task={selectedTask}
          onClose={handleCloseDetailsModal}
          onTaskUpdated={handleTaskUpdated}
          canEdit={canEdit(selectedTask)}
          canDelete={canDelete(selectedTask)}
          projectMembers={projectMembers}
          currentUserId={currentUserId}
        />
      )}

      {/* Add/Edit Task Form Modal */}
      {showCreateTask && (
        <AddTaskForm
          onClose={handleCloseForm}
          onTaskCreated={handleTaskUpdated}
          editingTask={editingTask}
          projectId={projectId}
          projectMembers={projectMembers}
        />
      )}
    </div>
  );
}

export default ProjectTasksTab;
