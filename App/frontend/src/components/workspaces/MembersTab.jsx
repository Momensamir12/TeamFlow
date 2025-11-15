import React, { useState } from 'react';
import { Users, Mail, Shield, MoreVertical, UserMinus, Copy, Check, ChevronDown } from 'lucide-react';
import { getWorkspaceRoleName, getRoleColor, canManageWorkspace, WORKSPACE_ROLE } from '../../constants/config';
import { updateMemberRole, removeMember, getWorkspaceCode } from '../../api/workspaceApi';
import InviteByEmailModal from './InviteByEmailModal';

function MembersTab({ workspaceId, members, userRole, onMembersUpdated }) {
  const [showInvite, setShowInvite] = useState(false);
  const [showEmailInvite, setShowEmailInvite] = useState(false);
  const [inviteCode, setInviteCode] = useState('');
  const [loadingCode, setLoadingCode] = useState(false);
  const [copied, setCopied] = useState(false);
  const [processingMember, setProcessingMember] = useState(null);
  const [roleDropdowns, setRoleDropdowns] = useState({});

  // Safely get current user ID
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
  
  const canManage = canManageWorkspace(userRole);

  const handleShowInvite = async () => {
    setShowInvite(true);
    setLoadingCode(true);
    
    try {
      const result = await getWorkspaceCode(workspaceId);
      if (result.success) {
        setInviteCode(result.data.code || result.data);
      }
    } catch (error) {
      console.error('Error loading invite code:', error);
    } finally {
      setLoadingCode(false);
    }
  };

  const handleCopyCode = () => {
    navigator.clipboard.writeText(inviteCode);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const toggleRoleDropdown = (memberId) => {
    setRoleDropdowns(prev => ({
      ...prev,
      [memberId]: !prev[memberId]
    }));
  };

  const handleRoleChange = async (memberId, newRole) => {
    setProcessingMember(memberId);
    setRoleDropdowns(prev => ({ ...prev, [memberId]: false }));
    
    try {
      const result = await updateMemberRole(workspaceId, memberId, newRole);
      if (result.success) {
        onMembersUpdated();
      }
    } catch (error) {
      console.error('Error updating role:', error);
    } finally {
      setProcessingMember(null);
    }
  };

  const roleOptions = [
    { value: WORKSPACE_ROLE.VIEWER, label: 'Viewer' },
    { value: WORKSPACE_ROLE.MEMBER, label: 'Member' },
    { value: WORKSPACE_ROLE.ADMIN, label: 'Admin' }
  ];

  const handleRemoveMember = async (memberId) => {
    if (!confirm('Are you sure you want to remove this member?')) return;
    
    setProcessingMember(memberId);
    
    try {
      const result = await removeMember(workspaceId, memberId);
      if (result.success) {
        onMembersUpdated();
      }
    } catch (error) {
      console.error('Error removing member:', error);
    } finally {
      setProcessingMember(null);
    }
  };

  return (
    <div>
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Members</h2>
          <p className="text-gray-600 mt-1">
            {members.length} member{members.length !== 1 ? 's' : ''}
          </p>
        </div>
        
        {canManage && (
          <div className="flex gap-2">
            <button
              onClick={() => setShowEmailInvite(true)}
              className="flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
            >
              <Mail size={20} />
              Invite by Email
            </button>
            <button
              onClick={handleShowInvite}
              className="flex items-center gap-2 px-4 py-2 bg-white border border-indigo-600 text-indigo-600 rounded-lg hover:bg-indigo-50 transition-colors"
            >
              <Users size={20} />
              Invite Code
            </button>
          </div>
        )}
      </div>

      {/* Invite Code Section */}
      {showInvite && canManage && (
        <div className="bg-indigo-50 border border-indigo-200 rounded-lg p-4 mb-6">
          <h3 className="text-sm font-semibold text-indigo-900 mb-2">Workspace Invite Code</h3>
          {loadingCode ? (
            <div className="text-sm text-indigo-700">Loading code...</div>
          ) : (
            <div className="flex items-center gap-3">
              <code className="flex-1 px-4 py-2 bg-white rounded border border-indigo-300 font-mono text-lg tracking-wider text-indigo-900">
                {inviteCode}
              </code>
              <button
                onClick={handleCopyCode}
                className="flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
              >
                {copied ? <Check size={20} /> : <Copy size={20} />}
                {copied ? 'Copied!' : 'Copy'}
              </button>
            </div>
          )}
          <p className="text-xs text-indigo-700 mt-2">
            Share this code with people you want to invite to this workspace
          </p>
        </div>
      )}

      {/* Members List */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
        <div className="divide-y divide-gray-200">
          {members.map(member => (
            <div key={member.id} className="p-4 hover:bg-gray-50 transition-colors">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 bg-indigo-100 rounded-full flex items-center justify-center">
                    <Users size={20} className="text-indigo-600" />
                  </div>
                  <div>
                    <h4 className="font-medium text-gray-900">{member.userName}</h4>
                    <div className="flex items-center gap-2 text-sm text-gray-500">
                      <Mail size={14} />
                      {member.userEmail}
                    </div>
                  </div>
                </div>

                <div className="flex items-center gap-3">
                  {canManage && member.userId !== currentUserId ? (
                    <>
                      <div className="relative">
                        <button
                          type="button"
                          onClick={() => toggleRoleDropdown(member.userId)}
                          disabled={processingMember === member.userId}
                          className={`px-3 py-1 rounded border font-medium text-sm ${getRoleColor(member.role)} disabled:opacity-50 flex items-center gap-1`}
                        >
                          <span>{getWorkspaceRoleName(member.role)}</span>
                          <ChevronDown size={14} />
                        </button>
                        {roleDropdowns[member.userId] && (
                          <div className="absolute z-50 right-0 mt-1 bg-white border border-gray-300 rounded-lg shadow-lg min-w-[120px]">
                            {roleOptions.map(option => (
                              <button
                                key={option.value}
                                type="button"
                                onClick={() => handleRoleChange(member.userId, option.value)}
                                className="w-full px-4 py-2 text-left hover:bg-indigo-50 first:rounded-t-lg last:rounded-b-lg text-sm"
                              >
                                {option.label}
                              </button>
                            ))}
                          </div>
                        )}
                      </div>
                      <button
                        onClick={() => handleRemoveMember(member.userId)}
                        disabled={processingMember === member.userId}
                        className="p-2 text-red-600 hover:bg-red-50 rounded transition-colors disabled:opacity-50"
                        title="Remove member"
                      >
                        <UserMinus size={18} />
                      </button>
                    </>
                  ) : (
                    <span className={`px-3 py-1 rounded text-sm font-medium ${getRoleColor(member.role)}`}>
                      {getWorkspaceRoleName(member.role)}
                    </span>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {members.length === 0 && (
        <div className="text-center py-12 bg-white rounded-lg shadow">
          <Users size={48} className="mx-auto text-gray-400 mb-4" />
          <h3 className="text-lg font-semibold text-gray-900 mb-2">No members yet</h3>
          <p className="text-gray-600">Invite people to collaborate in this workspace</p>
        </div>
      )}

      {/* Email Invitation Modal */}
      {showEmailInvite && (
        <InviteByEmailModal
          workspaceId={workspaceId}
          onClose={() => setShowEmailInvite(false)}
          onSuccess={() => {
            onMembersUpdated();
          }}
        />
      )}
    </div>
  );
}

export default MembersTab;