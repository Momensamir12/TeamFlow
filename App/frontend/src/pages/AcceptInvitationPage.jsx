import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { CheckCircle, XCircle, Loader, Mail, Users } from 'lucide-react';
import { validateInvitation, acceptInvitation } from '../api/workspaceApi';

function AcceptInvitationPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [status, setStatus] = useState('loading'); // loading, ready, accepting, success, error
  const [invitation, setInvitation] = useState(null);
  const [error, setError] = useState('');
  const [currentToken, setCurrentToken] = useState(null);

  useEffect(() => {
    // Get token from URL or sessionStorage
    let tokenToUse = searchParams.get('token');
    
    if (!tokenToUse) {
      tokenToUse = sessionStorage.getItem('invitationToken');
      
      if (tokenToUse) {
        // Update URL with token
        navigate(`/accept-invitation?token=${tokenToUse}`, { replace: true });
        return;
      }
    }
    
    if (!tokenToUse) {
      setStatus('error');
      setError('Invalid invitation link');
      return;
    }
    
    setCurrentToken(tokenToUse);

    const checkAuth = () => {
      const accessToken = sessionStorage.getItem('accessToken');
      const emailVerified = sessionStorage.getItem('emailVerified') === 'true';
      
      if (!accessToken) {
        // Not logged in, redirect to login with return URL
        sessionStorage.setItem('invitationToken', tokenToUse);
        navigate(`/login?redirect=/accept-invitation&token=${tokenToUse}`);
        return false;
      }
      
      if (!emailVerified) {
        // Not verified, redirect to verification page with saved token
        sessionStorage.setItem('invitationToken', tokenToUse);
        navigate('/verify-email-required');
        return false;
      }
      
      return true;
    };

    const loadInvitation = async () => {
      if (!checkAuth()) return;

      try {
        const result = await validateInvitation(tokenToUse);
        
        if (result.success) {
          const data = result.data;
          
          if (data.isAccepted) {
            setError('This invitation has already been accepted');
            setStatus('error');
            return;
          }
          
          if (data.isExpired) {
            setError('This invitation has expired');
            setStatus('error');
            return;
          }
          
          setInvitation(data);
          setStatus('ready');
        } else {
          setError(result.message || 'Invalid invitation');
          setStatus('error');
        }
      } catch {
        setError('Failed to load invitation details');
        setStatus('error');
      }
    };

    loadInvitation();
  }, [searchParams, navigate]);

  const handleAccept = async () => {
    if (!currentToken) {
      setError('No invitation token available');
      setStatus('error');
      return;
    }
    
    setStatus('accepting');
    setError('');

    try {
      const result = await acceptInvitation(currentToken);
      
      if (result.success) {
        setStatus('success');
        sessionStorage.removeItem('invitationToken');
        
        // Redirect to dashboard after 2 seconds
        setTimeout(() => {
          navigate('/dashboard');
        }, 2000);
      } else {
        setError(result.message || 'Failed to accept invitation');
        setStatus('error');
      }
    } catch {
      setError('An error occurred while accepting the invitation');
      setStatus('error');
    }
  };

  const handleDecline = () => {
    sessionStorage.removeItem('invitationToken');
    navigate('/dashboard');
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-500 via-purple-500 to-pink-500 flex items-center justify-center p-4">
      <div className="bg-white rounded-2xl shadow-2xl p-8 max-w-md w-full">
        <div className="flex flex-col items-center">
          {status === 'loading' && (
            <>
              <div className="w-20 h-20 bg-indigo-100 rounded-full flex items-center justify-center mb-6">
                <Loader className="text-indigo-600 animate-spin" size={40} />
              </div>
              <h1 className="text-2xl font-bold text-gray-900 mb-2">Loading Invitation</h1>
              <p className="text-gray-600 text-center">
                Please wait while we verify your invitation...
              </p>
            </>
          )}

          {status === 'ready' && invitation && (
            <>
              <div className="w-20 h-20 bg-indigo-100 rounded-full flex items-center justify-center mb-6">
                <Users className="text-indigo-600" size={40} />
              </div>
              <h1 className="text-2xl font-bold text-gray-900 mb-2">Workspace Invitation</h1>
              <p className="text-gray-600 text-center mb-6">
                You've been invited to join <strong>{invitation.workspaceName}</strong>
              </p>
              
              <div className="w-full mb-6 p-4 bg-indigo-50 rounded-lg">
                <p className="text-sm text-gray-700 mb-1">Your Role:</p>
                <p className="text-lg font-semibold text-indigo-600">{invitation.roleName}</p>
              </div>

              <div className="w-full space-y-3">
                <button
                  onClick={handleAccept}
                  className="w-full px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors font-medium"
                >
                  Accept Invitation
                </button>
                <button
                  onClick={handleDecline}
                  className="w-full px-6 py-3 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors font-medium"
                >
                  Decline
                </button>
              </div>
            </>
          )}

          {status === 'accepting' && (
            <>
              <div className="w-20 h-20 bg-indigo-100 rounded-full flex items-center justify-center mb-6">
                <Loader className="text-indigo-600 animate-spin" size={40} />
              </div>
              <h1 className="text-2xl font-bold text-gray-900 mb-2">Accepting Invitation</h1>
              <p className="text-gray-600 text-center">
                Please wait...
              </p>
            </>
          )}

          {status === 'success' && (
            <>
              <div className="w-20 h-20 bg-green-100 rounded-full flex items-center justify-center mb-6">
                <CheckCircle className="text-green-600" size={40} />
              </div>
              <h1 className="text-2xl font-bold text-gray-900 mb-2">Welcome!</h1>
              <p className="text-gray-600 text-center mb-4">
                You've successfully joined the workspace.
              </p>
              <p className="text-sm text-gray-500">
                Redirecting to dashboard...
              </p>
            </>
          )}

          {status === 'error' && (
            <>
              <div className="w-20 h-20 bg-red-100 rounded-full flex items-center justify-center mb-6">
                <XCircle className="text-red-600" size={40} />
              </div>
              <h1 className="text-2xl font-bold text-gray-900 mb-2">Unable to Accept</h1>
              <p className="text-gray-600 text-center mb-6">
                {error}
              </p>
              <button
                onClick={() => navigate('/dashboard')}
                className="w-full px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors font-medium"
              >
                Go to Dashboard
              </button>
            </>
          )}
        </div>
      </div>
    </div>
  );
}

export default AcceptInvitationPage;
