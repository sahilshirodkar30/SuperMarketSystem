import React, { useState } from 'react';
import api from '../pages/api';

function SignupPage() {
    const [formData, setFormData] = useState({ username: '', email: '', password: '' });

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        console.log('Submitting form:', formData);
        try {
            const response = await api.post('Authentication/SignUp', formData);
            console.log('Registration response:', response.data);
           // alert('Registration successful. You can now log in.');
            setFormData({ username: '', email: '', password: '' }); // Clear form
        } catch (error) {
            console.error('Registration error:', error);
            const message = 'Registration failed. Please try again.';
            alert(`Registration failed: ${message}`);
        }
    };

    return (
        <div>
            <h2>Register</h2>
            <form
                onSubmit={handleSubmit}
                style={{
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    justifyContent: 'center',
                    gap: '10px',
                }}
            >
                <label htmlFor="username">Username</label>
                <input
                    id="username"
                    type="text"
                    name="username"
                    placeholder="Username"
                    value={formData.username}
                    onChange={handleChange}
                    required
                    style={{ width: '363px' }}
                />
                <label htmlFor="email">Email</label>
                <input
                    id="email"
                    type="email"
                    name="email"
                    placeholder="Email"
                    value={formData.email}
                    onChange={handleChange}
                    required
                    style={{ width: '363px' }}
                />
                <label htmlFor="password">Password</label>
                <input
                    id="password"
                    type="password"
                    name="password"
                    placeholder="Password"
                    value={formData.password}
                    onChange={handleChange}
                    required
                    style={{ width: '363px' }}
                />
                <button type="submit" style={{ width: '363px' }}>Register</button>
            </form>
        </div>
    );
}

export default SignupPage;
