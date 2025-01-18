import axios from 'axios';

const API_BASE_URL = 'https://localhost:7093/api/'; // Update to match your backend URL

const api = axios.create({
    baseURL: API_BASE_URL,
});

// Attach token from localStorage to every request
api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// Optional: Add error handling for expired tokens
api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            localStorage.removeItem('token');
            window.location.href = '/login'; // Redirect to login on 401
        }
        return Promise.reject(error);
    }
);

export default api;
