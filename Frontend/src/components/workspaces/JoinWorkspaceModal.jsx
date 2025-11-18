import React, { useState } from 'react';
import { X, AlertCircle, Users } from 'lucide-react';
import { joinWorkspace } from '../../api/workspaceApi';

function JoinWorkspaceModal({ onClose, onWorkspaceJoined }) {
  const [code, setCode] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    
    if (!code.trim()) {
      setError('Invite code is required');
      return;
    }

    setIsSubmitting(true);

    try {
      const result = await joinWorkspace(code.trim());

      if (result.success) {
        onWorkspaceJoined();
        onClose();
      } else {
        setError(result.message || 'Invalid invite code');
      }
    } catch (err) {
      setError('An error occurred while joining workspace');
      console.error('Error joining workspace:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div 
      className="fixed inset-0 backdrop-blur-md flex items-center justify-center z-50 p-4"
      onClick={onClose}
    >
      <div 
        className="bg-white rounded-lg shadow-2xl max-w-md w-full"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-indigo-100 rounded-lg">
              <Users size={24} className="text-indigo-600" />
            </div>
            <h2 className="text-2xl font-bold text-gray-900">Join Workspace</h2>
          </div>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {error && (
            <div className="bg-red-50 border border-red-200 rounded-lg p-3">
              <div className="flex items-center gap-2 text-red-800">
                <AlertCircle size={20} />
                <span className="text-sm font-medium">{error}</span>
              </div>
            </div>
          )}

          <div>
            <label htmlFor="code" className="block text-sm font-medium text-gray-700 mb-2">
              Invite Code <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              id="code"
              value={code}
              onChange={(e) => setCode(e.target.value)}
              placeholder="Enter workspace invite code"
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent font-mono text-lg tracking-wider"
              disabled={isSubmitting}
              autoFocus
            />
            <p className="mt-2 text-sm text-gray-500">
              Ask your workspace admin for an invite code
            </p>
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
              className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors disabled:opacity-50"
              disabled={isSubmitting}
            >
              {isSubmitting ? 'Joining...' : 'Join Workspace'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default JoinWorkspaceModal;