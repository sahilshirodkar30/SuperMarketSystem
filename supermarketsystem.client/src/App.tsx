import React, { useState } from 'react';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import SignupPage from './pages/SignupPage';
import HomePage from './pages/HomePage';
import Categories from './pages/Categories';
import Navbar from './pages/Navbar';
import ProductAll from './pages/ProductAll'
import Employees from './pages/Employees'
function App() {
    const [isAuthenticated, setIsAuthenticated] = useState(!!localStorage.getItem('token'));

    const handleLogout = () => {
        setIsAuthenticated(false);
    };

    return (
        <Router>
          <Navbar isAuthenticated={isAuthenticated} onLogout={handleLogout} />
            <Routes>
                <Route path="/" element={<HomePage />} />
                <Route path="/signup" element={<SignupPage />} />
                
                {isAuthenticated && (
                    <>
                        <Route path="/Categories" element={<Categories />} />
                        <Route path="/ProductAll" element={<ProductAll />} />
                        <Route path="/Employees" element={<Employees />} />
                    </>
                )}
            </Routes>
        </Router>
    );

   
}

export default App;