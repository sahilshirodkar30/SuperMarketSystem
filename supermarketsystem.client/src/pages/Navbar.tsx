import React from 'react';
import { Link, useNavigate } from 'react-router-dom';

interface NavbarProps {
    isAuthenticated: boolean; // Type for isAuthenticated
    onLogout: () => void;     // Type for onLogout
}

const Navbar: React.FC<NavbarProps> = ({ isAuthenticated, onLogout }) => {
    const navigate = useNavigate();

    const handleLogout = () => {
        localStorage.removeItem('token'); // Clear token
        onLogout();
        navigate('/'); // Redirect to home
    };

    return (
        <nav style={{ padding: '10px', backgroundColor: '#f5f5f5' }}>
            <ul style={{ display: 'flex', listStyle: 'none', gap: '20px' }}>
                {isAuthenticated ? (
                    <>
                        <li>
                            <Link to="/Categories">Categories</Link>
                        </li>
                        <li>
                            <Link to="/ProductAll">ProductAll</Link>
                        </li>
                        <li>
                            <button onClick={handleLogout} style={{ cursor: 'pointer' }}>
                                Logout
                            </button>
                        </li>
                    </>
                ) : (
                    <>

                    </>
                )}
            </ul>
        </nav>
    );
};

export default Navbar;
