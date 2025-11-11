import React, { useState } from 'react';
import TaskCard from './TaskCard';
import TaskModal from './TaskModal';
import { TASK_STATUS } from '../../constants/config';

function TaskList({ tasks, onTaskUpdated }) {
  const [selectedTask, setSelectedTask] = useState(null);

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
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
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
        <p className="text-gray-500 text-lg">No tasks yet. Create your first task!</p>
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
        <TaskModal 
          task={selectedTask}
          onClose={handleCloseModal}
          onTaskUpdated={onTaskUpdated}
        />
      )}
    </>
  );
}

export default TaskList;