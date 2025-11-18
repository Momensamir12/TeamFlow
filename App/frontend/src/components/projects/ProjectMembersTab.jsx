import React, { useState } from 'react';
import { Users, UserPlus, Shield, MoreVertical, UserMinus } from 'lucide-react';
import { getProjectRoleName, getRoleColor, canEditProject, PROJECT_ROLE } from '../../constants/config';
import { removeProjectMember } from '../../api/projectApi';
import AddProjectMemberModal from './AddProjectMemberModal';
import Dropdown, { DropdownItem } from '../common/Dropdown';

function ProjectMembersTab({ projectId, members, workspaceMembers, userRole, onMembersUpdated }) {
  const [showAddModal, setShowAddModal] = useState(false);
  const [processingMember, setProcessingMember] = useState(null);
  const [showMenu, setShowMenu] = useState(null);

  const canManage = canEditProject(userRole);

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

  const handleRemoveMember = async (memberUserId) => {
    if (!window.confirm('Are you sure you want to remove this member from the project?')) {
      return;
    }

    setProcessingMember(memberUserId);
    setShowMenu(null);

    try {
      const result = await removeProjectMember(projectId, memberUserId);
      
      if (result.success) {
        onMembersUpdated();
      } else {
        alert(result.message || 'Failed to remove member');
      }
    } catch (error) {
      console.error('Error removing member:', error);
      alert('An error occurred while removing member');
    } finally {
      setProcessingMember(null);
    }
  };

  const handleMemberAdded = () => {
    onMembersUpdated();
  };

  // Get existing member IDs for filtering in AddMemberModal
  const existingMemberIds = members.map(m => m.userId);

  return (
    <div>
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Project Members</h2>
          <p className="text-gray-600 mt-1">
            {members.length} member{members.length !== 1 ? 's' : ''}
          </p>
        </div>
        
        {canManage && (
          <button
            onClick={() => setShowAddModal(true)}
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            <UserPlus size={20} />
            Add Member
          </button>
        )}
      </div>

      {/* Members List */}
      {members.length === 0 ? (
        <div className="text-center py-12 bg-white rounded-lg shadow">
          <Users size={48} className="mx-auto text-gray-400 mb-4" />
          <h3 className="text-lg font-semibold text-gray-900 mb-2">
            No members yet
          </h3>
          <p className="text-gray-600 mb-4">
            {canManage
              ? 'Add workspace members to this project'
              : 'No members have been added to this project yet'
            }
          </p>
          {canManage && (
            <button
              onClick={() => setShowAddModal(true)}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
            >
              Add Member
            </button>
          )}
        </div>
      ) : (
        <div className="bg-white rounded-lg shadow">
          <div className="divide-y divide-gray-200">
            {members.map(member => (
              <div 
                key={member.id}
                className="flex items-center justify-between p-4 hover:bg-gray-50 transition-colors"
              >
                <div className="flex items-center gap-4 flex-1">
                  <div className="w-10 h-10 rounded-full bg-blue-100 flex items-center justify-center">
                    <Users size={20} className="text-blue-600" />
                  </div>
                  <div>
                    <h3 className="font-medium text-gray-900">{member.userName}</h3>
                    <p className="text-sm text-gray-500">{member.userEmail}</p>
                  </div>
                </div>

                <div className="flex items-center gap-3">
                  <span className={`flex items-center gap-1 px-3 py-1 text-sm font-medium rounded ${getRoleColor(member.role)}`}>
                    <Shield size={14} />
                    {getProjectRoleName(member.role)}
                  </span>

                  {canManage && member.userId !== currentUserId && (
                    <Dropdown
                      isOpen={showMenu === member.userId}
                      onClose={() => setShowMenu(null)}
                      trigger={
                        <button
                          onClick={() => setShowMenu(showMenu === member.userId ? null : member.userId)}
                          className="p-1 hover:bg-gray-200 rounded transition-colors"
                          disabled={processingMember === member.userId}
                        >
                          <MoreVertical size={20} className="text-gray-600" />
                        </button>
                      }
                    >
                      <DropdownItem
                        onClick={() => handleRemoveMember(member.userId)}
                        className="text-red-600 hover:bg-red-50"
                      >
                        <div className="flex items-center gap-2">
                          <UserMinus size={16} />
                          Remove from Project
                        </div>
                      </DropdownItem>
                    </Dropdown>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Add Member Modal */}
      {showAddModal && (
        <AddProjectMemberModal
          projectId={projectId}
          workspaceMembers={workspaceMembers}
          existingMemberIds={existingMemberIds}
          onClose={() => setShowAddModal(false)}
          onMemberAdded={handleMemberAdded}
        />
      )}
    </div>
  );
}

export default ProjectMembersTab;
