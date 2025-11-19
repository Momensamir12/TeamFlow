import React, { useState, useEffect, useCallback } from 'react';
import { AlertCircle } from 'lucide-react';
import CommentList from './CommentList';
import AddComment from './AddComment';
import { taskCommentApi } from '../../api/taskCommentApi';

function CommentSection({ taskId, currentUserId }) {
  const [comments, setComments] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const fetchComments = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const result = await taskCommentApi.getTaskComments(taskId);
      if (result.success) {
        setComments(result.data || []);
      } else {
        setError(result.message || 'Failed to load comments');
      }
    } catch {
      setError('An error occurred while loading comments');
    } finally {
      setLoading(false);
    }
  }, [taskId]);

  useEffect(() => {
    fetchComments();
  }, [fetchComments]);

  const handleAddComment = async (content) => {
    setLoading(true);
    setError('');
    try {
      const result = await taskCommentApi.createComment(taskId, content);
      if (result.success) {
        setComments([...comments, result.data]);
      } else {
        setError(result.message || 'Failed to post comment');
      }
    } catch {
      setError('An error occurred while posting the comment');
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteComment = async (commentId) => {
    if (!window.confirm('Are you sure you want to delete this comment?')) {
      return;
    }

    setLoading(true);
    setError('');
    try {
      const result = await taskCommentApi.deleteComment(taskId, commentId);
      if (result.success) {
        setComments(comments.filter(c => c.id !== commentId));
      } else {
        setError(result.message || 'Failed to delete comment');
      }
    } catch {
      setError('An error occurred while deleting the comment');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="space-y-4 border-t border-gray-200 pt-6">
      <h3 className="text-lg font-semibold text-gray-900">Comments</h3>

      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-3">
          <div className="flex items-center gap-2 text-red-800">
            <AlertCircle size={18} />
            <span className="text-sm font-medium">{error}</span>
          </div>
        </div>
      )}

      <CommentList
        comments={comments}
        currentUserId={currentUserId}
        onDeleteComment={handleDeleteComment}
        loading={loading}
      />

      <AddComment
        onSubmit={handleAddComment}
        loading={loading}
      />
    </div>
  );
}

export default CommentSection;
