import { useState } from 'react';
import { useAuth } from './auth.tsx';
import { userClient } from './baseUrl.ts';

export function LoginPage() {
    const { login } = useAuth();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const [isAlternateUI, setIsAlternateUI] = useState(true);
    
    // Registration modal state
    const [showRegisterModal, setShowRegisterModal] = useState(false);
    const [registerUserId, setRegisterUserId] = useState('');
    const [registerName, setRegisterName] = useState('');
    const [registerEmail, setRegisterEmail] = useState('');
    const [registerPassword, setRegisterPassword] = useState('');
    const [registerConfirmPassword, setRegisterConfirmPassword] = useState('');
    const [registerError, setRegisterError] = useState('');
    const [registerLoading, setRegisterLoading] = useState(false);

    const handleLogin = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        try {
            await login({ email, password });
            // navigation is handled inside the hook
        } catch {
            setError("Invalid email or password, or account is inactive");
        } finally {
            setLoading(false);
        }
    };

    const handleRegister = async (e: React.FormEvent) => {
        e.preventDefault();
        setRegisterError('');

        if (registerPassword !== registerConfirmPassword) {
            setRegisterError("Passwords do not match");
            return;
        }

        if (registerPassword.length < 6) {
            setRegisterError("Password must be at least 6 characters long");
            return;
        }

        setRegisterLoading(true);

        try {
            await userClient.createUser({
                userId: registerUserId.trim() || undefined,
                name: registerName.trim(),
                email: registerEmail.trim(),
                password: registerPassword,
            });
            
            // Close modal and clear form
            setShowRegisterModal(false);
            setRegisterUserId('');
            setRegisterName('');
            setRegisterEmail('');
            setRegisterPassword('');
            setRegisterConfirmPassword('');
            setRegisterError('');
            
            // Optionally auto-login the user
            setEmail(registerEmail);
            setPassword(registerPassword);
            
        } catch {
            setRegisterError("Registration failed. User ID or email may already be in use.");
        } finally {
            setRegisterLoading(false);
        }
    };

    function ToggleUI() {
        setIsAlternateUI(prev => !prev);
    }

    // Original UI
    const originalUI = (
        <>
            <div className="navbar bg-base-100 shadow-sm">
                <a className="btn btn-ghost text-xl" onClick={ToggleUI}>Toggle UI Changes</a>
            </div>
            <div className="min-h-screen flex items-center justify-center bg-base-200">
                <div className="card w-96 bg-base-100 shadow-xl">
                    <div className="card-body">
                        <h2 className="card-title text-center text-2xl mb-4">The Film Journal</h2>

                        <form onSubmit={handleLogin}>
                            <div className="form-control">
                                <label htmlFor="email" className="label">
                                    <span className="label-text">Email</span>
                                </label>
                                <input
                                    id="email"
                                    type="email"
                                    placeholder="email@example.com"
                                    className="input input-bordered"
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    required/>
                            </div>

                            <div className="form-control mt-4">
                                <label htmlFor="password" className="label">
                                    <span className="label-text">Password</span>
                                </label>
                                <input
                                    id="password"
                                    type="password"
                                    placeholder="••••••••"
                                    className="input input-bordered"
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    required/>
                            </div>

                            {error && (
                                <div className="alert alert-error mt-4">
                                    <span>{error}</span>
                                </div>
                            )}

                            <div className="form-control mt-6">
                                <button type="submit" className="btn btn-primary" disabled={loading}>
                                    {loading ? <span className="loading loading-spinner"/> : "Login"}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </>
    );

    // Alternative UI - completely different design
    const alternateUI = (
        <div style={{
            minHeight: '100vh',
            background: '#0a0a0a',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            padding: '20px'
        }}>
            <div style={{ position: 'fixed', top: '24px', right: '24px' }}>
                <button 
                    onClick={ToggleUI} 
                    style={{
                        background: '#1a1a1a',
                        color: '#e50914',
                        padding: '10px 20px',
                        borderRadius: '25px',
                        border: '1px solid #333',
                        fontWeight: '600',
                        cursor: 'pointer',
                        boxShadow: '0 4px 12px rgba(0,0,0,0.5)'
                    }}
                >
                    Switch to Classic View
                </button>
            </div>
            
            <div style={{
                background: '#1a1a1a',
                borderRadius: '20px',
                boxShadow: '0 20px 60px rgba(0,0,0,0.8)',
                padding: '50px 40px',
                width: '100%',
                maxWidth: '420px',
                textAlign: 'center',
                border: '1px solid #2a2a2a'
            }}>
                <div style={{ marginBottom: '40px' }}>
                    <div style={{ fontSize: '60px', marginBottom: '20px' }}>🌬️</div>
                    <h1 style={{ fontSize: '32px', fontWeight: '700', color: '#fff', margin: '0 0 10px 0' }}>
                        The Film Journal
                    </h1>
                    <p style={{ color: '#888', fontSize: '16px', margin: 0 }}>
                        Welcome back!
                    </p>
                </div>

                <form onSubmit={handleLogin} style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
                    <input
                        id="email"
                        type="email"
                        placeholder="Email address"
                        style={{
                            width: '100%',
                            padding: '15px 20px',
                            border: '2px solid #2a2a2a',
                            borderRadius: '12px',
                            fontSize: '16px',
                            outline: 'none',
                            transition: 'border-color 0.2s',
                            boxSizing: 'border-box',
                            backgroundColor: '#0a0a0a',
                            color: '#fff'
                        }}
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        onFocus={(e) => e.target.style.borderColor = '#e50914'}
                        onBlur={(e) => e.target.style.borderColor = '#2a2a2a'}
                        required
                    />

                    <input
                        id="password"
                        type="password"
                        placeholder="Password"
                        style={{
                            width: '100%',
                            padding: '15px 20px',
                            border: '2px solid #2a2a2a',
                            borderRadius: '12px',
                            fontSize: '16px',
                            outline: 'none',
                            transition: 'border-color 0.2s',
                            boxSizing: 'border-box',
                            backgroundColor: '#0a0a0a',
                            color: '#fff'
                        }}
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        onFocus={(e) => e.target.style.borderColor = '#e50914'}
                        onBlur={(e) => e.target.style.borderColor = '#2a2a2a'}
                        required
                    />

                    {error && (
                        <div style={{
                            background: '#2a1a1a',
                            borderLeft: '4px solid #e50914',
                            padding: '15px',
                            borderRadius: '8px',
                            textAlign: 'left'
                        }}>
                            <p style={{ color: '#ff6b6b', margin: 0, fontSize: '14px' }}>{error}</p>
                        </div>
                    )}

                    <button 
                        type="submit" 
                        style={{
                            width: '100%',
                            background: '#e50914',
                            color: 'white',
                            padding: '16px',
                            borderRadius: '12px',
                            border: 'none',
                            fontSize: '18px',
                            fontWeight: '700',
                            cursor: loading ? 'not-allowed' : 'pointer',
                            opacity: loading ? 0.6 : 1,
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            boxShadow: '0 4px 15px rgba(229, 9, 20, 0.4)',
                            transition: 'all 0.2s'
                        }}
                        disabled={loading}
                        onMouseEnter={(e) => !loading && (e.currentTarget.style.background = '#f40612')}
                        onMouseLeave={(e) => !loading && (e.currentTarget.style.background = '#e50914')}
                    >
                        {loading ? (
                            <span style={{
                                width: '24px',
                                height: '24px',
                                border: '3px solid white',
                                borderTop: '3px solid transparent',
                                borderRadius: '50%',
                                display: 'inline-block',
                                animation: 'spin 1s linear infinite'
                            }} />
                        ) : (
                            'Sign In'
                        )}
                    </button>

                    <div style={{ marginTop: '10px', color: '#888', fontSize: '14px' }}>
                        Don't have an account?{' '}
                        <a 
                            onClick={() => setShowRegisterModal(true)} 
                            style={{ color: '#e50914', cursor: 'pointer', textDecoration: 'none', fontWeight: '600' }}
                        >
                            Register here
                        </a>
                    </div>
                </form>
            </div>

            {/* Registration Modal */}
            {showRegisterModal && (
                <div 
                    style={{
                        position: 'fixed',
                        top: 0,
                        left: 0,
                        right: 0,
                        bottom: 0,
                        background: 'rgba(0, 0, 0, 0.85)',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        zIndex: 1000,
                        padding: '20px'
                    }}
                    onClick={() => setShowRegisterModal(false)}
                >
                    <div 
                        style={{
                            background: '#1a1a1a',
                            borderRadius: '20px',
                            boxShadow: '0 20px 60px rgba(0,0,0,0.9)',
                            padding: '50px 40px',
                            width: '100%',
                            maxWidth: '420px',
                            textAlign: 'center',
                            border: '1px solid #2a2a2a',
                            position: 'relative',
                            maxHeight: '90vh',
                            overflowY: 'auto'
                        }}
                        onClick={(e) => e.stopPropagation()}
                    >
                        {/* Close button */}
                        <button
                            onClick={() => setShowRegisterModal(false)}
                            style={{
                                position: 'absolute',
                                top: '20px',
                                right: '20px',
                                background: 'transparent',
                                border: 'none',
                                color: '#888',
                                fontSize: '28px',
                                cursor: 'pointer',
                                padding: '0',
                                width: '32px',
                                height: '32px',
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center',
                                borderRadius: '50%',
                                transition: 'all 0.2s'
                            }}
                            onMouseEnter={(e) => {
                                e.currentTarget.style.background = '#2a2a2a';
                                e.currentTarget.style.color = '#fff';
                            }}
                            onMouseLeave={(e) => {
                                e.currentTarget.style.background = 'transparent';
                                e.currentTarget.style.color = '#888';
                            }}
                        >
                            ×
                        </button>

                        <div style={{ marginBottom: '40px' }}>
                            <div style={{ fontSize: '60px', marginBottom: '20px' }}>🎬</div>
                            <h1 style={{ fontSize: '32px', fontWeight: '700', color: '#fff', margin: '0 0 10px 0' }}>
                                Join The Film Journal
                            </h1>
                            <p style={{ color: '#888', fontSize: '16px', margin: 0 }}>
                                Create your account
                            </p>
                        </div>

                        <form onSubmit={handleRegister} style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
                            <input
                                type="text"
                                placeholder="User ID (optional)"
                                style={{
                                    width: '100%',
                                    padding: '15px 20px',
                                    border: '2px solid #2a2a2a',
                                    borderRadius: '12px',
                                    fontSize: '16px',
                                    outline: 'none',
                                    transition: 'border-color 0.2s',
                                    boxSizing: 'border-box',
                                    backgroundColor: '#0a0a0a',
                                    color: '#fff'
                                }}
                                value={registerUserId}
                                onChange={(e) => setRegisterUserId(e.target.value)}
                                onFocus={(e) => e.target.style.borderColor = '#e50914'}
                                onBlur={(e) => e.target.style.borderColor = '#2a2a2a'}
                            />

                            <input
                                type="text"
                                placeholder="Full name"
                                style={{
                                    width: '100%',
                                    padding: '15px 20px',
                                    border: '2px solid #2a2a2a',
                                    borderRadius: '12px',
                                    fontSize: '16px',
                                    outline: 'none',
                                    transition: 'border-color 0.2s',
                                    boxSizing: 'border-box',
                                    backgroundColor: '#0a0a0a',
                                    color: '#fff'
                                }}
                                value={registerName}
                                onChange={(e) => setRegisterName(e.target.value)}
                                onFocus={(e) => e.target.style.borderColor = '#e50914'}
                                onBlur={(e) => e.target.style.borderColor = '#2a2a2a'}
                                required
                            />

                            <input
                                type="email"
                                id="email"
                                placeholder="Email address"
                                style={{
                                    width: '100%',
                                    padding: '15px 20px',
                                    border: '2px solid #2a2a2a',
                                    borderRadius: '12px',
                                    fontSize: '16px',
                                    outline: 'none',
                                    transition: 'border-color 0.2s',
                                    boxSizing: 'border-box',
                                    backgroundColor: '#0a0a0a',
                                    color: '#fff'
                                }}
                                value={registerEmail}
                                onChange={(e) => setRegisterEmail(e.target.value)}
                                onFocus={(e) => e.target.style.borderColor = '#e50914'}
                                onBlur={(e) => e.target.style.borderColor = '#2a2a2a'}
                                required
                            />

                            <input
                                type="password"
                                id={"password"}
                                placeholder="Password"
                                style={{
                                    width: '100%',
                                    padding: '15px 20px',
                                    border: '2px solid #2a2a2a',
                                    borderRadius: '12px',
                                    fontSize: '16px',
                                    outline: 'none',
                                    transition: 'border-color 0.2s',
                                    boxSizing: 'border-box',
                                    backgroundColor: '#0a0a0a',
                                    color: '#fff'
                                }}
                                value={registerPassword}
                                onChange={(e) => setRegisterPassword(e.target.value)}
                                onFocus={(e) => e.target.style.borderColor = '#e50914'}
                                onBlur={(e) => e.target.style.borderColor = '#2a2a2a'}
                                required
                            />

                            <input
                                type="password"
                                placeholder="Confirm password"
                                style={{
                                    width: '100%',
                                    padding: '15px 20px',
                                    border: '2px solid #2a2a2a',
                                    borderRadius: '12px',
                                    fontSize: '16px',
                                    outline: 'none',
                                    transition: 'border-color 0.2s',
                                    boxSizing: 'border-box',
                                    backgroundColor: '#0a0a0a',
                                    color: '#fff'
                                }}
                                value={registerConfirmPassword}
                                onChange={(e) => setRegisterConfirmPassword(e.target.value)}
                                onFocus={(e) => e.target.style.borderColor = '#e50914'}
                                onBlur={(e) => e.target.style.borderColor = '#2a2a2a'}
                                required
                            />

                            {registerError && (
                                <div style={{
                                    padding: '12px',
                                    background: 'rgba(229, 9, 20, 0.1)',
                                    border: '1px solid #e50914',
                                    borderRadius: '8px',
                                    color: '#e50914',
                                    fontSize: '14px'
                                }}>
                                    {registerError}
                                </div>
                            )}

                            <button
                                type="submit"
                                disabled={registerLoading}
                                style={{
                                    width: '100%',
                                    padding: '15px',
                                    background: registerLoading ? '#666' : '#e50914',
                                    color: '#fff',
                                    border: 'none',
                                    borderRadius: '12px',
                                    fontSize: '16px',
                                    fontWeight: '600',
                                    cursor: registerLoading ? 'not-allowed' : 'pointer',
                                    transition: 'all 0.2s',
                                    boxShadow: '0 4px 12px rgba(229, 9, 20, 0.3)',
                                    display: 'flex',
                                    alignItems: 'center',
                                    justifyContent: 'center'
                                }}
                                onMouseEnter={(e) => !registerLoading && (e.currentTarget.style.background = '#f40612')}
                                onMouseLeave={(e) => !registerLoading && (e.currentTarget.style.background = '#e50914')}
                            >
                                {registerLoading ? (
                                    <span style={{
                                        width: '24px',
                                        height: '24px',
                                        border: '3px solid white',
                                        borderTop: '3px solid transparent',
                                        borderRadius: '50%',
                                        display: 'inline-block',
                                        animation: 'spin 1s linear infinite'
                                    }} />
                                ) : (
                                    'Create Account'
                                )}
                            </button>

                            <div style={{ marginTop: '10px', color: '#888', fontSize: '14px' }}>
                                Already have an account?{' '}
                                <a 
                                    onClick={() => setShowRegisterModal(false)} 
                                    style={{ color: '#e50914', cursor: 'pointer', textDecoration: 'none', fontWeight: '600' }}
                                >
                                    Login here
                                </a>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            <style>{`
                @keyframes spin {
                    from { transform: rotate(0deg); }
                    to { transform: rotate(360deg); }
                }
            `}</style>
        </div>
    );

    return isAlternateUI ? alternateUI : originalUI;
}