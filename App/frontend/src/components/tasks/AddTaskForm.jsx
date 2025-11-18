import React, { useState } from 'react';
import { X, AlertCircle, ListTodo, ChevronDown } from 'lucide-react';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.css';
import { addTask, updateTask } from '../../api/taskApi';
import { TASK_STATUS, TASK_PRIORITY } from '../../constants/config';

function AddTaskForm({ onTaskCreated, onClose, editingTask, projectId, projectMembers }) {
  const [formData, setFormData] = useState({
    title: editingTask?.title || '',
    description: editingTask?.description || '',
    status: editingTask?.status ?? TASK_STATUS.TODO,
    priority: editingTask?.priority ?? TASK_PRIORITY.MEDIUM,
    // store deadline as Date | null for DatePicker
    deadline: editingTask?.deadline ? new Date(editingTask.deadline) : null,
    projectId: projectId || editingTask?.projectId || null,
    assigneeId: editingTask?.assigneeId || ''
  });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [showStatusDropdown, setShowStatusDropdown] = useState(false);
  const [showPriorityDropdown, setShowPriorityDropdown] = useState(false);
  const [showAssigneeDropdown, setShowAssigneeDropdown] = useState(false);

  const statusOptions = [
    { value: TASK_STATUS.TODO, label: 'To Do' },
    { value: TASK_STATUS.IN_PROGRESS, label: 'In Progress' },
    { value: TASK_STATUS.DONE, label: 'Done' },
    { value: TASK_STATUS.BLOCKED, label: 'Blocked' }
  ];

  const priorityOptions = [
    { value: TASK_PRIORITY.LOW, label: 'Low' },
    { value: TASK_PRIORITY.MEDIUM, label: 'Medium' },
    { value: TASK_PRIORITY.HIGH, label: 'High' },
    { value: TASK_PRIORITY.URGENT, label: 'Urgent' }
  ];

  const getAssigneeName = () => {
    if (!formData.assigneeId) return 'Unassigned';
    const assignee = projectMembers?.find(m => m.userId === formData.assigneeId);
    return assignee ? `${assignee.userName} (${assignee.userEmail})` : 'Unassigned';
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const taskData = {
        ...formData,
        deadline: formData.deadline ? new Date(formData.deadline).toISOString() : null,
        assigneeId: formData.assigneeId || null
      };

      const result = editingTask 
        ? await updateTask({ ...taskData, id: editingTask.id })
        : await addTask(taskData);
      
      if (result.success) {
        onTaskCreated();
        onClose();
      } else {
        setError(result.message || `Failed to ${editingTask ? 'update' : 'add'} task`);
      }
    } catch (error) {
      console.error('Catch block error:', error);
      setError(`Failed to ${editingTask ? 'update' : 'add'} task. Please try again.`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div 
      className="fixed inset-0 bg-gray-900 bg-opacity-50 flex items-center justify-center p-4"
      style={{ zIndex: 9999 }}
      onClick={onClose}
    >
      <div 
        className="bg-white rounded-lg shadow-2xl max-w-2xl w-full relative overflow-visible"
        style={{ zIndex: 10000 }}
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-indigo-100 rounded-lg">
              <ListTodo size={24} className="text-indigo-600" />
            </div>
            <h2 className="text-2xl font-bold text-gray-900">
              {editingTask ? 'Edit Task' : 'Add New Task'}
            </h2>
          </div>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4 overflow-visible">
          {error && (
            <div className="bg-red-50 border border-red-200 rounded-lg p-3">
              <div className="flex items-center gap-2 text-red-800">
                <AlertCircle size={20} />
                <span className="text-sm font-medium">{error}</span>
              </div>
            </div>
          )}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Task Title *
          </label>
          <input
            type="text"
            value={formData.title}
            onChange={(e) => setFormData({ ...formData, title: e.target.value })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
            placeholder="Enter task title"
            required
            disabled={loading}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Description
          </label>
          <textarea
            value={formData.description}
            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
            placeholder="Enter task description"
            rows="3"
            disabled={loading}
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="relative">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Status
            </label>
            <button
              type="button"
              onClick={() => setShowStatusDropdown(!showStatusDropdown)}
              disabled={loading}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent bg-white text-left flex items-center justify-between disabled:opacity-50"
            >
              <span>{statusOptions.find(o => o.value === formData.status)?.label}</span>
              <ChevronDown size={16} />
            </button>
            {showStatusDropdown && (
              <div className="absolute z-50 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg">
                {statusOptions.map(option => (
                  <button
                    key={option.value}
                    type="button"
                    onClick={() => {
                      setFormData({ ...formData, status: option.value });
                      setShowStatusDropdown(false);
                    }}
                    className="w-full px-4 py-2 text-left hover:bg-indigo-50 first:rounded-t-lg last:rounded-b-lg"
                  >
                    {option.label}
                  </button>
                ))}
              </div>
            )}
          </div>

          <div className="relative">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Priority
            </label>
            <button
              type="button"
              onClick={() => setShowPriorityDropdown(!showPriorityDropdown)}
              disabled={loading}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent bg-white text-left flex items-center justify-between disabled:opacity-50"
            >
              <span>{priorityOptions.find(o => o.value === formData.priority)?.label}</span>
              <ChevronDown size={16} />
            </button>
            {showPriorityDropdown && (
              <div className="absolute z-50 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg">
                {priorityOptions.map(option => (
                  <button
                    key={option.value}
                    type="button"
                    onClick={() => {
                      setFormData({ ...formData, priority: option.value });
                      setShowPriorityDropdown(false);
                    }}
                    className="w-full px-4 py-2 text-left hover:bg-indigo-50 first:rounded-t-lg last:rounded-b-lg"
                  >
                    {option.label}
                  </button>
                ))}
              </div>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Deadline
            </label>
            <DatePicker
              selected={formData.deadline}
              onChange={(date) => setFormData({ ...formData, deadline: date })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
              placeholderText="Select a deadline"
              disabled={loading}
              dateFormat="yyyy-MM-dd"
              withPortal
            />
          </div>
        </div>

        {/* Assignee Selection - Only show if projectMembers are provided */}
        {projectMembers && projectMembers.length > 0 && (
          <div className="relative">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Assign To
            </label>
            <button
              type="button"
              onClick={() => setShowAssigneeDropdown(!showAssigneeDropdown)}
              disabled={loading}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent bg-white text-left flex items-center justify-between disabled:opacity-50"
            >
              <span className={formData.assigneeId ? 'text-gray-900' : 'text-gray-500'}>
                {getAssigneeName()}
              </span>
              <ChevronDown size={16} />
            </button>
            {showAssigneeDropdown && (
              <div className="absolute z-50 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg max-h-60 overflow-y-auto">
                <button
                  type="button"
                  onClick={() => {
                    setFormData({ ...formData, assigneeId: '' });
                    setShowAssigneeDropdown(false);
                  }}
                  className="w-full px-4 py-2 text-left hover:bg-indigo-50 first:rounded-t-lg"
                >
                  Unassigned
                </button>
                {projectMembers.map(member => (
                  <button
                    key={member.userId}
                    type="button"
                    onClick={() => {
                      setFormData({ ...formData, assigneeId: member.userId });
                      setShowAssigneeDropdown(false);
                    }}
                    className="w-full px-4 py-2 text-left hover:bg-indigo-50 last:rounded-b-lg"
                  >
                    {member.userName} ({member.userEmail})
                  </button>
                ))}
              </div>
            )}
          </div>
        )}

        <div className="flex gap-3 pt-4">
          <button
            type="button"
            onClick={onClose}
            disabled={loading}
            className="px-4 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors disabled:opacity-50"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={loading}
            className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors font-medium disabled:opacity-50"
          >
            {loading ? (editingTask ? 'Updating...' : 'Adding...') : (editingTask ? 'Update Task' : 'Add Task')}
          </button>
        </div>
      </form>
      </div>
    </div>
  );
}

export default AddTaskForm;