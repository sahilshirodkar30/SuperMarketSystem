import React, { useState } from 'react';
import LoginPage from '../pages/LoginPage';
import SignupPage from '../pages/SignupPage';
function HomePage() {
    const [isRegister, setIsRegister] = useState(false);

    const toggleForm = () => {
        setIsRegister(!isRegister);
    };
    return (
        <div>
            <h1>Student Management System</h1>
            {isRegister ? (
                <div>
                    <SignupPage />
                    <p>
                        Already have an account?{' '}
                        <button onClick={toggleForm}>Login</button>
                    </p>
                </div>
            ) : (
                <div>
                    <LoginPage />
                    <p>
                        Don't have an account?{' '}
                        <button onClick={toggleForm}>Register</button>
                    </p>
                </div>
            )}
        </div>
    );
}

export default HomePage;
