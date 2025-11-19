import React, { useState } from 'react';
import TaskCard from './TaskCard';
import TaskDetailsModal from './TaskDetailsModal';
import { TASK_STATUS } from '../../constants/config';

function TaskList({ tasks, onTaskUpdated, searchQuery = '' }) {
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

  // Group tasks by status
  const todoTasks = tasks.filter(t => t.status === TASK_STATUS.TODO);
  const inProgressTasks = tasks.filter(t => t.status === TASK_STATUS.IN_PROGRESS);
  const doneTasks = tasks.filter(t => t.status === TASK_STATUS.DONE);
  const blockedTasks = tasks.filter(t => t.status === TASK_STATUS.BLOCKED);

  const handleTaskClick = (task) => {
    setSelectedTask(task);
  };

  const handleCloseModal = () => {
    setSelectedTask(null);
  };

  const canEdit = (task) => {
    // Both assignee and owner can edit status/priority
    return task.assigneeId === currentUserId || task.ownerId === currentUserId;
  };

  const canDelete = (task) => {
    return task.ownerId === currentUserId;
  };

  const TaskSection = ({ title, tasks, bgColor, statusColor }) => {
    return tasks.length > 0 ? (
      <div className="mb-8">
        <div className={`flex items-center gap-2 mb-4 pb-2 border-b-2 ${bgColor}`}>
          <h2 className="text-lg font-semibold">
            {title}
          </h2>
          <span className={`px-2 py-1 text-sm font-medium rounded-full ${statusColor}`}>
            {tasks.length}
          </span>
        </div>
        <div className="space-y-2">
          {tasks.map(task => (
            <TaskCard 
              key={task.id} 
              task={task} 
              onClick={() => handleTaskClick(task)}
            />
          ))}
        </div>
      </div>
    ) : null;
  };

  if (tasks.length === 0) {
    return (
      <div className="text-center py-12 bg-white rounded-lg shadow">
        <p className="text-gray-500 text-lg">
          {searchQuery ? `No tasks found matching "${searchQuery}"` : 'No tasks yet. Create your first task!'}
        </p>
      </div>
    );
  }

  // Check if all filtered sections are empty
  const hasVisibleTasks = todoTasks.length > 0 || inProgressTasks.length > 0 || doneTasks.length > 0 || blockedTasks.length > 0;
  
  if (!hasVisibleTasks && searchQuery) {
    return (
      <div className="text-center py-12 bg-white rounded-lg shadow">
        <p className="text-gray-500 text-lg">No tasks found matching "{searchQuery}"</p>
        <p className="text-gray-400 text-sm mt-2">Try adjusting your search terms</p>
      </div>
    );
  }

  return (
    <>
      <div>
        <TaskSection 
          title="To Do" 
          tasks={todoTasks} 
          bgColor="border-gray-400"
          statusColor="bg-gray-100 text-gray-700"
        />
        <TaskSection 
          title="In Progress" 
          tasks={inProgressTasks} 
          bgColor="border-blue-400"
          statusColor="bg-blue-100 text-blue-700"
        />
        <TaskSection 
          title="Done" 
          tasks={doneTasks} 
          bgColor="border-green-400"
          statusColor="bg-green-100 text-green-700"
        />
        <TaskSection 
          title="Blocked" 
          tasks={blockedTasks} 
          bgColor="border-red-400"
          statusColor="bg-red-100 text-red-700"
        />
      </div>

      {/* Task Detail Modal */}
      {selectedTask && (
        <TaskDetailsModal 
          task={selectedTask}
          onClose={handleCloseModal}
          onTaskUpdated={() => {
            onTaskUpdated();
            handleCloseModal();
          }}
          canEdit={canEdit(selectedTask)}
          canDelete={canDelete(selectedTask)}
          projectMembers={[]}
          currentUserId={currentUserId}
        />
      )}
    </>
  );
}

export default TaskList;