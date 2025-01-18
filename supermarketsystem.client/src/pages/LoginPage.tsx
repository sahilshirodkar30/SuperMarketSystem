import React, { useState } from 'react';
import api from '../pages/api';
import { useNavigate } from 'react-router-dom';
import './LoginPage.css';

function LoginPage() {
    const [formData, setFormData] = useState({ username: '', password: '' });
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setLoading(true);
        try {
            const response = await api.post('Authentication/login', {
                UserName: formData.username,
                Password: formData.password,
            });

            if (response.data?.token) {
                localStorage.setItem('token', response.data.token);
                alert('Login successful!');
                navigate('/ProductAll');
            } else {
                alert('Login failed: Token not received.');
            }
        } catch (error) {
            console.error('Login error:', error);
            alert('Login failed. Please check your credentials.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div>
            <h2>Login</h2>
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
                <button type="submit" disabled={loading} style={{ width: '363px' }}>
                    {loading ? 'Logging in...' : 'Login'}
                </button>
            </form>
        </div>
    );
}

export default LoginPage;
