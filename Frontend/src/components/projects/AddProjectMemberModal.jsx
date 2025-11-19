import React, { useState } from 'react';
import { X, AlertCircle, UserPlus, ChevronDown } from 'lucide-react';
import { addProjectMember } from '../../api/projectApi';
import { PROJECT_ROLE, getProjectRoleName } from '../../constants/config';

function AddProjectMemberModal({ projectId, workspaceMembers, existingMemberIds, onClose, onMemberAdded }) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [showMemberDropdown, setShowMemberDropdown] = useState(false);
  const [showRoleDropdown, setShowRoleDropdown] = useState(false);
  
  const [formData, setFormData] = useState({
    projectId: projectId,
    userId: '',
    role: PROJECT_ROLE.MEMBER
  });

  // Filter out members who are already in the project
  const availableMembers = workspaceMembers.filter(
    member => !existingMemberIds.includes(member.userId)
  );

  const roleOptions = [
    { value: PROJECT_ROLE.VIEWER, label: getProjectRoleName(PROJECT_ROLE.VIEWER) },
    { value: PROJECT_ROLE.MEMBER, label: getProjectRoleName(PROJECT_ROLE.MEMBER) },
    { value: PROJECT_ROLE.ADMIN, label: getProjectRoleName(PROJECT_ROLE.ADMIN) }
  ];

  const getSelectedMemberName = () => {
    if (!formData.userId) return 'Choose a member...';
    const member = availableMembers.find(m => m.userId === formData.userId);
    return member ? `${member.userName} (${member.userEmail})` : 'Choose a member...';
  };

  const handleMemberSelect = (userId) => {
    setFormData(prev => ({ ...prev, userId }));
    setShowMemberDropdown(false);
  };

  const handleRoleSelect = (role) => {
    setFormData(prev => ({ ...prev, role: parseInt(role) }));
    setShowRoleDropdown(false);
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ 
      ...prev, 
      [name]: name === 'role' ? parseInt(value) : value 
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    
    if (!formData.userId) {
      setError('Please select a member');
      return;
    }

    setIsSubmitting(true);

    try {
      const result = await addProjectMember(formData);

      if (result.success) {
        onMemberAdded();
        onClose();
      } else {
        setError(result.message || 'Failed to add member to project');
      }
    } catch (err) {
      setError('An error occurred while adding member');
      console.error('Error adding project member:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div 
      className="fixed inset-0 backdrop-blur-md flex items-center justify-center p-4"
      style={{ zIndex: 9999 }}
      onClick={onClose}
    >
      <div 
        className="bg-white rounded-lg shadow-2xl max-w-md w-full relative overflow-visible"
        style={{ zIndex: 10000 }}
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-blue-100 rounded-lg">
              <UserPlus size={24} className="text-blue-600" />
            </div>
            <h2 className="text-2xl font-bold text-gray-900">Add Member</h2>
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

          {availableMembers.length === 0 ? (
            <div className="text-center py-4">
              <p className="text-gray-600">All workspace members are already in this project.</p>
            </div>
          ) : (
            <>
              <div className="relative">
                <label htmlFor="userId" className="block text-sm font-medium text-gray-700 mb-2">
                  Select Member <span className="text-red-500">*</span>
                </label>
                <div className="relative">
                  <button
                    type="button"
                    onClick={() => setShowMemberDropdown(!showMemberDropdown)}
                    disabled={isSubmitting}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white text-left flex items-center justify-between disabled:opacity-50"
                  >
                    <span className={formData.userId ? 'text-gray-900' : 'text-gray-500'}>
                      {getSelectedMemberName()}
                    </span>
                    <ChevronDown size={16} />
                  </button>
                  {showMemberDropdown && (
                    <div className="absolute z-50 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg max-h-60 overflow-y-auto">
                      {availableMembers.map(member => (
                        <button
                          key={member.userId}
                          type="button"
                          onClick={() => handleMemberSelect(member.userId)}
                          className="w-full px-4 py-2 text-left hover:bg-blue-50 first:rounded-t-lg last:rounded-b-lg"
                        >
                          {member.userName} ({member.userEmail})
                        </button>
                      ))}
                    </div>
                  )}
                </div>
              </div>

              <div className="relative">
                <label htmlFor="role" className="block text-sm font-medium text-gray-700 mb-2">
                  Project Role <span className="text-red-500">*</span>
                </label>
                <div className="relative">
                  <button
                    type="button"
                    onClick={() => setShowRoleDropdown(!showRoleDropdown)}
                    disabled={isSubmitting}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white text-left flex items-center justify-between disabled:opacity-50"
                  >
                    <span>{getProjectRoleName(formData.role)}</span>
                    <ChevronDown size={16} />
                  </button>
                  {showRoleDropdown && (
                    <div className="absolute z-50 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg">
                      {roleOptions.map(option => (
                        <button
                          key={option.value}
                          type="button"
                          onClick={() => handleRoleSelect(option.value)}
                          className="w-full px-4 py-2 text-left hover:bg-blue-50 first:rounded-t-lg last:rounded-b-lg"
                        >
                          {option.label}
                        </button>
                      ))}
                    </div>
                  )}
                </div>
              </div>

              <div className="flex justify-end gap-3 pt-4">
                <button
                  type="button"
                  onClick={onClose}
                  className="px-4 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors"
                  disabled={isSubmitting}
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50"
                  disabled={isSubmitting}
                >
                  {isSubmitting ? 'Adding...' : 'Add Member'}
                </button>
              </div>
            </>
          )}
        </form>
      </div>
    </div>
  );
}

export default AddProjectMemberModal;
