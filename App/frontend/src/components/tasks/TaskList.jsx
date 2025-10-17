import React from 'react';
import TaskCard from './TaskCard';

function TaskList({ tasks }) {
  // Group tasks by status
  const todoTasks = tasks.filter(t => t.status?.toLowerCase() === 'todo');
  const inProgressTasks = tasks.filter(t => t.status?.toLowerCase() === 'inprogress');
  const doneTasks = tasks.filter(t => t.status?.toLowerCase() === 'done');
  const blockedTasks = tasks.filter(t => t.status?.toLowerCase() === 'blocked');

  const TaskSection = ({ title, tasks, bgColor }) => (
    tasks.length > 0 && (
      <div className="mb-8">
        <h2 className={`text-lg font-semibold mb-4 pb-2 border-b-2 ${bgColor}`}>
          {title} ({tasks.length})
        </h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {tasks.map(task => (
            <TaskCard key={task.id || Math.random()} task={task} />
          ))}
        </div>
      </div>
    )
  );

  return (
    <div>
      <TaskSection 
        title="To Do" 
        tasks={todoTasks} 
        bgColor="border-gray-400"
      />
      <TaskSection 
        title="In Progress" 
        tasks={inProgressTasks} 
        bgColor="border-blue-400"
      />
      <TaskSection 
        title="Done" 
        tasks={doneTasks} 
        bgColor="border-green-400"
      />
      <TaskSection 
        title="Blocked" 
        tasks={blockedTasks} 
        bgColor="border-red-400"
      />
    </div>
  );
}

export default TaskList;